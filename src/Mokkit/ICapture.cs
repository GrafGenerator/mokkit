using System;

namespace Mokkit;

/// <summary>
/// Read-only contract shared by <see cref="Capture{T}"/> and <see cref="Trapture{T}"/>.
/// Exposes the captured value once it has been set by the capture system.
/// </summary>
/// <typeparam name="T">The type of the captured value.</typeparam>
public interface ICapture<out T>
{
    /// <summary>
    /// Gets the captured value, or the type default (<c>null</c> for reference types) if no value has been captured yet.
    /// </summary>
    T? Value { get; }

    /// <summary>
    /// Gets the captured value, guarded — <c>client.EnsureValue</c> instead of <c>client.Value!</c>, so the
    /// null-forgiving operator stays out of tests. This is the capture-level member of the <c>Ensure</c>
    /// family and applies the same definition of "empty": a capture that was never filled, or one holding
    /// <c>null</c> / <c>""</c> / <c>0</c> / <see cref="Guid.Empty"/> / an empty collection, fails loudly here
    /// rather than letting a bogus value flow onward.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the capture is unfilled or its value is empty.</exception>
    T EnsureValue { get; }

    /// <summary>
    /// Reads a member off the captured value — <c>client.Prop(c =&gt; c.Id)</c> instead of
    /// <c>client.Value!.Id</c>. Equivalent to <c>propFn(EnsureValue)</c>: the <b>capture</b> is guarded, the
    /// projected member is handed back as-is (so a nullable member may still come back <c>null</c>).
    /// </summary>
    /// <typeparam name="TProp">The type of the member being read.</typeparam>
    /// <param name="propFn">Projects the member from the guarded captured value.</param>
    /// <returns>The projected member, which may itself be <c>null</c> if the member is nullable.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propFn"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the capture is unfilled or its value is empty.</exception>
    TProp? Prop<TProp>(Func<T, TProp> propFn);
}
