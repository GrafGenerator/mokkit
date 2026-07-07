using System;

namespace Mokkit;

/// <summary>
/// Marks a <c>partial</c> arrange-extension method whose body Mokkit's source generator supplies. The method must be
/// declared in a <c>partial</c> class and have an <c>out Capture&lt;T&gt;</c> or <c>out Trapture&lt;T&gt;</c> parameter.
/// When the arrange runs, the generator sets that capture to <c>new T(...)</c>, forwarding the method's remaining
/// parameters positionally to <c>T</c>'s constructor. Methods without this marker keep their hand-written bodies, so
/// generated and manual arranges coexist.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class MokkitCaptureAttribute : Attribute
{
}
