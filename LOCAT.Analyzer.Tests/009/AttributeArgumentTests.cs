using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    LOCAT.Analyzer._009.OptionalParameterNamedArgumentAnalyzer,
    LOCAT.Analyzer._009.OptionalParameterNamedArgumentCodeFix,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;


namespace LOCAT.Analyzer.Tests._009;

public class AttributeArgumentTests
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
using System;

class C
{

    sealed class _009TestAttribute(string name = ""default value"") : Attribute;

    [_009TestAttribute({|#0:""test""|#0})]
    void Test()
    {
        return;
    }
}
";

        const string fix = @"
using System;

class C
{

    sealed class _009TestAttribute(string name = ""default value"") : Attribute;

    [_009TestAttribute(name: ""test"")]
    void Test()
    {
        return;
    }
}
";

        await Verifier.VerifyCodeFixAsync(
            text,
            Expected(location: 0, "name"),
            fix);
    }

    [Fact]
    public async Task NamedArgument_NoDiagnostic()
    {
        const string text  = @"
using System;

class C
{

    sealed class _009TestAttribute(string name) : Attribute;

    [_009TestAttribute(name: ""test"")]
    void Test()
    {
        return;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

}