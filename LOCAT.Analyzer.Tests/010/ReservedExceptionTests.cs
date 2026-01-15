using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    LOCAT.Analyzer._010.DoNotUseReservedExceptionsAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace LOCAT.Analyzer.Tests._010;

public class ReservedExceptionTests
{
    DiagnosticResult Expected(int location, string name)
    {
        return new DiagnosticResult("LOCAT010", DiagnosticSeverity.Warning)
              .WithLocation(location)
              .WithMessageFormat("`{0}` is a reserved exception type")
              .WithArguments(name);
    }

    [Theory]
    [InlineData("Exception", "using System;")]
    [InlineData("System.Exception", "")]
    [InlineData("SystemException", "using System;")]
    [InlineData("ApplicationException", "using System;")]
    [InlineData("NullReferenceException", "using System;")]
    [InlineData("IndexOutOfRangeException", "using System;")]
    [InlineData("AccessViolationException", "using System;")]
    [InlineData("StackOverflowException", "using System;")]
    [InlineData("OutOfMemoryException", "using System;")]
    [InlineData("COMException", "using System.Runtime.InteropServices;")]
    [InlineData("SEHException", "using System.Runtime.InteropServices;")]
    [InlineData("ExecutionEngineException", "using System;")]
    public async Task ReservedExceptions_AreWarnings(string exceptionType, string usings)
    {
        var shortName = exceptionType.Contains(".")
            ? exceptionType.Substring(exceptionType.LastIndexOf('.') + 1)
            : exceptionType;

        var text = $@"
{usings}

class C
{{
    void Test()
    {{
        throw {{|#0:new {exceptionType}()|}};
    }}
}}
";

        await Verifier.VerifyAnalyzerAsync(text, Expected(0, shortName));
    }

    [Theory]
    [InlineData("InvalidOperationException", "using System;")]
    [InlineData("ArgumentException", "using System;")]
    [InlineData("NotSupportedException", "using System;")]
    [InlineData("System.InvalidOperationException", "")]
    public async Task NonReservedFrameworkExceptions_AreNotReported(string exceptionType, string usings)
    {
        var text = $@"
{usings}

class C
{{
    void Test()
    {{
        throw new {exceptionType}();
    }}
}}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task CustomException_IsNotReported()
    {
        const string text = @"
using System;

class MyCustomException : Exception
{
}

class C
{
    void Test()
    {
        throw new MyCustomException();
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }
}