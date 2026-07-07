using System;

namespace Mokkit.Inspect;

/// <summary>
/// <c>Ensure</c> helpers for the Inspect phase. Because the source is already materialized during Inspect, these
/// guard the value and capture it <b>eagerly</b> (synchronously) so later chained steps can consume the captured
/// value directly. They apply to value scopes too, since <see cref="ITestInspectScope{T}"/> derives from
/// <see cref="ITestInspect"/>.
/// </summary>
public static class EnsureInspectExtensions
{
    /// <summary>
    /// Guards <paramref name="value"/> as non-empty and captures it into <paramref name="captured"/> for reuse in
    /// later chained inspect steps.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="inspect">The inspect chain.</param>
    /// <param name="value">The already-materialized value (e.g. <c>result.ClientId!.Value</c>).</param>
    /// <param name="captured">Receives the validated value.</param>
    /// <param name="because">Optional context appended to the failure message.</param>
    /// <returns>The inspect chain for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the value is empty.</exception>
    public static ITestInspect Ensure<T>(
        this ITestInspect inspect,
        T value,
        out T captured,
        string? because = null)
    {
        captured = EnsureGuard.NotEmpty(value, because);

        return inspect;
    }

    /// <summary>
    /// Projects a nullable value from <paramref name="source"/>, guards that it has a non-empty value, and captures
    /// the unwrapped result — removing the <c>!.Value</c> noise for nullable-struct members (e.g. a <c>Guid?</c> id).
    /// </summary>
    /// <typeparam name="TSource">The source object type.</typeparam>
    /// <typeparam name="T">The unwrapped value type.</typeparam>
    /// <param name="inspect">The inspect chain.</param>
    /// <param name="source">The source object to project from.</param>
    /// <param name="selector">Projects a nullable value from the source (e.g. <c>r => r.ClientId</c>).</param>
    /// <param name="captured">Receives the unwrapped, validated value.</param>
    /// <param name="because">Optional context appended to the failure message.</param>
    /// <returns>The inspect chain for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the projected value is null or empty.</exception>
    public static ITestInspect Ensure<TSource, T>(
        this ITestInspect inspect,
        TSource source,
        Func<TSource, T?> selector,
        out T captured,
        string? because = null)
        where T : struct
    {
        var selected = selector(source);

        if (!selected.HasValue)
        {
            throw new InvalidOperationException(EnsureGuard.Message(typeof(T), because, "was null"));
        }

        captured = EnsureGuard.NotEmpty(selected.Value, because);

        return inspect;
    }
}
