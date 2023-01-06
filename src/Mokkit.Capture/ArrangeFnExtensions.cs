namespace Mokkit.Capture;

public static class ArrangeFnExtensions
{
    public static ITestArrange Then(this ArrangeAsyncFn arrangeFn, ArrangeAsyncFn thenFn) =>
        Arrange.Start(arrangeFn).Then(thenFn);
    
    public static ITestArrange Then(this ArrangeAsyncFn arrangeFn, ArrangeFn thenFn) =>
        Arrange.Start(arrangeFn).Then(thenFn);
    
    public static ITestArrange Then(this ArrangeFn arrangeFn, ArrangeAsyncFn thenFn) =>
        Arrange.Start(arrangeFn).Then(thenFn);
    
    public static ITestArrange Then(this ArrangeFn arrangeFn, ArrangeFn thenFn) =>
        Arrange.Start(arrangeFn).Then(thenFn);
}