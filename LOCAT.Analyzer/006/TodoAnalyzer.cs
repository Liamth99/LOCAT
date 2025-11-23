using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LOCAT.Analyzer._006;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TodoAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new (
        "LOCAT006",
        "TODO Comments should be fixed, or be added as issues on the projects repo",
        messageFormat: "Comment contains `{0}`. Address or create issue: `{1}`.",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(startContext =>
        {
            var config = startContext.Options.AnalyzerConfigOptionsProvider;

            startContext.RegisterSyntaxTreeAction(c => AnalyzeSyntaxTree(c, config));
        });
    }

    private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context, AnalyzerConfigOptionsProvider config)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var trivia in root.DescendantTrivia())
        {
            if(trivia.Token.Parent is DocumentationCommentTriviaSyntax || trivia.Token.Parent?.AncestorsAndSelf().OfType<DocumentationCommentTriviaSyntax>().Any() is true)
                continue; // ignore xml docs

            if (!trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && !trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                continue;

            config.GetOptions(context.Tree).TryGetValue("dotnet_diagnostic.LOCAT006.todo_regex", out var configValue);

            var todoRegex = new Regex(configValue ?? @"\b(todo|fixme|bug|temp)\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

            var match = todoRegex.Match(trivia.ToString());

            if (!match.Success)
                continue;

            var comment = Regex.Replace(trivia.ToString(), @"\s+|$", " ").Trim();

            if (comment.StartsWith("//"))
            {
                if(comment[2] is '~')
                    continue; // ignored comment
                comment = comment.Substring(2).Trim();
            }

            else if (comment.StartsWith("/*") && comment.EndsWith("*/"))
            {
                if(comment[2] is '~')
                    continue; // ignored comment
                comment = comment.Substring(2, comment.Length - 4).Trim();
            }

            var diagnostic = Diagnostic.Create(Rule, trivia.GetLocation(), match.Groups[1], comment);
            context.ReportDiagnostic(diagnostic);
        }
    }
}