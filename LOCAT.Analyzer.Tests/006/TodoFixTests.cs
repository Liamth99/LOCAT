using System.Threading.Tasks;
using LOCAT.Analyzer._006;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace LOCAT.Analyzer.Tests._006;

public class TodoFixTests : LocatVerifierBase<TodoAnalyzer, SuppressTodoFix>
{
    DiagnosticResult Expected(int location, string type, string content)
    {
        return new DiagnosticResult("LOCAT006", DiagnosticSeverity.Warning)
              .WithLocation(location)
              .WithMessageFormat("Comment contains `{0}`. Address or create issue: `{1}`.")
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

        await VerifyCodeFixAsync(text, fix, [Expected(0, "TODO", "TODO: fix this")], cancellationToken: TestContext.Current.CancellationToken);
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

        await VerifyCodeFixAsync(text, fix, [Expected(0, "TODO", "TODO: improve logic")], cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task IgnoredComment_SingleLine_NotFlagged()
    {
            const string text = @"
//~ TODO: already ignored
";

            await VerifyAnalyzerAsync(text, cancellationToken: TestContext.Current.CancellationToken);
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

            await VerifyCodeFixAsync(text, fix, [Expected(0, "fixme", "fixme: handle error")], cancellationToken: TestContext.Current.CancellationToken);
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

            await VerifyCodeFixAsync(text, fix, [Expected(0, "bug", "bug: issue here")], cancellationToken: TestContext.Current.CancellationToken);
    }
}