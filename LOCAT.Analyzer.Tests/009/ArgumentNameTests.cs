using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    LOCAT.Analyzer._009.OptionalParameterNamedArgumentAnalyzer,
    LOCAT.Analyzer._009.OptionalParameterNamedArgumentCodeFix,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace LOCAT.Analyzer.Tests._009;

public class ArgumentNameTests
{
    DiagnosticResult Expected(int location, string name)
    {
        return new DiagnosticResult("LOCAT009", DiagnosticSeverity.Info)
              .WithLocation(location)
              .WithMessageFormat("The optional parameter ‘{0}’ is passed positionally. Use a named argument for clarity.")
              .WithArguments(name);
    }

    [Fact]
    public async Task OptionalParameter_Positional_TriggersDiagnostic()
    {
        const string text = @"
class C
{
    void M(int x = 5) {}

    void Test()
    {
        M({|#0:5|#0});
    }
}
";

        const string fix = @"
class C
{
    void M(int x = 5) {}

    void Test()
    {
        M(x: 5);
    }
}
";

        await Verifier.VerifyCodeFixAsync(
            text,
            Expected(location: 0, "x"),
            fix);
    }

    [Fact]
    public async Task OnlyOptionalParameterGetsNamedArgument()
    {
        const string text = @"
class C
{
    void M(int a, int b = 10) {}

    void Test()
    {
        M(1, {|#0:2|#0});
    }
}
";

        const string fix = @"
class C
{
    void M(int a, int b = 10) {}

    void Test()
    {
        M(1, b: 2);
    }
}
";

        await Verifier.VerifyCodeFixAsync(
            text,
            Expected(location: 0, "b"),
            fix);
    }

    [Fact]
    public async Task NamedArgument_NoDiagnostic()
    {
        const string text = @"
class C
{
    void M(int x = 5) {}

    void Test()
    {
        M(x: 5);
    }
}
";

        // No fix expected
        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task MultipleOptionalParameters_AllTriggerDiagnostics()
    {
        const string text = @"
class C
{
    void M(int a = 1, int b = 2, int c = 3) {}

    void Test()
    {
        M({|#0:10|#0}, {|#1:20|#1}, {|#2:30|#2});
    }
}
";

        const string fix = @"
class C
{
    void M(int a = 1, int b = 2, int c = 3) {}

    void Test()
    {
        M(a: 10, b: 20, c: 30);
    }
}
";

        await Verifier.VerifyCodeFixAsync(
            text,
            [Expected(location: 0, "a"),
            Expected(location: 1, "b"),
            Expected(location: 2, "c")],
            fix);
    }

    [Fact]
    public async Task MixedArguments_OnlyPositionalOptionalGetsDiagnostic()
    {
        const string text = @"
class C
{
    void M(int a = 1, int b = 2, int c = 3) {}

    void Test()
    {
        M({|#0:10|#0}, b: 20, {|#1:30|#1});
    }
}
";

        const string fix = @"
class C
{
    void M(int a = 1, int b = 2, int c = 3) {}

    void Test()
    {
        M(a: 10, b: 20, c: 30);
    }
}
";

        await Verifier.VerifyCodeFixAsync(
            text,
            [Expected(location: 0, "a"),
            Expected(location: 1, "c")],
            fix);
    }
}