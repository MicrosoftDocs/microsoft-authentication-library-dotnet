# Overview
It is strongly recommended to use async programming practices for better performance and responsive apps. However, some legacy apps cannot use asynchronous programming. 

MSAL.NET is based on Task-based Asynchronous Pattern (TAP). This page provides links to guidance about how to use async methods in a synchronous way. This has no one solution that fits all. So various best practices are recommended.

## Asynchronous programming
If you are not familiar with asynchronous programming, this article will get you familiarized with it:
[Asynchronous programming with async and await](/dotnet/csharp/programming-guide/concepts/async/).

You can check for courses on LinkedIn:
[Advanced Programming in C#](https://www.linkedin.com/learning/async-programming-in-c-sharp/introduction?u=3322).

## Calling Asynchronous methods from Synchronous code
There are several ways to run asynchronous code from a synchronous code. Various links are listed here.

[Task.RunSynchronously](/dotnet/api/system.threading.tasks.task.runsynchronously?view=net-5.0)
```csharp
var getAcctsTasks = PCA.RemoveAsync(acct);
// there is no timeout for RunSynchronously
if (!getAcctsTasks.IsCompleted)
{
   getAcctsTasks.RunSynchronously();
}
```

[Wait for a task to complete with Task.Wait](/dotnet/api/system.threading.tasks.task.wait?view=net-5.0)
```csharp
// wait can optionally have timeout, and cancellation token (not shown)
int timeoutMilliSec = 3000;
PCA.RemoveAsync(acct).Wait(timeoutMilliSec);
```

[Wait to get result with Task.Result](/dotnet/api/system.threading.tasks.task-1.result?view=net-5.0#remarks)
```csharp
var authResult = PCA.AcquireTokenSilent(Scopes, acct).ExecuteAsync().Result;
return authResult;
```

If you need to run multiple tasks at a time prior to wrapping them, it may be useful to take a look at 
[Consuming the Task-based Asynchronous Pattern](/dotnet/standard/asynchronous-programming-patterns/consuming-the-task-based-asynchronous-pattern).

## Watch out for exceptions and deadlocks
Here is how to catch exceptions and prevent deadlocks with `.ConfigureAwait(false)`.
```csharp
try
{
	Console.WriteLine("Pre AcquireTokenInteractive");
	// Run with wait command
	// create the builder
	var builder = PCA.AcquireTokenInteractive(Scopes);

	// run it interactively.
	// make sure to have ConfigureAwait(false) to avoid any potential deadlocks
	var authResult = builder.ExecuteAsync()
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
	Console.WriteLine("Post AcquireTokenInteractive - Got the token");

	return result;
}
catch (MsalClientException ex)
{
	// catch MSAL exception
	Console.WriteLine(ex.Message);
}
catch (Exception ex)
{
	Console.WriteLine(ex.Message);
}
```