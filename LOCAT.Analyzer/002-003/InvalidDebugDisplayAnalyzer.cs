using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LOCAT.Analyzer._002_003;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvalidDebugDisplayAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule1 = new (
        "LOCAT002",
        "Debug Display value is empty",
        "Debug Displays Should not be empty",
        category: "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Debug Displays Should not be empty."
    );
    
    private static readonly DiagnosticDescriptor Rule2 = new (
        "LOCAT003",
        "Debug Display value should not be constant",
        "Debug Displays Should contain member data",
        category: "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Debug Displays Should contain member data."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule1, Rule2];
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        var name = attributeSyntax.Name.ToString();

        if (name != "DebuggerDisplay" && name != "DebuggerDisplayAttribute")
            return;

        var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;

        if (attributeSymbol?.ContainingType.ToDisplayString() != "System.Diagnostics.DebuggerDisplayAttribute")
            return;

        var argument = attributeSyntax
                      .ArgumentList?
                      .Arguments
                      .FirstOrDefault(a => a.NameEquals?.Name.Identifier.ValueText is "Value" or null);

        if (argument?.Expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            var value = literal.Token.ValueText;

            if (string.IsNullOrWhiteSpace(value))
            {
                var diagnostic = Diagnostic.Create(Rule1, argument.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
            else if (!(value.Contains("{") && value.Contains("}")))
            {
                var diagnostic = Diagnostic.Create(Rule2, argument.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}