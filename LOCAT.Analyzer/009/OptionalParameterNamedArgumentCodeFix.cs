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

        var node = root.FindNode(diagnosticSpan);
        var argument = node.FirstAncestorOrSelf<ArgumentSyntax>();
        var attributeArgument = node.FirstAncestorOrSelf<AttributeArgumentSyntax>();

        if (argument == null && attributeArgument == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use named argument",
                c => AddNamedArgumentAsync(context.Document, node, c),
                nameof(OptionalParameterNamedArgumentCodeFix)),
            diagnostic);
    }

    private static async Task<Document> AddNamedArgumentAsync(
        Document document,
        SyntaxNode argumentNode,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (root == null || semanticModel == null)
            return document;

        // Determine if we are dealing with a standard argument or an attribute argument
        var (parentList, parentContainer) = argumentNode switch
        {
            ArgumentSyntax arg => (arg.Parent!, arg.Parent!.Parent!),
            AttributeArgumentSyntax attrArg => (attrArg.Parent!, attrArg.Parent!.Parent!),
            _ => (null, null)
        };

        if (parentContainer == null)
            return document;

        var symbolInfo = semanticModel.GetSymbolInfo(parentContainer, cancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return document;

        var arguments = parentList switch
        {
            ArgumentListSyntax al => al.Arguments.Cast<SyntaxNode>().ToList(),
            AttributeArgumentListSyntax aal => aal.Arguments.Cast<SyntaxNode>().ToList(),
            _ => null
        };

        if (arguments == null)
            return document;

        var index = arguments.IndexOf(argumentNode);
        if (index < 0 || index >= methodSymbol.Parameters.Length)
            return document;

        var parameter = methodSymbol.Parameters[index];
        var nameColon = SyntaxFactory.NameColon(parameter.Name);

        var newNode = argumentNode switch
        {
            ArgumentSyntax arg => arg.WithNameColon(nameColon),
            AttributeArgumentSyntax attrArg => attrArg.WithNameColon(nameColon),
            _ => argumentNode
        };

        var newRoot = root.ReplaceNode(argumentNode, newNode.WithTriviaFrom(argumentNode));
        return document.WithSyntaxRoot(newRoot);
    }
}