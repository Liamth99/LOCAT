using System.Threading.Tasks;
using LOCAT.Analyzer._002_003;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace LOCAT.Analyzer.Tests._002_003;

public class InvalidDebugDisplayAttributeTests
{
    DiagnosticResult Expected2(int location)
    {
        return new DiagnosticResult("LOCAT002", DiagnosticSeverity.Error)
              .WithLocation(location)
              .WithMessageFormat("Debug Displays Should not be empty");
    }

    DiagnosticResult Expected3(int location)
    {
        return new DiagnosticResult("LOCAT003", DiagnosticSeverity.Warning)
              .WithLocation(location)
              .WithMessageFormat("Debug Displays Should contain member data");
    }

    [Fact]
    public async Task ValueIsEmpty_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""""|#0})]
                            public class Class1{}
                            ";

        await LocatVerifier<InvalidDebugDisplayAnalyzer>.VerifyAnalyzerAsync(text, [Expected2(0)]);
    }

    [Fact]
    public async Task ValueIsWhiteSpace_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""     ""|#0})]
                            public class Class1{}";

        await LocatVerifier<InvalidDebugDisplayAnalyzer>.VerifyAnalyzerAsync(text, [Expected2(0)]);
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

        await LocatVerifier<InvalidDebugDisplayAnalyzer>.VerifyAnalyzerAsync(text, [Expected3(0)]);
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

        await LocatVerifier<InvalidDebugDisplayAnalyzer>.VerifyAnalyzerAsync(text, [Expected3(0)]);
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

        await LocatVerifier<InvalidDebugDisplayAnalyzer>.VerifyAnalyzerAsync(text);
    }
}