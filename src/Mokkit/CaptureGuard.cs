using System;

namespace Mokkit;

/// <summary>
/// Shared implementation behind <see cref="ICapture{T}.EnsureValue"/> and <see cref="ICapture{T}.Prop{TProp}"/>.
/// It lives here rather than as a default interface member because the package targets netstandard2.0, so
/// <see cref="Capture{T}"/> and <see cref="Trapture{T}"/> each delegate to it and stay in step on wording and
/// behavior.
/// </summary>
internal static class CaptureGuard
{
    /// <summary>
    /// Returns the captured value, guarded by the shared <see cref="EnsureGuard"/> notion of "empty" — so an
    /// unfilled capture fails here, including a value-type one that has no <c>null</c> to check.
    /// </summary>
    /// <typeparam name="T">The captured type.</typeparam>
    /// <param name="value">The current captured value, if any.</param>
    /// <param name="captureName">The capture flavor's name, used in the failure message.</param>
    /// <returns>The guarded, non-empty captured value.</returns>
    public static T EnsureValue<T>(T? value, string captureName)
    {
        if (value is not { } capturedValue)
        {
            throw new InvalidOperationException($"{captureName} is not initialized");
        }

        return EnsureGuard.NotEmpty(capturedValue, $"Read from an unfilled or empty {captureName}.");
    }

    /// <summary>
    /// Projects a member off a guarded captured value — <c>propFn(EnsureValue)</c>.
    /// </summary>
    /// <typeparam name="T">The captured type.</typeparam>
    /// <typeparam name="TProp">The type of the member being read.</typeparam>
    /// <param name="value">The current captured value, if any.</param>
    /// <param name="propFn">Projects the member from the guarded captured value.</param>
    /// <param name="captureName">The capture flavor's name, used in the failure message.</param>
    /// <returns>The projected member.</returns>
    public static TProp? Prop<T, TProp>(T? value, Func<T, TProp> propFn, string captureName)
    {
        if (propFn is null)
        {
            throw new ArgumentNullException(nameof(propFn));
        }

        return propFn(EnsureValue(value, captureName));
    }
}
