# Logging infrastructure in MSAL.NET

>[!NOTE]
>See also: [Logging in MSAL.NET](/azure/active-directory/develop/msal-logging-dotnet) on Microsoft Learn.

## Identity Logger Interface

MSAL.NET uses an interface named `IIdentityLogger` to provide logging for messages (MSAL.NET 4.45.0+). This comes with the benefit of enabling one logger implementation to be sharable between our partner SDKs (Microsoft.Identity.Web, Microsoft.IdentityModel). In order to take advantage of this new API you will need to provide an implementation of the `IIdentityLogger` interface.

This interface enables you to dynamically change the behavior of the logger without having to rebuild your MSAL implementation. For example, you could configure the `IsEnabled` method to the log level from an environment variable or app configuration for greater flexibility during debugging.

### IIdentityLogger Interface
```CSharp
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
```

### IIdentityLogger Implementations

#### Log level from configuration file

It's highly recommended to configure your code to use a configuration file in your environment to set the log level as it will enable your code to change the MSAL logging level without needing to rebuild or restart the application. This is critical for diagnostic purposes, enabling us to quickly gather the required logs from the application that is currently deployed and in production. Verbose logging can be costly so it's best to use the *Information* level by default and enable verbose logging when an issue is encountered. [See JSON configuration provider](/aspnet/core/fundamentals/configuration/?view=aspnetcore-7.0#json-configuration-provider) for an example on how to load data from a configuration file without restarting the application.

#### Log Level as Environment Variable

It is highly recommended to configure your code to use an environment variable on the machine to set the log level as it will enable your code to change the MSAL logging level without needing to rebuild the application. This is critical for diagnostic purposes, enabling us to quickly gather the required logs from the application that is currently deployed and in production.

See [EventLogLevel](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/dev/src/Microsoft.IdentityModel.Abstractions/EventLogLevel.cs) for details on the available log levels.

Sample `IIdentityLogger`implementation: 

```CSharp
    class MyIdentityLogger : IIdentityLogger
    {
        public EventLogLevel MinLogLevel { get; }

        public TestIdentityLogger()
        {
            //Try to pull the log level from an environment variable
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
            Console.WriteLine(entry.message);
        }
    }
```

Using `IIdentityLogger` implementation in MSAL:
```CSharp
    MyIdentityLogger myLogger = new MyIdentityLogger(logLevel);

    var app = ConfidentialClientApplicationBuilder
        .Create(TestConstants.ClientId)
        .WithClientSecret("secret")
        .WithLogging(myLogger, piiLogging)
        .Build();
```

## Add a log callback (Legacy)
An app also has the option to configure logging with a few lines of code, and have custom control over the level of detail and whether or not personal and organizational data is logged with a logging callback.

Create a logging callback:

```CSharp
void MyLoggingMethod(LogLevel level, string message, bool containsPii)
{
    Console.WriteLine($"MSAL {level} {containsPii} {message}");
}
```

Then provide the callback to `WithLogging` method when creating the `PublicClientApplication` or `ConfidentialClientApplication`:

```CSharp
var app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
          .WithLogging(MyLoggingMethod, LogLevel.Info,
                       enablePiiLogging: true, 
                       enableDefaultPlatformLogging: false) // the platform logger for .NET FW / .NET Core is EventSource
          .Buid();
```

**Important:** exception messages are not captured if `enablePiiLogging` is set to `false` (PII = Personally Identifiable Information). If you need to contact support, please try to send **Verbose** logs with **PII**. **Do not post PII logs on GitHub!**
Logs will never contain passwords or tokens, but PII logs may contain usernames, IDs etc.

Example of logs - [logs with PII](files/example_logging_pii.txt) and [logs without PII](files/example_logging_no_pii.txt).

## Logging in a distributed token cache
If you use Microsoft.Identity.Web's token cache serializers in .NET Framework or .NET Core, you can still benefit from detailed token cache logs.

To enable detailed logging for Microsoft.Identity.Web's token cache serializers in .NET Framework or .NET Core, set the [LoggerFilterOptions.MinLevel](/dotnet/api/microsoft.extensions.logging.loggerfilteroptions.minlevel?view=dotnet-plat-ext-7.0#microsoft-extensions-logging-loggerfilteroptions-minlevel) property to [LogLevel.Debug](/dotnet/api/microsoft.extensions.logging.loglevel?view=dotnet-plat-ext-7.0):

```csharp
// more code here
     app.AddDistributedTokenCache(services =>
     {
                services.AddDistributedMemoryCache();
                services.AddLogging(configure => configure.AddConsole())
                        .Configure<LoggerFilterOptions>(options => options.MinLevel = Microsoft.Extensions.Logging.LogLevel.Debug);
     });
// more code here
```

See more sample code using Microsoft Identity Web token cache serializers in the [ConfidentialClientTokenCache sample](https://github.com/Azure-Samples/active-directory-dotnet-v1-to-v2/blob/master/ConfidentialClientTokenCache/Program.cs). Also see [Implement a custom logging provider](/dotnet/core/extensions/custom-logging-provider) for more details.

## Network traces

**Important:** Network traces typically contain PII information. Please remove sensitive details before posting on GitHub.

You can get a network trace using tools like [Fiddler](https://www.telerik.com/fiddler). 
If such tool is not possible to use, for example on mobile, you can modify the `HttpClient` used by MSAL to log the HTTP traffic. See example `HttpClient` with logging [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/5a61d0c7adb2d30ffd6c61ac75501f7af1bfa063/tests/Microsoft.Identity.Test.Common/Core/Helpers/HttpSnifferClientFactory.cs#L11) (this should not be used in production, but only for logging). Custom `HttpClient` can be added as following: 

```csharp          
var msalPublicClient = PublicClientApplicationBuilder
       .Create(ClientId)
       .WithHttpClientFactory(new HttpSnifferClientFactory())
       .Build();
```

## Correlation ID

Logs help understand MSAL's behavior on the client side.

To understand what's happening on the service side, the team needs a correlation ID. This ID traces an authentication request through the various back-end services. 

The correlation ID can be obtained in three ways: 

1. From a successful authentication result - `AuthenticationResult.CorrelationId`.
2. From a service exception - `MsalServiceException.CorrelationId`.
3. By passing a custom correlation ID to `.WithCorrelationId(Guid)` builder method when building a token request. Use a different ID value for each request. Don't use a constant or we won't be able to differentiate requests.

## Windows Broker (WAM) logging

If you are using WAM, collect the MSAL verbose logs first. If more investigation is needed, follow: https://aka.ms/wamhot - this will use a tool created by Office that collects WAM traces. You can use the tool with any program.

## Windows Broker (WAM) network traces

If using Fiddler, please configure it as if capturing from an UWP app:

1. Enable AppContainer loopback in Fiddler UI -> WinConfig -> Exempt All -> Save Changes.
![image](https://user-images.githubusercontent.com/12273384/116380660-11677980-a80c-11eb-8989-bbf4985bde1c.png)

2. Enable HTTPS decryption, but exclude AD FS from HTTPS decryption: 
![image](https://user-images.githubusercontent.com/12273384/116380787-35c35600-a80c-11eb-88a2-210431148c0b.png)

## Full WAM logs

Go to http://aka.ms/icesdptool, which will automatically download a .cab file containing the Office Sign-in and Authentication Diagnostic tool.
Run the tool and repro your scenario, once the repro is complete. Finish the process.

Note: If do not want to give Fiddler traces simply reject the certificate requests that will pop up.

The wizard will prompt you for a password to safeguard your trace files. Please provide a password. You can enter "1234" as the password (aka the Default password). However, if for personal reasons you decide to enter a different password, please ensure that you communicate that separately to the team doing the investigation downstream.

Finally, open the folder where all the logs collected are stored. It is typically in a folder like
                %LOCALAPPDATA%\ElevatedDiagnostics\<numbers>
                Typing "%LOCALAPPDATA%" in your file explorer will take you to the correct location
Send the latest.cab, which contains all the collected logs, file to us.

## Personal and organizational data
By default, the MSAL logger doesn't capture any highly sensitive personal or organizational data. The library provides the option to enable logging personal and organizational data if you decide to do so.

The following sections provide more details about MSAL error logging for your application.