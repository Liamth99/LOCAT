using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace LOCAT.Analyzer._006;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SuppressTodoFix)), Shared]
public class SuppressTodoFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["LOCAT006"];

    // Disable batch fixing
    public override FixAllProvider GetFixAllProvider() => null!;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root is null)
            return;

        var diagnostic     = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var trivia = root.FindTrivia(diagnosticSpan.Start);

        if (trivia == default)
            return;

        context.RegisterCodeFix(
            Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                title: "Suppress TODO (add ~)",
                createChangedDocument: c => AddTildeAsync(context.Document, trivia, c),
                equivalenceKey: "AddTildeFix"),
            diagnostic);
    }

    private async Task<Document> AddTildeAsync(Document document, SyntaxTrivia trivia, CancellationToken cancellationToken)
    {
        var originalText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var span = trivia.Span;

        var commentText = originalText.GetSubText(span).ToString();

        string fixedComment = commentText;

        if (commentText.StartsWith("//", StringComparison.Ordinal))
            fixedComment = $"//~{commentText.Substring(2)}";

        else if (commentText.StartsWith("/*", StringComparison.Ordinal))
            fixedComment = $"/*~{commentText.Substring(2)}";

        var newText = originalText.Replace(span, fixedComment);
        return document.WithText(newText);
    }
}