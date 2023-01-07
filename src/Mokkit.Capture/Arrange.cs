using Mokkit.Capture.Suite;

namespace Mokkit.Capture;

public static class Arrange
{
    public static TestArrange Start(TestStage stage) => new(stage);
    
    public static TestArrange Start(ArrangeAsyncFn arrangeFn) => new(arrangeFn);
    
    public static TestArrange Start(ArrangeFn arrangeFn) => new(arrangeFn);
}