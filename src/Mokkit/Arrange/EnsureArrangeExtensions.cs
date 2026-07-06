using System;

namespace Mokkit.Arrange;

/// <summary>
/// <c>Ensure</c> helpers for the Arrange phase. They derive a value (from a capture or an inline thunk),
/// guard it as non-empty, and hand it back through a <see cref="Trapture{T}"/> that later Act/Inspect steps
/// consume transparently. The derivation runs when the arrange chain executes, so the source capture is
/// already populated by the time the value is read.
/// </summary>
public static class EnsureArrangeExtensions
{
    /// <summary>
    /// Derives a value from an already-arranged capture, guards it as non-empty, and captures it into
    /// <paramref name="captured"/> for use after the arrange chain is awaited.
    /// </summary>
    /// <typeparam name="TSource">The captured source type (e.g. an entity).</typeparam>
    /// <typeparam name="T">The derived value type (e.g. its id).</typeparam>
    /// <param name="arrange">The arrange chain.</param>
    /// <param name="source">The source capture to read once the chain runs.</param>
    /// <param name="selector">Projects the derived value from the (non-null) source value.</param>
    /// <param name="captured">Receives a transparent capture of the derived value.</param>
    /// <param name="because">Optional context appended to the failure message.</param>
    /// <returns>The arrange chain for fluent chaining.</returns>
    public static ITestArrange Ensure<TSource, T>(
        this ITestArrange arrange,
        ICapture<TSource> source,
        Func<TSource, T> selector,
        out Trapture<T> captured,
        string? because = null)
        where TSource : class
    {
        var initializer = Trapture.Start(out captured);

        return arrange.Then(_ =>
        {
            var sourceValue = source.Value
                ?? throw new InvalidOperationException("Ensure failed: the source capture is not initialized.");

            initializer.Set(EnsureGuard.NotEmpty(selector(sourceValue), because));
        });
    }

    /// <summary>
    /// Evaluates a deferred thunk, guards its result as non-empty, and captures it into <paramref name="captured"/>
    /// for use after the arrange chain is awaited. Use this when the value is derived from more than one capture.
    /// </summary>
    /// <typeparam name="T">The derived value type.</typeparam>
    /// <param name="arrange">The arrange chain.</param>
    /// <param name="selector">A thunk evaluated when the chain runs (e.g. <c>() => client.Value!.Id</c>).</param>
    /// <param name="captured">Receives a transparent capture of the derived value.</param>
    /// <param name="because">Optional context appended to the failure message.</param>
    /// <returns>The arrange chain for fluent chaining.</returns>
    public static ITestArrange Ensure<T>(
        this ITestArrange arrange,
        Func<T> selector,
        out Trapture<T> captured,
        string? because = null)
    {
        var initializer = Trapture.Start(out captured);

        return arrange.Then(_ => initializer.Set(EnsureGuard.NotEmpty(selector(), because)));
    }
}
