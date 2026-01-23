using System.Threading.Tasks;
using LOCAT.Analyzer._011;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace LOCAT.Analyzer.Tests._011;

public class RegexTimeoutTests
{
    DiagnosticResult Expected(int location)
    {
        return new DiagnosticResult("LOCAT011", DiagnosticSeverity.Warning)
              .WithLocation(location)
              .WithMessageFormat("Regex is created without a match timeout");
    }

    [Fact]
    public async Task Warn_OnRegexCreationWithoutTimeoutOrNonBacktracking()
    {
        const string text = @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var r = {|#0:new Regex(""abc"")|};
    }
}";
        await LocatVerifier<RegexTimeoutAnalyzer>.VerifyAnalyzerAsync(text, [Expected(0)]);
    }

    [Fact]
    public async Task NoWarn_WhenRegexTimeoutProvided()
    {
        const string text = @"
using System;
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var r = new Regex(""abc"", RegexOptions.None, TimeSpan.FromSeconds(1));
    }
}";
        await LocatVerifier<RegexTimeoutAnalyzer>.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task NoWarn_WhenNonBacktrackingUsed()
    {
        const string text = @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var r = new Regex(""abc"", RegexOptions.NonBacktracking);
    }
}";
        await LocatVerifier<RegexTimeoutAnalyzer>.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task NoWarn_WhenNonBacktrackingCombinedWithOtherFlags()
    {
        const string text = @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var r = new Regex(""abc"", RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);
    }
}";
        await LocatVerifier<RegexTimeoutAnalyzer>.VerifyAnalyzerAsync(text, []);
    }

    [Fact]
    public async Task Warn_OnGeneratedRegexWithoutTimeout()
    {
        const string text = @"
using System.Text.RegularExpressions;

partial class C
{
    [{|#0:GeneratedRegex(""abc"")|}]
    private static partial Regex R();
    private static partial Regex R() => null!;
}";
        await LocatVerifier<RegexTimeoutAnalyzer>.VerifyAnalyzerAsync(text, [Expected(0)]);
    }

    [Fact]
    public async Task NoWarn_WhenGeneratedRegexHasTimeout()
    {
        const string text = @"
using System;
using System.Text.RegularExpressions;

partial class C
{
    [GeneratedRegex(""abc"", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex R();
    private static partial Regex R() => null!;
}";
        await LocatVerifier<RegexTimeoutAnalyzer>.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task NoWarn_WhenGeneratedRegexUsesNonBacktracking()
    {
        const string text = @"
using System.Text.RegularExpressions;

partial class C
{
    [GeneratedRegex(""abc"", RegexOptions.NonBacktracking)]
    private static partial Regex R();
    private static partial Regex R() => null!;
}";
        await LocatVerifier<RegexTimeoutAnalyzer>.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task Warn_WhenRegexOptionsNotConstant()
    {
        const string text = @"
using System.Text.RegularExpressions;

class C
{
    void M(RegexOptions opts)
    {
        var r = {|#0:new Regex(""abc"", opts)|};
    }
}";
        await LocatVerifier<RegexTimeoutAnalyzer>.VerifyAnalyzerAsync(text, [Expected(0)]);
    }

}