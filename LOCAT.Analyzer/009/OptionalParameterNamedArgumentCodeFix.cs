using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LOCAT.Analyzer._009;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptionalParameterNamedArgumentCodeFix)), Shared]
public sealed class OptionalParameterNamedArgumentCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["LOCAT009"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var argument = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<ArgumentSyntax>();
        if (argument == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use named argument",
                c => AddNamedArgumentAsync(context.Document, argument, c),
                nameof(OptionalParameterNamedArgumentCodeFix)),
            diagnostic);
    }

    private static async Task<Document> AddNamedArgumentAsync(
        Document document,
        ArgumentSyntax argumentSyntax,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (root == null || semanticModel == null)
            return document;

        var argumentList = (ArgumentListSyntax)argumentSyntax.Parent!;
        var invocation = (InvocationExpressionSyntax)argumentList.Parent!;

        var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, invocation, cancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return document;

        var index = argumentList.Arguments.IndexOf(argumentSyntax);
        if (index < 0 || index >= methodSymbol.Parameters.Length)
            return document;

        var parameter = methodSymbol.Parameters[index];

        var newArgument = argumentSyntax
            .WithNameColon(SyntaxFactory.NameColon(parameter.Name))
            .WithTriviaFrom(argumentSyntax);

        var newRoot = root.ReplaceNode(argumentSyntax, newArgument);

        return document.WithSyntaxRoot(newRoot);
    }
}