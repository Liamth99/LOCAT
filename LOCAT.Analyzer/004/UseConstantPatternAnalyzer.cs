using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LOCAT.Analyzer._004;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseConstantPatternAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new (
        "LOCAT004",
        "Use 'is' pattern for constant checks",
        "Consider using '{0} {2}' instead of '{1} {2}'",
        "Style",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Using 'is' patterns for constant is more explicit and consistent with modern C# style.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binary = (BinaryExpressionSyntax)context.Node;

        // Skip expression trees
        var enclosingLambda = binary.FirstAncestorOrSelf<LambdaExpressionSyntax>();
        if (enclosingLambda is not null)
        {
            var lambdaType = context.SemanticModel.GetTypeInfo(enclosingLambda, context.CancellationToken).ConvertedType;
            if (lambdaType is INamedTypeSymbol named &&
                named.Name == "Expression" &&
                named.ContainingNamespace?.ToDisplayString() == "System.Linq.Expressions")
            {
                return;
            }
        }

        var model = context.SemanticModel;

        var leftIsConst  = IsConstantExpression(binary.Left,  model, context.CancellationToken);
        var rightIsConst = IsConstantExpression(binary.Right, model, context.CancellationToken);

        // must have exactly one side constant
        if (leftIsConst == rightIsConst)
            return;

        var exprSide = leftIsConst ? binary.Right : binary.Left;

        var typeInfo = context.SemanticModel.GetTypeInfo(exprSide);
        if (typeInfo.Type == null)
            return;

        // (Optional) Skip comparing two constant numeric values if operand is not a variable
        // e.g., "if (5 == 5)" – avoid flagging nonsense cases
        if (exprSide is LiteralExpressionSyntax)
            return;

        bool isEquals = binary.IsKind(SyntaxKind.EqualsExpression);

        var diagnostic = Diagnostic.Create(
            Rule,
            binary.GetLocation(),
            isEquals ? "is" : "is not",
            isEquals ? "==" : "!=",
            leftIsConst ? binary.Left.ToString() : binary.Right.ToString());

        context.ReportDiagnostic(diagnostic);
    }

    internal static bool IsConstantExpression(ExpressionSyntax node, SemanticModel model, CancellationToken token)
    {
        if (node is LiteralExpressionSyntax)
            return true;

        // handles -5, +3, etc.
        if (node is PrefixUnaryExpressionSyntax unary && unary.Operand is LiteralExpressionSyntax)
            return true;

        // 2. Enum members or const variables like: x == MyEnum.Value, x == ConstVar
        var symbol = model.GetSymbolInfo(node, token).Symbol;

        if (symbol is IFieldSymbol fieldSymbol)
        {
            // const fields OR enum members (which are implicitly constants)
            if (fieldSymbol.IsConst || fieldSymbol.ContainingType?.TypeKind == TypeKind.Enum)
                return true;
        }

        if (symbol is ILocalSymbol localSymbol)
        {
            // const local variables
            if (localSymbol.IsConst)
                return true;
        }

        return false;
    }
}