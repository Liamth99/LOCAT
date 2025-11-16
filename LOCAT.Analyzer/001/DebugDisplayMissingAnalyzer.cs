using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LOCAT.Analyzer._001;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DebugDisplayMissingAnalyzer : DiagnosticAnalyzer
{
    private readonly Regex _modelsRegex = new (".+.models($|.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(1000));

    private static readonly DiagnosticDescriptor Rule = new (
        "LOCAT001",
        Resources.LOCAT001Title,
        Resources.LOCAT001MessageFormat,
        category: "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Resources.LOCAT001Description
    );
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeOperation, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeOperation(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // ignore interfaces
        if(context.Node is InterfaceDeclarationSyntax)
            return;

        // Ignore abstract classes
        if(classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword)))
            return;

        var name = Utils.GetNamespaceOfClass(classDeclaration);

        if (name is null || !_modelsRegex.IsMatch(name))
        {
            return;
        }

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol is null)
        {
            return;
        }

        var hasDebuggerDisplay = classSymbol
                                .GetAttributes()
                                .Any(attr => attr.AttributeClass?.Name == "DebuggerDisplayAttribute");

        if (!hasDebuggerDisplay)
        {
            // The class is missing the DebuggerDisplayAttribute
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}