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

    /// <summary>
    /// Reference-type counterpart of the nullable-struct overload: projects a possibly-null reference from
    /// <paramref name="source"/>, guards it as non-empty, and captures it — so a name or an email threads on
    /// without a <c>!</c> at the use site.
    /// </summary>
    /// <typeparam name="TSource">The source object type.</typeparam>
    /// <typeparam name="T">The projected reference type.</typeparam>
    /// <param name="inspect">The inspect chain.</param>
    /// <param name="source">The source object to project from.</param>
    /// <param name="selector">Projects a reference from the source (e.g. <c>r =&gt; r.Name</c>).</param>
    /// <param name="captured">Receives the validated, non-null value.</param>
    /// <param name="because">Optional context appended to the failure message.</param>
    /// <returns>The inspect chain for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the projected value is null or empty.</exception>
    public static ITestInspect Ensure<TSource, T>(
        this ITestInspect inspect,
        TSource source,
        Func<TSource, T?> selector,
        out T captured,
        string? because = null)
        where T : class
    {
        captured = EnsureGuard.NotEmpty(selector(source), because)!;

        return inspect;
    }

    /// <summary>
    /// Projects a value off an already-filled <see cref="ICapture{T}"/>, guards it as non-empty, and captures
    /// it. This is the capture-shaped sibling of the plain-source overloads — equivalent to
    /// <c>.Ensure(client.Prop(c =&gt; c.Id), out var id)</c>, but it reports an uninitialized capture as its own
    /// failure rather than as an empty value. Works for both value and reference members.
    /// </summary>
    /// <typeparam name="TSource">The captured type.</typeparam>
    /// <typeparam name="T">The projected value type.</typeparam>
    /// <param name="inspect">The inspect chain.</param>
    /// <param name="source">The capture to read (must already be filled — Inspect runs after Arrange/Act).</param>
    /// <param name="selector">Projects the value from the (non-null) captured value.</param>
    /// <param name="captured">Receives the validated, non-empty value.</param>
    /// <param name="because">Optional context appended to the failure message.</param>
    /// <returns>The inspect chain for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the capture is not initialized, or when the projected value is empty.
    /// </exception>
    public static ITestInspect Ensure<TSource, T>(
        this ITestInspect inspect,
        ICapture<TSource> source,
        Func<TSource, T> selector,
        out T captured,
        string? because = null)
    {
        if (source.Value is not { } sourceValue)
        {
            throw new InvalidOperationException(
                EnsureGuard.Message(typeof(T), because, "came from an uninitialized capture"));
        }

        captured = EnsureGuard.NotEmpty(selector(sourceValue), because);

        return inspect;
    }
}
