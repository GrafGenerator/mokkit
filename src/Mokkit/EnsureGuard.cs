using System;
using System.Collections;
using System.Collections.Generic;

namespace Mokkit;

/// <summary>
/// Type-aware "not empty" guard shared by the <c>Ensure</c> arrange/inspect helpers. Emptiness is decided by the
/// runtime shape of the value: <c>null</c> is empty; an empty <see cref="string"/> or empty
/// <see cref="IEnumerable"/> is empty; any other value equal to its type default (e.g. <see cref="Guid.Empty"/>,
/// <c>0</c>) is empty; every other non-null value is considered present.
/// </summary>
internal static class EnsureGuard
{
    /// <summary>
    /// Returns <paramref name="value"/> if it is not empty; otherwise throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <typeparam name="T">The value type being guarded.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="because">Optional context appended to the failure message.</param>
    /// <returns>The validated, non-empty value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the value is empty.</exception>
    public static T NotEmpty<T>(T value, string? because = null)
    {
        if (IsEmpty(value))
        {
            throw new InvalidOperationException(Message(typeof(T), because));
        }

        return value;
    }

    /// <summary>
    /// Builds a consistent failure message for the <c>Ensure</c> helpers.
    /// </summary>
    /// <param name="type">The type of the value that failed the guard.</param>
    /// <param name="because">Optional caller-supplied context.</param>
    /// <param name="reason">The reason the guard failed (defaults to "was empty").</param>
    /// <returns>The composed message.</returns>
    public static string Message(Type type, string? because, string reason = "was empty")
    {
        var baseMessage = $"Ensure failed: expected a non-empty value of type '{type.Name}', but it {reason}.";

        return because is null ? baseMessage : $"{baseMessage} {because}";
    }

    private static bool IsEmpty<T>(T value)
    {
        switch (value)
        {
            case null:
                return true;
            case string s:
                return s.Length == 0;
            case IEnumerable enumerable:
                return !HasAny(enumerable);
            default:
                return EqualityComparer<T>.Default.Equals(value, default!);
        }
    }

    private static bool HasAny(IEnumerable enumerable)
    {
        var enumerator = enumerable.GetEnumerator();
        try
        {
            return enumerator.MoveNext();
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }
}
