using Mokkit.Capture.Suite;
using NUnit.Framework;

namespace Mokkit.Playground.CaptureTests;

public class BasePlayground
{
    protected TestStage Stage = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Stage = new TestStage();
    }
}