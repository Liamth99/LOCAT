using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LOCAT.Analyzer._011;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RegexTimeoutAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new (
        "LOCAT011",
        "Regex should include a timeout",
        messageFormat: "Regex is created without a match timeout",
        "Performance",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeRegexCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeGeneratedRegexAttribute, SyntaxKind.Attribute);
    }

    private static void AnalyzeRegexCreation(SyntaxNodeAnalysisContext context)
    {
        var creation = (ObjectCreationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(creation).Symbol is not IMethodSymbol ctor)
            return;

        if (ctor.ContainingType.ToDisplayString() is not "System.Text.RegularExpressions.Regex")
            return;

        if (ctor.Parameters.Any(p => p.Type.ToDisplayString() is "System.TimeSpan"))
            return;

        if (creation.ArgumentList is null || HasNonBacktrackingFlag(creation.ArgumentList.Arguments, ctor, context.SemanticModel))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, creation.GetLocation()));
    }

    private static void AnalyzeGeneratedRegexAttribute(SyntaxNodeAnalysisContext context)
    {
        var attribute = (AttributeSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol ctor)
            return;

        if (ctor.ContainingType.ToDisplayString() is not "System.Text.RegularExpressions.GeneratedRegexAttribute")
            return;

        if (HasNonBacktrackingInAttribute(attribute, ctor, context.SemanticModel))
            return;

        bool hasTimeoutArg = false;

        if (attribute.ArgumentList is not null)
        {
            var args = attribute.ArgumentList.Arguments;

            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];

                // Named argument
                if (arg.NameEquals is not null)
                {
                    if (arg.NameEquals.Name.Identifier.Text is "matchTimeoutMilliseconds")
                    {
                        hasTimeoutArg = true;
                        break;
                    }
                }
                else // Positional argument
                {
                    if (i < ctor.Parameters.Length && ctor.Parameters[i].Name is "matchTimeoutMilliseconds")
                    {
                        hasTimeoutArg = true;
                        break;
                    }
                }
            }
        }

        if (hasTimeoutArg)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, attribute.GetLocation()));
    }

    private static bool HasNonBacktrackingFlag(SeparatedSyntaxList<ArgumentSyntax> arguments, IMethodSymbol ctor, SemanticModel model)
    {
        for (int i = 0; i < arguments.Count; i++)
        {
            var param = ctor.Parameters[i];

            if (param.Type.ToDisplayString() is not "System.Text.RegularExpressions.RegexOptions")
                continue;

            var arg = arguments[i];

            var constant = model.GetConstantValue(arg.Expression);

            if (!constant.HasValue)
                continue;

            var value = (RegexOptions)constant.Value!;

            return value.HasFlag((RegexOptions)1024);
        }

        return false;
    }

    private static bool HasNonBacktrackingInAttribute(AttributeSyntax attribute, IMethodSymbol ctor, SemanticModel model)
    {
        if (attribute.ArgumentList is null)
            return false;

        var args = attribute.ArgumentList.Arguments;

        for (int i = 0; i < args.Count; i++)
        {
            var param = ctor.Parameters[i];

            if (param.Type.ToDisplayString() is not "System.Text.RegularExpressions.RegexOptions")
                continue;

            var constant = model.GetConstantValue(args[i].Expression);

            if (!constant.HasValue)
                continue;

            var value = (RegexOptions)constant.Value!;

            return value.HasFlag((RegexOptions)1024);
        }

        return false;
    }
}