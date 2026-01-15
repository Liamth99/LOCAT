using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LOCAT.Analyzer._010;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotUseReservedExceptionsAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new (
        "LOCAT010",
        "Do not use reserved exception types",
        messageFormat: "`{0}` is a reserved exception type",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public static readonly ImmutableArray<string> ReservedExceptionTypes =
    [
        "System.Exception",
        "System.SystemException",
        "System.ApplicationException",
        "System.NullReferenceException",
        "System.IndexOutOfRangeException",
        "System.AccessViolationException",
        "System.StackOverflowException",
        "System.OutOfMemoryException",
        "System.Runtime.InteropServices.COMException",
        "System.Runtime.InteropServices.SEHException",
        "System.ExecutionEngineException",
    ];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.ObjectCreationExpression);
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax creation)
            return;

        if(creation.NewKeyword.IsMissing)
            return;

        var typeInfo    = context.SemanticModel.GetTypeInfo(creation, context.CancellationToken);
        var createdType = typeInfo.Type as INamedTypeSymbol;

        if (createdType is null)
            return;

        if(!ReservedExceptionTypes.Contains(createdType.ToDisplayString()))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, creation.GetLocation(), createdType.Name));
    }
}