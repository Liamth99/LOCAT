using System.Threading.Tasks;
using LOCAT.Analyzer._004;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace LOCAT.Analyzer.Tests._004;

public class ConstantTests
{
    private static readonly DiagnosticResult Expected =
        new DiagnosticResult("LOCAT004", DiagnosticSeverity.Info)
           .WithMessageFormat("Consider using '{0} {2}' instead of '{1} {2}'");

    [Fact]
    public async Task Flags_Null_Comparison()
    {
        var test = @"
class C {
    void M(object x) {
        if ({|#0:x == null|}) { }
    }
}";

        await LocatVerifier<UseConstantPatternAnalyzer>.VerifyAnalyzerAsync(test, [Expected.WithLocation(0).WithArguments("is", "==", "null")]);
    }

    [Fact]
    public async Task Flags_Int_Comparison()
    {
        var test = @"
class C {
    void M(int x) {
        if ({|#0:x == 0|}) { }
    }
}";

        await LocatVerifier<UseConstantPatternAnalyzer>.VerifyAnalyzerAsync(test, [Expected.WithLocation(0).WithArguments("is", "==", "0")]);
    }

    [Fact]
    public async Task Flags_String_Comparison()
    {
        var test = @"
class C {
    void M(string x) {
        if ({|#0:x == ""test""|}) { }
    }
}";

        await LocatVerifier<UseConstantPatternAnalyzer>.VerifyAnalyzerAsync(test, [Expected.WithLocation(0).WithArguments("is", "==", "\"test\"")]);
    }

    [Fact]
    public async Task Flags_Char_Comparison()
    {
        var test = @"
class C {
    void M(char x) {
        if ({|#0:x == 'c'|}) { }
    }
}";

        await LocatVerifier<UseConstantPatternAnalyzer>.VerifyAnalyzerAsync(test, [Expected.WithLocation(0).WithArguments("is", "==", "'c'")]);
    }

    [Fact]
    public async Task Flags_Boolean_Comparison()
    {
        var test = @"
class C {
    void M(bool x) {
        if ({|#0:x == true|}) { }
    }
}";

        await LocatVerifier<UseConstantPatternAnalyzer>.VerifyAnalyzerAsync(test, [Expected.WithLocation(0).WithArguments("is", "==", "true")]);
    }

    [Fact]
    public async Task Does_Not_Flag_When_Inside_ExpressionTree()
    {
        var test = @"
using System;
using System.Linq.Expressions;

class C {
    void M() {
        Expression<Func<int, bool>> e = x => x == 0;
    }
}";

        await LocatVerifier<UseConstantPatternAnalyzer>.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Flags_Enum_Member()
    {
        var test = @"
enum E { A, B }

class C {
    void M(E e) {
        if ({|#0:e == E.A|}) { }
    }
}";

        await LocatVerifier<UseConstantPatternAnalyzer>.VerifyAnalyzerAsync(test, [Expected.WithLocation(0).WithArguments("is", "==", "E.A")]);
    }

    [Fact]
    public async Task Flags_Const_Field()
    {
        var test = @"
class C {
    private const int Max = 10;
    void M(int x) {
        if ({|#0:x == Max|}) { }
    }
}";

        await LocatVerifier<UseConstantPatternAnalyzer>.VerifyAnalyzerAsync(test, [Expected.WithLocation(0).WithArguments("is", "==", "Max")]);
    }

    [Fact]
    public async Task Flags_Const_Local()
    {
        var test = @"
class C {
    void M(int x) {
        const int Y = 42;
        if ({|#0:x == Y|}) { }
    }
}";

        await LocatVerifier<UseConstantPatternAnalyzer>.VerifyAnalyzerAsync(test, [Expected.WithLocation(0).WithArguments("is", "==", "Y")]);
    }

    [Fact]
    public async Task Flags_Negative_Number()
    {
        var test = @"
class C {
    void M(int x) {
        if ({|#0:x == -5|}) { }
    }
}";

        await LocatVerifier<UseConstantPatternAnalyzer>.VerifyAnalyzerAsync(test, [Expected.WithLocation(0).WithArguments("is", "==", "-5")]);
    }

    [Fact]
    public async Task Flags_Namespace_Qualified_Enum()
    {
        var test = @"
namespace N { public enum Color { Red, Blue } }

class C {
    void M(N.Color c) {
        if ({|#0:c == N.Color.Blue|}) { }
    }
}";

        await LocatVerifier<UseConstantPatternAnalyzer>.VerifyAnalyzerAsync(test, [Expected.WithLocation(0).WithArguments("is", "==", "N.Color.Blue")]);
    }
}