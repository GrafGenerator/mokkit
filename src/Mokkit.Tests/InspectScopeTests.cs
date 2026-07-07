using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Mokkit.Tests;

public class InspectScopeTests
{
    private sealed record Payload(bool Success, int Code, string Value);

    [Fact]
    public async Task ValueScope_RunsInnerSteps_InOrder_OverTheScopedValue()
    {
        var stage = await StageHelper.EmptyStage();
        var payload = new Payload(true, 200, "ok");
        var seen = new List<string>();

        await stage.Inspect()
            .ThenValueScope(payload)
            .Then((value, _) => seen.Add($"code:{value.Code}"))
            .Then((value, _) => seen.Add($"value:{value.Value}"));

        Assert.Equal(new[] { "code:200", "value:ok" }, seen);
    }

    [Fact]
    public async Task ContextScope_PassesBothValueAndContext_ToInnerSteps()
    {
        var stage = await StageHelper.EmptyStage();
        var payload = new Payload(true, 201, "created");

        Payload? seenValue = null;
        string? seenContext = null;

        await stage.Inspect()
            .ThenValueScope(payload, "ctx-value")
            .Then((value, context, _) =>
            {
                seenValue = value;
                seenContext = context;
            });

        Assert.Same(payload, seenValue);
        Assert.Equal("ctx-value", seenContext);
    }
}
