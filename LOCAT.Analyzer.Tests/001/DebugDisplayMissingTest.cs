using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
        LOCAT.Analyzer._001.DebugDisplayMissingAnalyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace LOCAT.Analyzer.Tests._001;

public class DebugDisplayMissingTest
{
    [Fact]
    public async Task ClassWithNoDebugDisplayFileNamespace_AlertDiagnostic()
    {
        const string text = @"
                            namespace Company.Models;
                            public class {|#0:Class1|#0}
                            {
                            }
                            ";

        var expected = new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("'{0}' should have a DebugDisplay.")
            .WithArguments("Class1");

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task ClassWithNoDebugDisplayFileNamespace_AndExtraNamesSpaceStuffs_AlertDiagnostic()
    {
        const string text = @"
                            namespace Company.Models.Classes;
                            public class {|#0:Class1|#0}
                            {
                            }
                            ";

        var expected = new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("'{0}' should have a DebugDisplay.")
            .WithArguments("Class1");

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task ClassWithNoDebugDisplayScopeNamespace_AndExtraNamesSpaceStuffs_AlertDiagnostic()
    {
        const string text = @"
                            namespace Company.Models.Classes
                            {
                            public class {|#0:Class1|#0}
                            {
                            }
                            }
                            ";

        var expected = new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("'{0}' should have a DebugDisplay.")
            .WithArguments("Class1");

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task ClassWithNoDebugDisplayScopeNamespace_AlertDiagnostic()
    {
        const string text = @"
                            namespace Company.Models
                            {
                            public class {|#0:Class1|#0}
                            {
                            }
                            }
                            ";

        var expected = new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("'{0}' should have a DebugDisplay.")
            .WithArguments("Class1");

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task IgnoreInterfaces()
    {
        const string text = @"
                            namespace Company.Models
                            {
                            public interface IClass1
                            {
                            }
                            }
                            ";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task IgnoreAbstractClass()
    {
        const string text = @"
                            namespace Company.Models
                            {
                            public abstract class Class1
                            {
                            }
                            }
                            ";

        await Verifier.VerifyAnalyzerAsync(text);
    }
}