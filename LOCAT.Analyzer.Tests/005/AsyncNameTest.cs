using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
        LOCAT.Analyzer._005.AsyncNameAnalyzer,
        LOCAT.Analyzer._005.AsyncNameFix,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace LOCAT.Analyzer.Tests._005;

public class AsyncNameTests
{
    private static readonly DiagnosticResult Expected =
        new DiagnosticResult("LOCAT005", DiagnosticSeverity.Warning)
           .WithMessage("Async Method names should end with Async.")
           .WithLocation(0);

    [Fact]
    public async Task IgnoreMain()
    {
        const string text = @"
using System.Threading.Tasks;

public class Program
{
    async Task Main()
    {
        await Task.Delay(100);
    }
}";

        await Verifier.VerifyAnalyzerAsync(text);
    }
    
    [Fact]
    public async Task CheckVariableNames()
    {
        const string text = @"
using System.Threading.Tasks;

public class TestClass
{
    async Task {|#0:TestMethod|#0}()
    {
        await Task.Delay(100);
    }

    async Task TestMethodAsync()
    {
        await Task.Delay(100);
    }
}";

        await Verifier.VerifyAnalyzerAsync(text, Expected);
    }

    [Fact]
    public async Task CodeFixAppendsAsync()
    {
        // Input code that will trigger the analyzer
        const string testCode =
            @"
using System.Threading.Tasks;

public class TestClass
{
    public async Task {|#0:task|#0}()
    {
        await Task.Delay(50);
    }
}
";

        // Expected code after applying the CodeFixProvider
        const string fixedCode =
            @"
using System.Threading.Tasks;

public class TestClass
{
    public async Task taskAsync()
    {
        await Task.Delay(50);
    }
}
";

        await Verifier.VerifyCodeFixAsync(testCode, Expected, fixedCode);
    }

    [Fact]
    public async Task CodeFixAppendsAsyncToOverrides()
    {
        // Input code that will trigger the analyzer
        const string testCode =
            @"
using System.Threading.Tasks;

public abstract class TestClass
{
    public abstract Task task();
}

public class TestClass2 : TestClass
{
    public override async Task {|#0:task|#0}()
    {
        await Task.Delay(50);
    }
}
";

        // Expected code after applying the CodeFixProvider
        const string fixedCode =
            @"
using System.Threading.Tasks;

public abstract class TestClass
{
    public abstract Task taskAsync();
}

public class TestClass2 : TestClass
{
    public override async Task taskAsync()
    {
        await Task.Delay(50);
    }
}
";

        await Verifier.VerifyCodeFixAsync(testCode, Expected, fixedCode);
    }
}