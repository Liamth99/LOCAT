using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
        LOCAT.Analyzer._002_003.InvalidDebugDisplayAnalyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace LOCAT.Analyzer.Tests._002_003;

public class InvalidDebugDisplayAttributeTests
{
    [Fact]
    public async Task ValueIsEmpty_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""""|#0})]
                            public class Class1{}
                            ";

        var expected = new DiagnosticResult("LOCAT002", DiagnosticSeverity.Error)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("Debug Displays Should not be empty.");

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task ValueIsWhiteSpace_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""     ""|#0})]
                            public class Class1{}";

        var expected = new DiagnosticResult("LOCAT002", DiagnosticSeverity.Error)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("Debug Displays Should not be empty.");

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task ValueDoesNotHaveBraces_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""Thisisuseless""|#0})]
                            public class Class1
                            {
                                public int Id { get; set; }
                            };";

        var expected = new DiagnosticResult("LOCAT003", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("Debug Displays Should contain member data.");

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task ValueHasOneBrace_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""Thisisuseless}""|#0})]
                            public class Class1
                            {
                                public int Id { get; set; }
                            };";

        var expected = new DiagnosticResult("LOCAT003", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("Debug Displays Should contain member data.");

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task NoIssue_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""{ThisIsUseful}""|#0})]
                            public class Class1
                            {
                                public int Id { get; set; }
                            };";

        await Verifier.VerifyAnalyzerAsync(text, []);
    }
}