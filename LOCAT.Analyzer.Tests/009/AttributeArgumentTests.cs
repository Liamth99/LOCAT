using System.Threading.Tasks;
using LOCAT.Analyzer._009;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;


namespace LOCAT.Analyzer.Tests._009;

public class AttributeArgumentTests : LocatVerifierBase<OptionalParameterNamedArgumentAnalyzer, OptionalParameterNamedArgumentCodeFix>
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

        await VerifyCodeFixAsync(text, fix, [Expected(location: 0, "name")]);
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

        await VerifyAnalyzerAsync(text);
    }

}