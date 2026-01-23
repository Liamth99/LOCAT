using System.Threading.Tasks;
using LOCAT.Analyzer._001;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace LOCAT.Analyzer.Tests._001;

public class DebugDisplayMissingTest : LocatVerifierBase<DebugDisplayMissingAnalyzer>
{
    DiagnosticResult Expected(int location, string argument)
    {
        return new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
              .WithLocation(location)
              .WithMessageFormat("'{0}' should have a DebugDisplay")
              .WithArguments(argument);
    }

    [Fact]
    public async Task ClassWithNoDebugDisplayFileNamespace_AlertDiagnostic()
    {
        const string text = @"
                            namespace Company.Models;
                            public class {|#0:Class1|#0}
                            {
                            }
                            ";

        await VerifyAnalyzerAsync(text, [Expected(0, "Class1")], cancellationToken: TestContext.Current.CancellationToken);
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

        await VerifyAnalyzerAsync(text, [Expected(0, "Class1")], cancellationToken: TestContext.Current.CancellationToken);
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

        await VerifyAnalyzerAsync(text, [Expected(0, "Class1")], cancellationToken: TestContext.Current.CancellationToken);
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

        await VerifyAnalyzerAsync(text, [Expected(0, "Class1")], cancellationToken: TestContext.Current.CancellationToken);
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

        await VerifyAnalyzerAsync(text, cancellationToken: TestContext.Current.CancellationToken);
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

        await VerifyAnalyzerAsync(text, cancellationToken: TestContext.Current.CancellationToken);
    }
}