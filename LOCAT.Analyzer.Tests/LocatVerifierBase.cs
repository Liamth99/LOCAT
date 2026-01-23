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
/// Provides utility methods for verifying diagnostics produced by Roslyn analyzers during unit testing.
/// </summary>
/// <typeparam name="TAnalyzer">
/// The type of analyzer to test. Must derive from <see cref="DiagnosticAnalyzer"/> and have a parameterless constructor.
/// </typeparam>
public abstract class LocatVerifierBase<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <summary>
    /// Verifies the diagnostics produced by the provided analyzer on the given source code.
    /// </summary>
    /// <param name="source">The source code to analyze.</param>
    /// <param name="expected">The expected diagnostics to be produced by the analyzer. Can be null if no diagnostics are expected.</param>
    /// <param name="additionalReferences">A collection of additional assembly references to include in the analysis. Can be null if no additional references are needed.</param>
    /// <param name="editorConfigContent">The content of an editor configuration file to use during the analysis. Can be null if no configuration is needed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task VerifyAnalyzerAsync(
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

/// <summary>
/// Provides a framework for unit testing Roslyn analyzers and code fixes.
/// This class allows verification of diagnostics and fixes produced by a specific analyzer and code fix provider.
/// </summary>
/// <typeparam name="TAnalyzer">
/// The type of the diagnostic analyzer to test. Must derive from <see cref="DiagnosticAnalyzer"/> and have a parameterless constructor.
/// </typeparam>
/// <typeparam name="TCodeFixProvider">
/// The type of the code fix provider to test. Must derive from <see cref="CodeFixProvider"/> and have a parameterless constructor.
/// </typeparam>
public abstract class LocatVerifierBase<TAnalyzer, TCodeFixProvider> : LocatVerifierBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFixProvider  : CodeFixProvider,    new()
{
    /// <summary>
    /// Verifies that the provided source code produces the expected diagnostics and that the specified code fix resolves the diagnostics correctly.
    /// </summary>
    /// <param name="source">The source code to analyze and fix.</param>
    /// <param name="fix">The expected source code after applying the code fix.</param>
    /// <param name="expected">The expected diagnostics to be produced by the analyzer. Can be null if no diagnostics are expected.</param>
    /// <param name="additionalReferences">A collection of additional assembly references to include in the analysis. Can be null if no additional references are required.</param>
    /// <param name="editorConfigContent">The content of an editor configuration file to use during the analysis. Can be null if no configuration is needed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous verification operation.</returns>
    public static async Task VerifyCodeFixAsync(
        string                         source,
        string                         fix,
        IEnumerable<DiagnosticResult>? expected             = null,
        IEnumerable<Assembly>?         additionalReferences = null,
        string?                        editorConfigContent  = null,
        CancellationToken              cancellationToken    = default)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFixProvider, DefaultVerifier>
        {
            TestCode            = source,
            FixedCode           = fix,
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