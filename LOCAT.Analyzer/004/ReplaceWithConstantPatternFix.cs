using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LOCAT.Analyzer._004;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReplaceWithConstantPatternFix)), Shared]
public class ReplaceWithConstantPatternFix : CodeFixProvider
{

    public sealed override ImmutableArray<string> FixableDiagnosticIds => ["LOCAT004"];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var parseOptions = (CSharpParseOptions)context.Document.Project.ParseOptions!;
        if (parseOptions.LanguageVersion < LanguageVersion.CSharp9)
        {
            return; // Language feature requires C# 9+
        }

        var root       = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));
        if (diagnostic is null)
            return;

        var span = diagnostic.Location.SourceSpan;
        var binary = root!.FindToken(span.Start).Parent!.AncestorsAndSelf()
                         .OfType<BinaryExpressionSyntax>().FirstOrDefault();

        if (binary is null)
            return;

        bool isEquals = binary.IsKind(SyntaxKind.EqualsExpression);

        // Determine left and right constant/variable roles

        var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

        var leftIsConst  = UseConstantPatternAnalyzer.IsConstantExpression(binary.Left,  model!, context.CancellationToken);
        var rightIsConst = UseConstantPatternAnalyzer.IsConstantExpression(binary.Right, model!, context.CancellationToken);

        // Must be exactly one constant
        if (leftIsConst == rightIsConst)
            return;

        var constExpr = leftIsConst  ? binary.Left  : binary.Right;
        var valueExpr = leftIsConst  ? binary.Right : binary.Left;

        var replacement = isEquals ? $"is {(leftIsConst ? binary.Left.ToString() : binary.Right.ToString())}" : $"is not {(leftIsConst ? binary.Left.ToString() : binary.Right.ToString())}";
        var title       = $"Use '{replacement}' pattern";

        var action = CodeAction.Create(
            title,
            c => ReplaceWithIsPattern(context.Document, root, binary, valueExpr, constExpr, isEquals, c),
            equivalenceKey: title);

        context.RegisterCodeFix(action, diagnostic);
    }

    private static Task<Document> ReplaceWithIsPattern(
        Document               document,
        SyntaxNode             root,
        BinaryExpressionSyntax binaryExpression,
        ExpressionSyntax       valueExpr,
        ExpressionSyntax       constExpr,
        bool                   isEqualsExpression,
        CancellationToken      cancellationToken)
    {
        PatternSyntax constantPattern = SyntaxFactory.ConstantPattern(constExpr.WithoutTrivia());

        PatternSyntax finalPattern = isEqualsExpression
            ? constantPattern
            : SyntaxFactory.UnaryPattern(
                SyntaxFactory.Token(SyntaxKind.NotKeyword),
                constantPattern);

        var isPatternExpression =
            SyntaxFactory.IsPatternExpression(
                valueExpr.WithoutTrivia(),
                finalPattern);

        // Preserve original trivia
        var newExpression = isPatternExpression
                           .WithLeadingTrivia(binaryExpression.GetLeadingTrivia())
                           .WithTrailingTrivia(binaryExpression.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(binaryExpression, newExpression);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}