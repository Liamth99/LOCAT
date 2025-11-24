using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LOCAT.Analyzer._009;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OptionalParameterNamedArgumentAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new (
        "LOCAT009",
        "Optional parameters should be passed using named arguments",
        messageFormat: "The optional parameter ‘{0}’ is passed positionally. Use a named argument for clarity.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
            return;

        var semanticModel = context.SemanticModel;

        var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, invocation, context.CancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count is 0)
            return;

        for (int i = 0; i < arguments.Count; i++)
        {
            var arg = arguments[i];

            if (arg.NameColon is not null)
                continue;

            if (i >= methodSymbol.Parameters.Length)
                continue; // params or error cases

            var parameter = methodSymbol.Parameters[i];

            if (parameter.IsOptional)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    arg.GetLocation(),
                    parameter.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}