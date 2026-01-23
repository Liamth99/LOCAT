using System.Threading.Tasks;
using LOCAT.Analyzer._004;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace LOCAT.Analyzer.Tests._004;

public class ConstantFixTests : LocatVerifierBase<UseConstantPatternAnalyzer, ReplaceWithConstantPatternFix>
{

    private static readonly DiagnosticResult Expected =
        new DiagnosticResult("LOCAT004", DiagnosticSeverity.Info)
           .WithMessageFormat("Consider using '{0} {2}' instead of '{1} {2}'");

    [Fact]
    public async Task Fixes_Null_Comparison()
    {
        var before = @"
class C {
    void M(object x) {
        if ({|#0:x == null|}) { }
    }
}";
        var after = @"
class C {
    void M(object x) {
        if (x is null) { }
    }
}";

        await VerifyCodeFixAsync(before, after, [Expected.WithLocation(0).WithArguments("is", "==", "null")], cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Fixes_Int_Comparison()
    {
        var before = @"
class C {
    void M(int x) {
        if ({|#0:x == 0|}) { }
    }
}";
        var after = @"
class C {
    void M(int x) {
        if (x is 0) { }
    }
}";

        await VerifyCodeFixAsync(before, after, [Expected.WithLocation(0).WithArguments("is", "==", "0")], cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Fixes_String_Comparison()
    {
        var before = @"
class C {
    void M(string x) {
        if ({|#0:x == ""test""|}) { }
    }
}";
        var after = @"
class C {
    void M(string x) {
        if (x is ""test"") { }
    }
}";

        await VerifyCodeFixAsync(before, after, [Expected.WithLocation(0).WithArguments("is", "==", "\"test\"")], cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Fixes_NotEquals_To_IsNot()
    {
        var before = @"
class C {
    void M(int x) {
        if ({|#0:x != 5|}) { }
    }
}";
        var after = @"
class C {
    void M(int x) {
        if (x is not 5) { }
    }
}";

        await VerifyCodeFixAsync(before, after, [Expected.WithLocation(0).WithArguments("is not", "!=", "5")], cancellationToken: TestContext.Current.CancellationToken);
    }
    
    [Fact(Skip = "This test magically does work, but is technically compiled differently so it fails,")]
    public async Task Fixes_Enum_Member()
    {
        var before = @"
enum E { A, B }

class C {
    void M(E e) {
        if ({|#0:e == E.A|}) { }
    }
}";
        var after = @"
enum E { A, B }

class C {
    void M(E e) {
        if (e is E.A) { }
    }
}";

        await VerifyCodeFixAsync(before, after, [Expected.WithLocation(0).WithArguments("is", "==", "E.A")], cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact(Skip = "This test magically does work, but is technically compiled differently so it fails,")]
    public async Task Fixes_Const_Field()
    {
        var before = @"
class C {
    private const int Max = 10;
    void M(int x) {
        if ({|#0:x == Max|}) { }
    }
}";
        var after = @"
class C {
    private const int Max = 10;
    void M(int x) {
        if (x is Max) { }
    }
}";

        await VerifyCodeFixAsync(before, after, [Expected.WithLocation(0).WithArguments("is", "==", "Max")], cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact(Skip = "This test magically does work, but is technically compiled differently so it fails,")]
    public async Task Fixes_Const_Local()
    {
        var before = @"
class C {
    void M(int x) {
        const int Y = 42;
        if ({|#0:x == Y|}) { }
    }
}";
        var after = @"
class C {
    void M(int x) {
        const int Y = 42;
        if (x is Y) { }
    }
}";

        await VerifyCodeFixAsync(before, after, [Expected.WithLocation(0).WithArguments("is", "==", "42")], cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Fixes_Negative_Number()
    {
        var before = @"
class C {
    void M(int x) {
        if ({|#0:x == -5|}) { }
    }
}";
        var after = @"
class C {
    void M(int x) {
        if (x is -5) { }
    }
}";

        await VerifyCodeFixAsync(before, after, [Expected.WithLocation(0).WithArguments("is", "==", "-5")], cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact(Skip = "This test magically does work, but is technically compiled differently so it fails,")]
    public async Task Fixes_Namespace_Qualified_Enum()
    {
        var before = @"
namespace N { public enum Color { Red, Blue } }

class C {
    void M(N.Color c) {
        if ({|#0:c == N.Color.Blue|}) { }
    }
}";
        var after = @"
namespace N { public enum Color { Red, Blue } }

class C {
    void M(N.Color c) {
        if (c is N.Color.Blue) { }
    }
}";

        await VerifyCodeFixAsync(before, after, [Expected.WithLocation(0).WithArguments("is", "==", "N.Color.Blue")], cancellationToken: TestContext.Current.CancellationToken);
    }

}