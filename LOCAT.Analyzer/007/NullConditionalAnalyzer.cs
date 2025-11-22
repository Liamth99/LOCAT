using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using DiagnosticDescriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using LanguageNames = Microsoft.CodeAnalysis.LanguageNames;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace LOCAT.Analyzer._007;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullConditionalAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new (
        id: "LOCAT007",
        title: "Possible incorrect use of null-conditional operator",
        messageFormat: "Possible incorrect use of null-conditional operator here, detected as not null during compile time",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNullConditional, SyntaxKind.ConditionalAccessExpression);
    }

    private static void AnalyzeNullConditional(SyntaxNodeAnalysisContext context)
    {
        var conditional   = (ConditionalAccessExpressionSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (conditional.WhenNotNull is InvocationExpressionSyntax)
        {
            if (semanticModel.GetTypeInfo(conditional.Expression).Nullability.FlowState == NullableFlowState.NotNull)
                context.ReportDiagnostic(Diagnostic.Create(Rule, conditional.OperatorToken.GetLocation()));
        }

        else if (conditional.Expression is IdentifierNameSyntax)
        {
            if (semanticModel.GetTypeInfo(conditional.Expression).Nullability.FlowState == NullableFlowState.NotNull)
                context.ReportDiagnostic(Diagnostic.Create(Rule, conditional.OperatorToken.GetLocation()));
        }
    }
}