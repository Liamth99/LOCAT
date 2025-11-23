using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LOCAT.Analyzer._008;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RestrictedVariableNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new (
        "LOCAT008",
        "Variable name is not allowed according to the class’s naming policy",
        messageFormat: "Variable name '{0}' violates naming restrictions for class '{1}' whose regex restrictions are {2}",
        "Naming",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(startContext =>
        {
            var configOptionsProvider = startContext.Options.AnalyzerConfigOptionsProvider;

            startContext.RegisterSyntaxNodeAction(c => AnalyzeVariableDeclaration(c, configOptionsProvider),            SyntaxKind.VariableDeclarator);
            startContext.RegisterSyntaxNodeAction(c => AnalyzeMemberDeclaration(c, configOptionsProvider),              SyntaxKind.PropertyDeclaration);
            startContext.RegisterSyntaxNodeAction(c => AnalyzeMemberDeclaration(c, configOptionsProvider),              SyntaxKind.FieldDeclaration);
            startContext.RegisterSyntaxNodeAction(c => AnalyzeMemberDeclaration(c, configOptionsProvider),              SyntaxKind.Parameter);
            startContext.RegisterSyntaxNodeAction(c => AnalyzeSimpleLambdaDeclaration(c, configOptionsProvider),        SyntaxKind.SimpleLambdaExpression);
            startContext.RegisterSyntaxNodeAction(c => AnalyzeParenthesizedLambdaDeclaration(c, configOptionsProvider), SyntaxKind.ParenthesizedLambdaExpression);
            startContext.RegisterSyntaxNodeAction(c => AnalyzeForeachVariableDeclaration(c, configOptionsProvider),     SyntaxKind.ForEachStatement);
        });

    }

    private readonly Regex _isOptionRegex = new (@"dotnet_diagnostic.LOCAT008.(?<class>.*)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

    private void ReportIfRestricted(SyntaxNodeAnalysisContext context, AnalyzerConfigOptions config, string className, SyntaxToken varIdent)
    {
        foreach (var configOption in config.Keys)
        {
            var match = _isOptionRegex.Match(configOption);

            var value = match.Groups["class"].Value;

            if(!match.Success ||
               (!value.Equals(className, StringComparison.OrdinalIgnoreCase) && !value.Equals("default")) ||
               !config.TryGetValue(configOption, out var restRegex)
              )
                continue;

            if(Regex.IsMatch(varIdent.Text, restRegex, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
                context.ReportDiagnostic(Diagnostic.Create(Rule, varIdent.GetLocation(), varIdent.Text, className, restRegex));
        }
    }

    private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context, AnalyzerConfigOptionsProvider configOptionsProvider)
    {
        var variableDeclarator = (VariableDeclaratorSyntax)context.Node;

        var variableDeclaration = variableDeclarator.Parent as VariableDeclarationSyntax;
        if (variableDeclaration is null)
            return;

        var typeInfo = context.SemanticModel.GetTypeInfo(variableDeclaration.Type).Type;
        if (typeInfo is null)
            return;

        string className = typeInfo.Name;

        ReportIfRestricted(context, configOptionsProvider.GetOptions(context.FilterTree), className, variableDeclarator.Identifier);
    }

    private void AnalyzeMemberDeclaration(SyntaxNodeAnalysisContext context, AnalyzerConfigOptionsProvider configOptionsProvider)
    {

        switch (context.Node)
        {
            case PropertyDeclarationSyntax propertyDeclaration:
            {
                var className = context.SemanticModel.GetTypeInfo(propertyDeclaration.Type).Type?.Name;

                if (className is null)
                    return;

                ReportIfRestricted(context, configOptionsProvider.GetOptions(context.FilterTree), className, propertyDeclaration.Identifier);
                break;
            }

            case ParameterSyntax parameterSyntax:
            {
                if (parameterSyntax.Type is null)
                    return;

                var className = context.SemanticModel.GetTypeInfo(parameterSyntax.Type).Type?.Name;

                if (className is null)
                    return;

                ReportIfRestricted(context, configOptionsProvider.GetOptions(context.FilterTree), className, parameterSyntax.Identifier);

                break;
            }

            default:
                return;
        }
    }

    private void AnalyzeSimpleLambdaDeclaration(SyntaxNodeAnalysisContext context, AnalyzerConfigOptionsProvider configOptionsProvider)
    {
        var lambdaDeclarator = (SimpleLambdaExpressionSyntax)context.Node;

        var config = configOptionsProvider.GetOptions(context.FilterTree);

        var lambdaParameter = lambdaDeclarator.Parameter;
        var parameterSymbol = context.SemanticModel.GetDeclaredSymbol(lambdaParameter);

        CreateParameterDiagnostic(context, parameterSymbol, lambdaParameter, config);
    }

    private void AnalyzeParenthesizedLambdaDeclaration(SyntaxNodeAnalysisContext context, AnalyzerConfigOptionsProvider configOptionsProvider)
    {
        var lambdaDeclarator = (ParenthesizedLambdaExpressionSyntax)context.Node;

        var config = configOptionsProvider.GetOptions(context.FilterTree);

        foreach (var parameter in lambdaDeclarator.ParameterList.Parameters)
        {
            var parameterSymbol = context.SemanticModel.GetDeclaredSymbol(parameter);

            CreateParameterDiagnostic(context, parameterSymbol, parameter, config);
        }
    }

    private void CreateParameterDiagnostic(SyntaxNodeAnalysisContext context, ISymbol? symbol, ParameterSyntax syntax, AnalyzerConfigOptions config)
    {
        var    typeInfo   = (symbol as IParameterSymbol)?.Type;

        if (typeInfo is null)
            return;

        string className = typeInfo.Name;

        ReportIfRestricted(context, config, className, syntax.Identifier);
    }

    private void AnalyzeForeachVariableDeclaration(SyntaxNodeAnalysisContext context, AnalyzerConfigOptionsProvider configOptionsProvider)
    {
        var variableDeclarator = (ForEachStatementSyntax)context.Node;

        var symbolInfo = context.SemanticModel.GetDeclaredSymbol(variableDeclarator);

        string className = symbolInfo!.Type.Name;

        ReportIfRestricted(context, configOptionsProvider.GetOptions(context.FilterTree), className, variableDeclarator.Identifier);
    }
}