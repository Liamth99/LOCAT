using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace LOCAT.Analyzer.Tests;

/// <summary>
/// A static helper class to verify the behavior of diagnostics analyzers in tests.
/// </summary>
/// <typeparam name="TAnalyzer">The type of the diagnostic analyzer to verify. Must inherit from <see cref="DiagnosticAnalyzer"/>.</typeparam>
public static class LocatVerifier<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <summary>
    /// Verifies the diagnostics produced by the provided analyzer on the given source code.
    /// </summary>
    /// <param name="source">The source code to analyze.</param>
    /// <param name="expected">The expected diagnostics to be produced by the analyzer. Can be null if no diagnostics are expected.</param>
    /// <param name="additionalReferences">A collection of additional assembly references to include in the analysis. Can be null if no additional references are needed.</param>
    /// <param name="editorConfigContent">The content of an editor configuration file to use during the analysis. Can be null if no configuration is needed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public static async Task VerifyAnalyzerAsync(
        string                         source,
        IEnumerable<DiagnosticResult>? expected             = null,
        IEnumerable<Assembly>?         additionalReferences = null,
        string?                        editorConfigContent  = null,
        CancellationToken              cancellationToken    = default)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode            = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net70
        };

        if(additionalReferences is not null)
            test.TestState.AdditionalReferences.AddRange(additionalReferences.Select(x => MetadataReference.CreateFromFile(x.Location)));

        if(editorConfigContent is not null)
            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", editorConfigContent));

        if(expected is not null)
            test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync(cancellationToken);
    }
}

public static class LocatVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix  : CodeFixProvider,    new()
{

}