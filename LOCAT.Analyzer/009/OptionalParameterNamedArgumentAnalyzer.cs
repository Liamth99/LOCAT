using System.Collections.Immutable;
using System.Linq;
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
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeAttribute,  SyntaxKind.Attribute);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
            return;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        AnalyzeArguments(context, invocation.ArgumentList.Arguments, methodSymbol);
    }

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AttributeSyntax attribute)
            return;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol constructorSymbol)
            return;

        AnalyzeArguments(context, attribute.ArgumentList?.Arguments ?? default, constructorSymbol);
    }

    private static void AnalyzeArguments<T>(SyntaxNodeAnalysisContext context, SeparatedSyntaxList<T> arguments, IMethodSymbol methodSymbol) where T : SyntaxNode
    {
        if (arguments.Count == 0)
            return;

        for (int i = 0; i < arguments.Count; i++)
        {
            var arg = arguments[i];

            // Check for name: value (standard) or name = value (attribute properties)
            bool isNamed = arg switch
            {
                ArgumentSyntax a           => a.NameColon is not null,
                AttributeArgumentSyntax aa => aa.NameColon is not null || aa.NameEquals is not null,
                _                          => false
            };

            if (isNamed)
                continue;

            if (i >= methodSymbol.Parameters.Length)
                continue;

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

    // private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    // {
    //     if (context.Node is not InvocationExpressionSyntax invocation)
    //         return;
    //
    //     var semanticModel = context.SemanticModel;
    //
    //     var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, invocation, context.CancellationToken);
    //     if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
    //         return;
    //
    //     var arguments = invocation.ArgumentList.Arguments;
    //     if (arguments.Count is 0)
    //         return;
    //
    //     for (int i = 0; i < arguments.Count; i++)
    //     {
    //         var arg = arguments[i];
    //
    //         if (arg.NameColon is not null)
    //             continue;
    //
    //         if (i >= methodSymbol.Parameters.Length)
    //             continue; // params or error cases
    //
    //         var parameter = methodSymbol.Parameters[i];
    //
    //         if (parameter.IsOptional)
    //         {
    //             var diagnostic = Diagnostic.Create(
    //                 Rule,
    //                 arg.GetLocation(),
    //                 parameter.Name);
    //
    //             context.ReportDiagnostic(diagnostic);
    //         }
    //     }
    // }
}