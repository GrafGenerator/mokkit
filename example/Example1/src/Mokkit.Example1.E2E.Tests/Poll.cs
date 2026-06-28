namespace Mokkit.Example1.E2E.Tests;

/// <summary>
/// Bounded polling helper for asserting eventually-consistent (async/message-driven) outcomes.
/// </summary>
public static class Poll
{
    public static async Task<bool> Until(Func<Task<bool>> condition, TimeSpan timeout, TimeSpan? interval = null)
    {
        var step = interval ?? TimeSpan.FromMilliseconds(500);
        var attempts = Math.Max(1, (int)(timeout.TotalMilliseconds / step.TotalMilliseconds));

        for (var i = 0; i < attempts; i++)
        {
            if (await condition())
            {
                return true;
            }

            await Task.Delay(step);
        }

        return await condition();
    }
}
