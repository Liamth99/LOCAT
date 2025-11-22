using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
        LOCAT.Analyzer._006.TodoAnalyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace LOCAT.Analyzer.Tests._006;

public class TodoTests
{
    DiagnosticResult Expected(int location, string type, string content)
    {
        return new DiagnosticResult("LOCAT006", DiagnosticSeverity.Warning)
              .WithLocation(location)
              .WithMessageFormat("Comment contains `{0}`. Address or create issue: `{1}`")
              .WithArguments(type ,content);
    }

    [Fact]
    public async Task SingleLine_Todo_IsFlagged()
    {
        const string text = @"
class C
{
    void M()
    {
        {|#0:// TODO: fix this|}
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            text,
            Expected(0, "TODO", "TODO: fix this"));
    }

    [Fact]
    public async Task MultiLine_Todo_IsFlagged()
    {
        const string text = @"
class C
{
    void M()
    {
        {|#0:/* TODO: improve logic */|}
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            text,
            Expected(0, "TODO", "TODO: improve logic"));
    }

    [Fact]
    public async Task Multiple_Comments_AreFlagged()
    {
        const string text = @"
class C
{
    void M()
    {
        {|#0:// TODO: one|}
        {|#1:/* TODO: two */|}
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            text,
            Expected(0, "TODO", "TODO: one"),
            Expected(1, "TODO", "TODO: two"));
    }

    [Fact]
    public async Task Comment_Without_Todo_IsIgnored()
    {
        const string text = @"
class C
{
    void M()
    {
        // nothing to see here
        /* still nothing */
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task XmlDoc_Todo_IsIgnored()
    {
        const string text = @"
class C
{
    /// <summary>
    /// TODO: this should not trigger
    /// </summary>
    void M() { }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task StringLiteral_Todo_IsIgnored()
    {
        const string text = @"
class C
{
    void M()
    {
        var s = ""TODO: not a comment"";
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task Multiple_Markers_AreFlagged()
    {
        const string text = @"
class C
{
    void M()
    {
        {|#0:// FIXME: fix this|}
        {|#1:// BUG: handle edge case|}
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            text,
            Expected(0, "FIXME", "FIXME: fix this"),
            Expected(1, "BUG", "BUG: handle edge case"));
    }

    [Fact]
    public async Task Inline_Comment_IsFlagged()
    {
        const string text = @"
class C
{
    void M()
    {
        int x = 0; {|#0:// TODO: inline comment|}
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            text,
            Expected(0, "TODO", "TODO: inline comment"));
    }

    [Fact]
    public async Task Multi_Todo_In_One_Block_OnlyReportsOnce()
    {
        const string text = @"
class C
{
    void M()
    {
        {|#0:/* TODO: first
                 TODO: second */|}
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            text,
            Expected(0, "TODO", "TODO: first TODO: second"));
    }

    [Fact]
    public async Task SingleLine_IgnoreMarker_SuppressesDiagnostic()
    {
        const string text = @"
class C
{
    void M()
    {
        //~ TODO: this should be ignored
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task MultiLine_IgnoreMarker_SuppressesDiagnostic()
    {
        const string text = @"
class C
{
    void M()
    {
        /*~ TODO: ignored */
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task MultiLine_IgnoreMarker_MultiLineComment_SuppressesDiagnostic()
    {
        const string text = @"
class C
{
    void M()
    {
        /*~
           TODO: ignored
           TODO: ignored again
        */
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task SingleLine_NoIgnoreMarker_IsFlagged()
    {
        const string text = @"
class C
{
    void M()
    {
        {|#0:// TODO: report this|}
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            text,
            Expected(0, "TODO", "TODO: report this"));
    }

    [Fact]
    public async Task SingleLine_IgnoreMarker_MustBeImmediatelyAfterSlashes()
    {
        const string text = @"
class C
{
    void M()
    {
        //~ TODO: shouldnt be flagged
        {|#0:// ~ TODO: should be flagged|}
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            text,
            Expected(0, "TODO", "~ TODO: should be flagged"));
    }

    [Fact]
    public async Task Mixed_Ignored_And_Active_Comments()
    {
        const string text = @"
class C
{
    void M()
    {
        //~ TODO: ignore this
        {|#0:// TODO: report this|}

        /*~
           TODO: ignore this too
        */

        {|#1:/* TODO: but report this */|}
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            text,
            Expected(0, "TODO", "TODO: report this"),
            Expected(1, "TODO", "TODO: but report this"));
    }

    [Fact]
    public async Task Inline_Ignored_Comment()
    {
        const string text = @"
class C
{
    void M()
    {
        int x = 0; //~ TODO: ignore this
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task Inline_Comment_WithoutIgnoreMarker_IsFlagged()
    {
        const string text = @"
class C
{
    void M()
    {
        int x = 0; {|#0:// TODO: flagged|}
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            text,
            Expected(0, "TODO", "TODO: flagged"));
    }

        [Fact]
        public async Task IgnoreMarker_DoesNotApplyTo_XmlDocComments()
        {
            const string text = @"
class C
{
    /// <summary>
    /// TODO: still XML, should be ignored by analyzer logic, not by ~
    /// </summary>
    void M() { }
}
";

            await Verifier.VerifyAnalyzerAsync(text);
        }
}