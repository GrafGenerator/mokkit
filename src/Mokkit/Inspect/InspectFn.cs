using System;
using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Inspect;

/// <summary>
/// Represents a synchronous inspect function that performs assertions and verifications.
/// This delegate is used in the Assert phase of the AAA (Arrange-Act-Assert) pattern.
/// </summary>
/// <param name="host">The test host that provides access to configured services and dependencies.</param>
public delegate void InspectFn(ITestHost host);

/// <summary>
/// Represents an asynchronous inspect function that performs assertions and verifications.
/// This delegate is used in the Assert phase of the AAA (Arrange-Act-Assert) pattern.
/// </summary>
/// <param name="host">The test host that provides access to configured services and dependencies.</param>
/// <returns>A task representing the asynchronous inspect operation.</returns>
public delegate Task InspectAsyncFn(ITestHost host);

/// <summary>
/// Represents an asynchronous inspect function that operates within a scope and can execute inner functions.
/// This delegate allows for complex inspection scenarios with nested operations.
/// </summary>
/// <param name="host">The test host that provides access to configured services and dependencies.</param>
/// <param name="executeInnerFns">A function that executes inner inspection operations within the current scope.</param>
/// <returns>A task representing the asynchronous scoped inspect operation.</returns>
public delegate Task InspectScopeAsyncFn(ITestHost host, Func<Task> executeInnerFns);

/// <summary>
/// Represents a synchronous inspect function that operates on a specific value.
/// This delegate is used for value-based assertions in the Assert phase.
/// </summary>
/// <typeparam name="T">The type of the value being inspected.</typeparam>
/// <param name="value">The value to inspect and assert against.</param>
/// <param name="host">The test host that provides access to configured services and dependencies.</param>
public delegate void InspectValueFn<in T>(T value, ITestHost host);

/// <summary>
/// Represents a synchronous inspect function that operates on a specific value with additional context.
/// This delegate is used for value-based assertions that require contextual information.
/// </summary>
/// <typeparam name="T">The type of the value being inspected.</typeparam>
/// <typeparam name="TContext">The type of the context object.</typeparam>
/// <param name="value">The value to inspect and assert against.</param>
/// <param name="context">Additional context information for the inspection.</param>
/// <param name="host">The test host that provides access to configured services and dependencies.</param>
public delegate void InspectValueWithContextFn<in T, in TContext>(T value, TContext context, ITestHost host);

/// <summary>
/// Represents an asynchronous inspect function that operates on a specific value.
/// This delegate is used for value-based assertions in the Assert phase.
/// </summary>
/// <typeparam name="T">The type of the value being inspected.</typeparam>
/// <param name="value">The value to inspect and assert against.</param>
/// <param name="host">The test host that provides access to configured services and dependencies.</param>
/// <returns>A task representing the asynchronous value inspection operation.</returns>
public delegate Task InspectValueAsyncFn<in T>(T value, ITestHost host);

/// <summary>
/// Represents an asynchronous inspect function that operates on a specific value with additional context.
/// This delegate is used for value-based assertions that require contextual information.
/// </summary>
/// <typeparam name="T">The type of the value being inspected.</typeparam>
/// <typeparam name="TContext">The type of the context object.</typeparam>
/// <param name="value">The value to inspect and assert against.</param>
/// <param name="context">Additional context information for the inspection.</param>
/// <param name="host">The test host that provides access to configured services and dependencies.</param>
/// <returns>A task representing the asynchronous value inspection operation with context.</returns>
public delegate Task InspectValueWithContextAsyncFn<in T,in TContext>(T value, TContext context, ITestHost host);