using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
        LOCAT.Analyzer._001.DebugDisplayMissingAnalyzer,
        LOCAT.Analyzer._001.AddDebugDisplayFixProvider,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace LOCAT.Analyzer.Tests._001;

public class DebugDisplayFixTests
{
     [Fact]
    public async Task FixAddsDisplayAttribute()
    {
        // Input code that will trigger the analyzer
        const string testCode =
@"namespace Company.Models
{
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string fixedCode =
@"using System.Diagnostics;

namespace Company.Models
{
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(testCode,
            [
                new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
                   .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                   .WithArguments("TestClass")
            ],
            fixedCode);
    }

    [Fact]
    public async Task FixAddsDisplayAttribute_WithExistingUsings()
    {
        // Input code that will trigger the analyzer
        const string testCode =
            @"using System;

namespace Company.Models
{
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string fixedCode =
            @"using System;
using System.Diagnostics;

namespace Company.Models
{
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(testCode,
            [
                new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
                        .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                        .WithArguments("TestClass")
            ],
            fixedCode);
    }

    [Fact]
    public async Task FixAddsDisplayAttribute_WithXmlDocs()
    {
        // Input code that will trigger the analyzer
        const string testCode =
@"namespace Company.Models
{
    /// <summary>
    /// AAAAA
    /// </summary>
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string fixedCode =
@"using System.Diagnostics;

namespace Company.Models
{
    /// <summary>
    /// AAAAA
    /// </summary>
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(testCode,
            [
                new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
                   .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                   .WithArguments("TestClass")
            ],
            fixedCode);
    }

    [Fact]
    public async Task FixAddsDisplayAttribute_PreserveExistingAttributes()
    {
        // Input code that will trigger the analyzer
        const string testCode =
            @"using System;

namespace Company.Models
{
    [Serializable]
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string fixedCode =
            @"using System;
using System.Diagnostics;

namespace Company.Models
{
    [Serializable]
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(testCode,
            [
                new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
                   .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                   .WithArguments("TestClass")
            ],
            fixedCode);
    }

    [Fact]
    public async Task FixAddsDisplayAttribute_Multiple()
    {
        // Input code that will trigger the analyzer
        const string testCode =
            @"namespace Company.Models
{
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }

    public class {|#1:TestClass2|#1}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string fixedCode =
            @"using System.Diagnostics;

namespace Company.Models
{
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }

    [DebuggerDisplay("""")]
    public class TestClass2
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(testCode,
            [
                new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
                        .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                        .WithArguments("TestClass"),
                new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
                       .WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
                       .WithArguments("TestClass2")
            ],
            fixedCode);
    }

    [Fact]
    public async Task FixAddsDisplayAttribute_MultipleButOneIsGood()
    {
        // Input code that will trigger the analyzer
        const string testCode =
            @"using System.Diagnostics;
namespace Company.Models
{
    [DebuggerDisplay("""")]
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }

    public class {|#1:TestClass2|#1}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string fixedCode =
            @"using System.Diagnostics;
namespace Company.Models
{
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }

    [DebuggerDisplay("""")]
    public class TestClass2
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(testCode,
            [
                new DiagnosticResult("LOCAT001", DiagnosticSeverity.Warning)
                   .WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
                   .WithArguments("TestClass2")
            ],
            fixedCode);
    }

    [Fact]
    public async Task FixIsNotApplied_IfDebuggerDisplayExists()
    {
        // Input code where DebuggerDisplay is already present
        const string testCode = @"
        using System.Diagnostics;

        namespace Company.Models
        {
            [DebuggerDisplay(""Id = {Id}"")]
            public class TestClass
            {
                public int Id { get; set; }
            }
        }";

        // Expected code (should remain unchanged because the attribute already exists)
        const string fixedCode = testCode;

        // Verify that no additional attribute is added
        await Verifier.VerifyCodeFixAsync(testCode, fixedCode);
    }

    [Fact]
    public async Task FixIsNotApplied_NameSpaceIsNotModels()
    {
        // Input code where DebuggerDisplay is already present
        const string testCode = @"
        namespace NoDisplayPlz
        {
            public class TestClass
            {
                public int Id { get; set; }
            }
        }";

        // Expected code (should remain unchanged because the attribute already exists)
        const string fixedCode = testCode;

        // Verify that no additional attribute is added
        await Verifier.VerifyCodeFixAsync(testCode, fixedCode);
    }
}