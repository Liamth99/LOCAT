using System.Threading.Tasks;
using LOCAT.Analyzer._008;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace LOCAT.Analyzer.Tests._008;

public class RestrictedVariableNameTests : LocatVerifierBase<RestrictedVariableNameAnalyzer>
{
    private static readonly DiagnosticResult Expected =
        new DiagnosticResult("LOCAT008", DiagnosticSeverity.Warning)
           .WithMessageFormat("Variable name '{0}' violates naming restrictions for class '{1}' whose regex restrictions are {2}");


   [Fact]
    public async Task SingleCharacterVariable_MatchesRestriction_IsFlagged()
    {
        const string text = @"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<int> list = new();
        foreach (var {|#0:a|#0} in list)
        {
        }
    }
}
";

        await VerifyAnalyzerAsync(text, [Expected.WithLocation(0).WithArguments("a", "Int32", "\\b[a-z]\\b")], editorConfigContent: "[*.cs]\ndotnet_diagnostic.LOCAT008.default = \\b[a-z]\\b", cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidVariableName_DoesNotMatch_NotFlagged()
    {
        const string text = @"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<int> list = new();
        foreach (var number in list)
        {
        }
    }
}
";

        await VerifyAnalyzerAsync(text, editorConfigContent: "[*.cs]\ndotnet_diagnostic.LOCAT008.default = \\b[a-z]\\b", cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Parameter_SingleCharacter_IsFlagged()
    {
        const string text = @"
class C
{
    void M(int {|#0:x|#0})
    {
    }
}
";

        await VerifyAnalyzerAsync(text, [Expected.WithLocation(0).WithArguments("x", "Int32", "\\b[a-z]\\b")], editorConfigContent: "[*.cs]\ndotnet_diagnostic.LOCAT008.default = \\b[a-z]\\b", cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Property_SingleCharacter_IsFlagged()
    {
        const string text = @"
class C
{
    public int {|#0:p|#0} { get; set; }
}
";

        await VerifyAnalyzerAsync(text, [Expected.WithLocation(0).WithArguments("p", "Int32", "\\b[a-z]\\b")], editorConfigContent: "[*.cs]\ndotnet_diagnostic.LOCAT008.default = \\b[a-z]\\b", cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Field_SingleCharacter_IsFlagged()
    {
        const string text = @"
class C
{
    public int {|#0:f|#0};
}
";

        await VerifyAnalyzerAsync(text, [Expected.WithLocation(0).WithArguments("f", "Int32", "\\b[a-z]\\b")], editorConfigContent: "[*.cs]\ndotnet_diagnostic.LOCAT008.default = \\b[a-z]\\b", cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SimpleLambda_SingleCharacter_IsFlagged()
    {
        const string text = @"
using System;
using System.Linq;

class C
{
    void M()
    {
        var numbers = new[] { 1, 2, 3 };
        var result = numbers.Where({|#0:n|#0} => n > 0);
    }
}
";

        await VerifyAnalyzerAsync(text, [Expected.WithLocation(0).WithArguments("n", "Int32", "\\b[a-z]\\b")], editorConfigContent: "[*.cs]\ndotnet_diagnostic.LOCAT008.default = \\b[a-z]\\b", cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ParenthesizedLambda_SingleCharacter_IsFlagged()
    {
        const string text = @"
using System;
using System.Linq;

class C
{
    void M()
    {
        var numbers = new[] { 1, 2, 3 };
        var result = numbers.Where(({|#0:i|#0}) => i > 0);
    }
}
";

        await VerifyAnalyzerAsync(text, [Expected.WithLocation(0).WithArguments("i", "Int32", "\\b[a-z]\\b")], editorConfigContent: "[*.cs]\ndotnet_diagnostic.LOCAT008.default = \\b[a-z]\\b", cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ForEach_SingleCharacter_IsFlagged()
    {
        const string text = @"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<int> list = new();
        foreach (int {|#0:v|#0} in list)
        {
        }
    }
}
";

        await VerifyAnalyzerAsync(text, [Expected.WithLocation(0).WithArguments("v", "Int32", "\\b[a-z]\\b")], editorConfigContent: "[*.cs]\ndotnet_diagnostic.LOCAT008.default = \\b[a-z]\\b", cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MultipleVariables_AllMatching_AllFlagged()
    {
        const string text = @"
class C
{
    int {|#0:a|#0};
    void M(int {|#1:b|#1})
    {
        int {|#2:c|#2};
    }
}
";

        await VerifyAnalyzerAsync(
            text,
            [Expected.WithLocation(0).WithArguments("a", "Int32", "\\b[a-z]\\b"), Expected.WithLocation(1).WithArguments("b", "Int32", "\\b[a-z]\\b"), Expected.WithLocation(2).WithArguments("c", "Int32", "\\b[a-z]\\b")],
            editorConfigContent: "[*.cs]\ndotnet_diagnostic.LOCAT008.default = \\b[a-z]\\b",
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CaseInsensitive_Matching()
    {
        const string text = @"
class C
{
    int {|#0:X|#0};
    int {|#1:Y|#1};
}
";

        await VerifyAnalyzerAsync(
            text,
            [Expected.WithLocation(1).WithArguments("Y", "Int32", "(?i)[xy]"), Expected.WithLocation(0).WithArguments("X", "Int32", "(?i)[xy]")],
            editorConfigContent: "[*.cs]\ndotnet_diagnostic.LOCAT008.default = (?i)[xy]",
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NoConfiguration_NothingFlagged()
    {
        const string text = @"
class C
{
    int a;
    string s;
}
";

        await VerifyAnalyzerAsync(text, cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TypeSpecificRestriction_OnlyFlagsMatchingType()
    {
        const string text = @"
class C
{
    int {|#0:x|#0};
    string s;
}
";

        await VerifyAnalyzerAsync(text, [Expected.WithLocation(0).WithArguments("x", "Int32", "\\b[a-z]\\b")], editorConfigContent: "[*.cs]\ndotnet_diagnostic.LOCAT008.Int32 = \\b[a-z]\\b", cancellationToken: TestContext.Current.CancellationToken);
    }
}
