namespace Mokkit.Capture;

public static class Arrange
{
    public static TestArrange Start(ArrangeAsyncFn arrangeFn) => new TestArrange(arrangeFn);
    
    public static TestArrange Start(ArrangeFn arrangeFn) => new TestArrange(arrangeFn);
}