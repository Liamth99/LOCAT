using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
        LOCAT.Analyzer._006.TodoAnalyzer,
        LOCAT.Analyzer._006.SuppressTodoFix,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace LOCAT.Analyzer.Tests._006;

public class TodoFixTests
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
{|#0:// TODO: fix this|}
";

        const string fix = @"
//~ TODO: fix this
";

        await Verifier.VerifyCodeFixAsync(
            text,
            Expected(0, "TODO", "TODO: fix this"),
            fix);
    }

    [Fact]
    public async Task MultiLine_Todo_IsFlagged()
    {
        const string text = @"
{|#0:/* TODO: improve logic */|}
";

        const string fix = @"
/*~ TODO: improve logic */
";

        await Verifier.VerifyCodeFixAsync(
            text,
            Expected(0, "TODO", "TODO: improve logic"),
            fix);
    }

    [Fact]
    public async Task IgnoredComment_SingleLine_NotFlagged()
    {
            const string text = @"
//~ TODO: already ignored
";

            await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task CodeFix_AddsTilde_SingleLine()
    {
            const string text = @"
{|#0:// fixme: handle error|}
";

            const string fix = @"
//~ fixme: handle error
";

            await Verifier.VerifyCodeFixAsync(
                    text,
                    Expected(0, "fixme", "fixme: handle error"),
                    fix);
    }

    [Fact]
    public async Task CodeFix_AddsTilde_MultiLine()
    {
            const string text = @"
{|#0:/* bug: issue here */|}
";

            const string fix = @"
/*~ bug: issue here */
";

            await Verifier.VerifyCodeFixAsync(
                    text,
                    Expected(0, "bug", "bug: issue here"),
                    fix);
    }
}