using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    LOCAT.Analyzer._007.NullConditionalAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace LOCAT.Analyzer.Tests._007;

public class NullConditionalTests
{
    DiagnosticResult Expected(int location)
    {
        return new DiagnosticResult("LOCAT007", DiagnosticSeverity.Warning)
              .WithLocation(location)
              .WithMessageFormat("Possible incorrect use of null-conditional operator here, detected as not null during compile time");
    }

    // ===================================================================
    // Diagnostics expected: receiver is definitely non-null
    // ===================================================================

    [Fact]
    public async Task Warn_OnMethodCallWithNonNullReceiver()
    {
        const string text = @"
    #nullable enable
    class C
    {
        void M(System.Type t)
        {
            var a = t{|#0:?|}.GetConstructors();
        }
    }";
        await Verifier.VerifyAnalyzerAsync(text, Expected(0));
    }

    [Fact]
    public async Task Warn_OnPropertyAccessWithNonNullReceiver()
    {
        const string text = @"
    #nullable enable
    class C
    {
        void M(System.Type t)
        {
            var b = t{|#0:?|}.Assembly;
        }
    }";
        await Verifier.VerifyAnalyzerAsync(text, Expected(0));
    }

    [Fact]
    public async Task Warn_OnElementAccessWithNonNullReceiver()
    {
        const string text = @"
    #nullable enable
    class C
    {
        void M(int[] i)
        {
            var c = i{|#0:?|}[0];
        }
    }";
        await Verifier.VerifyAnalyzerAsync(text, Expected(0));
    }

    // ===================================================================
    // No diagnostics expected: receiver could be null
    // ===================================================================

    [Fact]
    public async Task NoWarn_MethodCallWithNullableReceiver()
    {
        const string text = @"
    #nullable enable
    class C
    {
        void M(System.Type? t)
        {
            var a = t?.GetConstructors();
        }
    }";
        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task NoWarn_PropertyAccessWithNullableReceiver()
    {
        const string text = @"
    #nullable enable
    class C
    {
        void M(System.Type? t)
        {
            var b = t?.Assembly;
        }
    }";
        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task NoWarn_ElementAccessWithNullableReceiver()
    {
        const string text = @"
    #nullable enable
    class C
    {
        void M(int[]? i)
        {
            var c = i?[0];
        }
    }";
        await Verifier.VerifyAnalyzerAsync(text);
    }

    // ===================================================================
    // No diagnostics expected: normal access without null-conditional operator
    // ===================================================================

    [Fact]
    public async Task NoWarn_NormalMethodCall()
    {
        const string text = @"
    #nullable enable
    class C
    {
        void M(System.Type t)
        {
            var a = t.GetConstructors();
        }
    }";
        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task NoWarn_NormalPropertyAccess()
    {
        const string text = @"
    #nullable enable
    class C
    {
        void M(System.Type t)
        {
            var b = t.Assembly;
        }
    }";
        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task NoWarn_NormalElementAccess()
    {
        const string text = @"
    #nullable enable
    class C
    {
        void M(int[] i)
        {
            var c = i[0];
        }
    }";
        await Verifier.VerifyAnalyzerAsync(text);
    }

    // ===================================================================
    // No diagnostics expected: variable might be null at runtime
    // ===================================================================

    [Fact]
    public async Task NoWarn_MethodCallWithMaybeNullLocal()
    {
        const string text = @"
    #nullable enable
    class C
    {
        void M(System.Type? t)
        {
            var local = t;
            var a = local?.GetConstructors();
        }
    }";
        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task NoWarn_ElementAccessWithMaybeNullArray()
    {
        const string text = @"
    #nullable enable
    class C
    {
        void M(int[]? i)
        {
            var a = i?[0];
        }
    }";
        await Verifier.VerifyAnalyzerAsync(text);
    }


}