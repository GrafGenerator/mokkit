using System;
using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Inspect;

public delegate void InspectFn(ITestHost host);

public delegate Task InspectAsyncFn(ITestHost host);

public delegate Task InspectScopeAsyncFn(ITestHost host, Func<Task> executeInnerFns);

public delegate void InspectValueFn<in T>(T value, ITestHost host);

public delegate void InspectValueWithContextFn<in T, in TContext>(T value, TContext context, ITestHost host);

public delegate Task InspectValueAsyncFn<in T>(T value, ITestHost host);

public delegate Task InspectValueWithContextAsyncFn<in T,in TContext>(T value, TContext context, ITestHost host);