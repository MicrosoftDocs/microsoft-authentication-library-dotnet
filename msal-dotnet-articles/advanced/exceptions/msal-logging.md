---
title: Logging errors and exceptions in MSAL.NET
description: Learn how to log errors and exceptions in MSAL.NET
services: active-directory
author: Dickson-Mwendia
manager: CelesteDG

ms.service: msal
ms.subservice: msal-dotnet
ms.topic: conceptual
ms.workload: identity
ms.date: 10/21/2022
ms.author: dmwendia
ms.reviewer: saeeda, jmprieur
ms.custom: aaddev, devx-track-dotnet
---

# Logging in MSAL.NET

MSAL.NET apps generate log messages that can help diagnose issues. You can configure logging with a few lines of code, and have custom control over the level of detail and whether or not personal and organizational data is logged. Logging isn't enabled by default. We recommend you enable MSAL logging to provide a way for users to submit logs when they have authentication issues. Note that MSAL doesn't store any logs and emits logs to the destination provided in the logger implementation.

>[!NOTE]
>Starting with MSAL.NET 4.58.0 developers can also [use OpenTelemetry](../monitoring.md#opentelemetry) to aggregate logs and measure application performance.

## Logging levels

There are several levels of logging detail:

- `LogAlways`: Base level which includes logs of important health metrics to help with diagnostics of MSAL operations.
- `Critical`: Logs that describe an unrecoverable application or system crash, or a catastrophic failure that requires immediate attention.
- `Error`: Indicates something has gone wrong and an error was generated. Used for debugging and identifying problems.
- `Warning`: Includes logs in scenarios when there hasn't necessarily been an error or failure, but are intended for diagnostics and pinpointing problems. This is the recommended minimum level that should be enabled in production apps.
- `Informational`: MSAL will log events intended for informational purposes, not necessarily intended for debugging.
- `Verbose`: MSAL logs the full details of library behavior. In production environment, verbose level should only be enabled temporarily to gather logs for a specific debugging purpose.

## Personal and organizational data

By default, the MSAL logger doesn't capture any highly sensitive personal or organizational data. The library provides the option to enable logging personal and organizational data if you decide to do so. For details, see [Handling of personally-identifiable information in MSAL.NET](/entra/msal/dotnet/resources/handling-pii).

## Configure logging in MSAL.NET

In MSAL, logging is set during application creation using the <xref:Microsoft.Identity.Client.BaseAbstractApplicationBuilder`1.WithLogging(Microsoft.IdentityModel.Abstractions.IIdentityLogger,System.Boolean)> builder. This method takes the following parameters:

- `identityLogger` is the logging implementation used by MSAL.NET to produce logs for debugging or health check purposes. Logs are only sent if logging is enabled.
- `enablePiiLogging` enables logging personal and organizational data (PII) if set to true. By default, this parameter is set to false, so that your application doesn't log sensitive data.

### IIdentityLogger interface

```csharp
namespace Microsoft.IdentityModel.Abstractions
{
    public interface IIdentityLogger
    {
        //
        // Summary:
        //     Checks to see if logging is enabled at given eventLogLevel.
        //
        // Parameters:
        //   eventLogLevel:
        //     Log level of a message.
        bool IsEnabled(EventLogLevel eventLogLevel);

        //
        // Summary:
        //     Writes a log entry.
        //
        // Parameters:
        //   entry:
        //     Defines a structured message to be logged at the provided Microsoft.IdentityModel.Abstractions.LogEntry.EventLogLevel.
        void Log(LogEntry entry);
    }
}
```

> [!NOTE]
> Partner libraries (`Microsoft.Identity.Web`, `Microsoft.IdentityModel`) already provide implementations of this interface for various environments (in particular ASP.NET Core).

### IIdentityLogger implementation

#### Log level from a configuration file

It's highly recommended to configure your code to use a configuration file in your environment to set the log level as it will enable your code to change the MSAL logging level without needing to rebuild or restart the application. This is critical for diagnostic purposes, enabling to quickly gather the required logs from the application that is currently deployed in production. Verbose logging can be costly, so it's best to use the `Informational` level by default and enable verbose logging when an issue is encountered. See [JSON configuration provider](/aspnet/core/fundamentals/configuration#json-configuration-provider) for an example on how to load data from a configuration file without restarting the application.

#### Log level from an environment variable

Another option we recommended is to configure your code to use an environment variable on the machine to set the log level as it will enable your code to change the MSAL logging level without needing to rebuild the application.

See <xref:Microsoft.IdentityModel.Abstractions.EventLogLevel> for details on the available log levels.

Example:

```csharp
    class MyIdentityLogger : IIdentityLogger
    {
        public EventLogLevel MinLogLevel { get; }

        public MyIdentityLogger()
        {
            //Retrieve the log level from an environment variable
            var msalEnvLogLevel = Environment.GetEnvironmentVariable("MSAL_LOG_LEVEL");

            if (Enum.TryParse(msalEnvLogLevel, out EventLogLevel msalLogLevel))
            {
                MinLogLevel = msalLogLevel;
            }
            else
            {
                //Recommended default log level
                MinLogLevel = EventLogLevel.Informational;
            }
        }

        public bool IsEnabled(EventLogLevel eventLogLevel)
        {
            return eventLogLevel <= MinLogLevel;
        }

        public void Log(LogEntry entry)
        {
            //Log Message here:
            Console.WriteLine(entry.Message);
        }
    }
```

Using `MyIdentityLogger`:

```csharp
    MyIdentityLogger myLogger = new MyIdentityLogger(logLevel);

    var app = ConfidentialClientApplicationBuilder
        .Create(TestConstants.ClientId)
        .WithClientSecret("secret")
        .WithLogging(myLogger, enablePiiLogging)
        .Build();
```

## Logging in a distributed token cache

If you use token cache serializers from [Microsoft.Identity.Web.TokenCache](https://www.nuget.org/packages/Microsoft.Identity.Web.TokenCache) package on .NET, you can enable additional caching logs.

To enable distributed cache logging, set the <xref:Microsoft.Extensions.Logging.LoggerFilterOptions.MinLevel> property to <xref:Microsoft.Extensions.Logging.LogLevel.Debug>.

```csharp
     app.AddDistributedTokenCache(services =>
     {
          services.AddDistributedMemoryCache();
          services.AddLogging(configure => configure.AddConsole())
               .Configure<LoggerFilterOptions>(options => options.MinLevel = Microsoft.Extensions.Logging.LogLevel.Debug);
     });
```

See [Implement a custom logging provider](/dotnet/core/extensions/custom-logging-provider) for more details.

## Correlation ID

Logs help understand the MSAL behavior on the client side. To understand what's happening on the service side, the team needs a correlation ID. This ID traces an authentication request through the various backend services.

The correlation ID can be obtained in three ways:

1. From a successful authentication result: <xref:Microsoft.Identity.Client.AuthenticationResult.CorrelationId?displayProperty=nameWithType>.
2. From a service exception: <xref:Microsoft.Identity.Client.MsalException.CorrelationId%2A?displayProperty=nameWithType>.
3. By passing a custom correlation ID to <xref:Microsoft.Identity.Client.BaseAbstractAcquireTokenParameterBuilder%601.WithCorrelationId(System.Guid)> when building a token request.

When providing your own correlation ID, use a different ID value for each request. Don't use a constant as we won't be able to differentiate between the requests.

## Network traces

> [!IMPORTANT]
> Network traces typically contain personally-identifiable information and credentials. **Remove any sensitive details** before posting the logs on GitHub.

In cases where verbose logs don't provide sufficient insights, you can get a network trace using tools like [Fiddler](https://www.telerik.com/fiddler) or [`mitmproxy`](https://mitmproxy.org/). You can configure your tool of choice to be a local proxy and accept traffic from devices on your local network, allowing you to capture traces from other devices, such as iPhone or Android phones. Platform-specific configuration may be required prior to capturing logs.

If such tool is not possible to use, you can modify the `HttpClient` used by MSAL to log the HTTP traffic. For reference, see this [custom `HttpClient` implementation with logging](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/b259cf00936a11a9cff789bf094935d8d31aea7f/tests/Microsoft.Identity.Test.Common/Core/Helpers/HttpSnifferClientFactory.cs#L11).

>[!WARNING]
>This client should not be used in production and only for logging.

Custom `HttpClient` can be added like this:

```csharp
var msalPublicClient = PublicClientApplicationBuilder
       .Create(ClientId)
       .WithHttpClientFactory(new HttpSnifferClientFactory())
       .Build();
```

### Network traces when using WAM

To collect network traces for [Web Account Manager (WAM) on Windows](../../acquiring-tokens/desktop-mobile/wam.md) with Fiddler, a few extra steps are needed.

1. Enable AppContainer loopback in Fiddler by clicking on **WinConfig**, selecting **Exempt All** and saving the changes.

:::image type="content" source="../../media/msal-logging/fiddler-exempt.png" alt-text="Exemption interface in Fiddler, showing all applications in the WinConfig dialog.":::

2. Enable HTTPS decryption, but exclude ADFS (`msft.sts.microsoft.com`) from HTTPS decryption:

:::image type="content" source="../../media/msal-logging/msft-sts-fiddler.png" alt-text="Screenshot of Fiddler Options, showing how to configure HTTPS decryption":::
