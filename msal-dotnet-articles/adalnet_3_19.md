# ADAL.NET 3.19.4 and MSAL 1.1.2-preview released

We released new versions of both authentication libraries for .NET: ADAL.NET and MSAL.NET. They bring new capabilities, and among other things help you be GDPR-compliant.

## ADAL.NET and MSAL.NET help you be GDPR-compliant

ADAL.NET and MSAL.NET  enable you to log information so that you can more easily diagnose authentication issues in your application. By default, the libraries don't capture or log any Personally Identifiable Information (PII) or Organizational Identifiable Information (OII), but you can turn on this feature. By turning on PII (OII), you, as the application developer, take responsibility for safely handling highly sensitive data and complying with any regulatory requirements.
Indeed, in May 2018, a European privacy law, the [General Data Protection Regulation (GDPR)](https://www.microsoft.com/en-us/TrustCenter/Privacy/gdpr/default.aspx), is due to take effect. The GDPR imposes new rules on companies, government agencies, non-profits, and other organizations that offer goods and services to people in the European Union (EU), or that collect and analyze data tied to EU residents. The GDPR applies no matter where you are located. To help you be [GDPR compliant](https://www.microsoft.com/en-us/trustcenter/privacy/gdpr/resources) out of the box, we have made changes in the authentication libraries.

### Change of behavior in ADAL.NET: by default, logging does not expose PII any longer

 In prior versions of ADAL.net, you needed to create a class implementing the `IAdalLogCallback` interface to log information. This interface has only one method, Log, which takes as parameters:

- The LogLevel enumeration (Information, Verbose, Warning, Error)
- The message to log

![LoggerTypes](loggerTypes.png)

The legacy way of logging information is by setting an instance of this class implementing `IAdalLogCallback` to the `Callback` properties of the `LoggerCallbackHandler` static class. In versions of ADAL prior to 3.18, ADAL.NET used to log all the information, including secrets and Personally Identifiable Information (PII). Your application can still use the old mechanism:
But you will get an obsolete warning telling you to use the new mechanism:

![ErrorList](errorList.png)

If you are using ADAL > 3.17.2, no PII will ever be logged through the `IAdalLogCallback` to help you be GDPR-compliant.

```CSharp
class MyLogger : IAdalLogCallback
{
 public void Log(LogLevel level, string message)
 {
  Console.ForegroundColor = ConsoleColor.White;
  Console.WriteLine($"{level} {message}");
  Console.ResetColor();
 }
}

class Program
{
 static void CallApi()
 {
  LoggerCallbackHandler.PiiLoggingEnabled = true; // No effect with IAdalLogCallback
  LoggerCallbackHandler.Callback = new MyLogger();
  AuthenticationContext authenticationContext =
    new AuthenticationContext("https://login.microsoftonline.com/common");
  AuthenticationResult result;
  result = await authenticationContext.AcquireTokenAsync("<clientId>",
                                     "<resourceId>",
                                     new Uri("<ClientURI>"),
                                     new  PlatformParameters(PromptBehavior.Auto)
  );
 }
}
```

#### But you can log PII if you really want to

Now, if you really need/want to log PII to help you debug, you can leverage another mechanism (which by the way, is the same as the one used by MSAL.Net) which disables the mechanism implementing `IAdalLogCallback`:

- You can subscribe to every message (including the ones filtered out because they contain PII information), by setting the `LogCallback` delegate of `LoggerCallbackHandler`. The `containsPii` parameter of a message lets you know if a message contains PII or not. Using `LogCallback` will disable logging the messages through the `LoggerCallbackHandler.Callback` property.
- When you set the `LogCallback` property of the `LoggerCallbackHandler` static class, you can also control if you want to log PII or not by setting the `PiiLoggingEnabled` property. By default, this boolean property is set to false (to help you being GRDP-compliant). If you set it to true, messages will be logged twice: once without any PII, (for which `containsPii` will be false), and the second time with PII (for which `containsPii` will be true).

Finally, when PII information is logged, it's systematically hashed.

```CSharp
class Program
 {
  private static void Log(LogLevel level, string message, bool containsPii)
  {
   if (containsPii)`
   {`
    Console.ForegroundColor = ConsoleColor.Red;`
   }`
   Console.WriteLine($"{level} {message}");`
   Console.ResetColor();
  }

  static async CallApi()
  {
   LoggerCallbackHandler.LogCallback = Log;
   LoggerCallbackHandler.PiiLoggingEnabled = true;
   AuthenticationContext authenticationContext = new
                   AuthenticationContext("https://login.microsoftonline.com/common");
   AuthenticationResult result =  `await` authenticationContext.AcquireTokenAsync("<clientId>",
                                  "<resourceId>",
                                  new Uri("<ClientURI>"),
                                  new  PlatformParameters(PromptBehavior.Auto)
    );
  }
 }
```

#### What if you want to disable logging altogether?

In ADAL V3, to disable logging:
LoggerCallbackHandler.UseDefaultLogging = `false`;
Note that in ADAL v2.0, to disable logging you used to use:
AdalTrace.LegacyTraceSwitch.Level = TraceLevel.Error;

### MSAL.NET now supports RSACng in .NET 4.7

.NET Framework 4.7 and onwards changed the default implementation for the RSA crypto service provider. It used to be the RSACryptoServiceProvider, whereas now it uses RSACng. This broke applications that intended to use the new type of certificates. MSAL.NET (like ADAL.NET in a previous version) now works on any versions of the .NET framework from .NET 4.5 onwards. You don't need to do anything to benefit from this improvement. It just works.

### MSAL.NET helps you support conditional access

A few months ago, ADAL.NET brought support for Conditional access, and, more specifically, let you handle Claim challenge exceptions.  ADAL.NET enabled you to process claim challenges sent by Azure AD when your application needed to involve users to let them accept that the application access additional resources, or to let them do multi-factor authentication. We explained the scenario in more details, in [this](https://azure.microsoft.com/fr-fr/blog/adal-net-3-17-0-released-2/) blog post, at that time.
MSAL.NET now also enables you to support conditional access as well as:

- The Claims information is surfaced in the `MsalServiceException`.
- The public client application receiving this exception needs to call the `AcquireTokenAsync` overrides that contain the `extraQueryParameters` parameter to request more claims. `extraQueryParameters` is really a string composed of `key=value` segments separated by an ampersand (&). To request more claims, as requested by the web API, you will need to use a key "claims", and the value will be the `Claims` property returned in the `MsalServiceException`. This property can also be  directly returned by the Web API in the case where it got itself a claim challenge while acquiring a toke in the name of the user. This will be the case in service to service calls ([on-behalf-of](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/on-behalf-of))

```CSharp
  AuthenticationResult result = null;
  try
  {
    result = await app.AcquireTokenSilentAsync(scopes, app.Users.FirstOrDefault());
  }
  catch (MsalServiceException msalServiceException)
  {
    if (!string.IsNullOrEmpty(msalServiceException.Claims)))
    {
      string extraQueryParameters = $"claims={msalServiceException.Claims}";
      result = await app.AcquireTokenAsync(scopes,
                                           app.Users.FirstOrDefault(),
                                           new UIBehavior(),
                                           extraQueryParameters);
     }
  }
```

In the future, we want to provide an override of `AcquireTokenAsync` that takes claims as a parameter.

### MSAL.NET was updated to support Xamarin Forms on Android 25.3.1

In previous versions of MSAL.NET, you had difficulties referencing the MSAL NuGet package when also using Xamarin.Forms 25.3.1.
You can now simply reference MSAL.NET from a newly created `Xamarin.Forms` application. Upgrading your existing application to MSAL.NET is slightly more complex due to the way that NuGet works today. However, we provide you with all the guidance you need in the MSAL.NET wiki page: [Troubleshooting Xamarin.Android issues with MSAL](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Troubleshooting-Xamarin.Android-issues-with-MSAL).

### ADAL.NET 3.19.2 fixes the "user canceled the authentication" intermittent error on the UWP platform

Recently we got an [issue](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/issues/969)�for applications leveraging the UWP platform where, when doing an initial request to authenticate and AcquireToken, it instantly returned a `"user canceled the authentication"` exception. The next time the request happened as expected. This issue is not new however  (delete: [already happed](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/issues/684)) since ADAL 3.19.0 it became more frequent.
In case you are interested in the details, this issue was due to a race condition. We had, for a long time, an existing bug as we did not enforce, on the UWP platform the call of WIA (Web Integrated Authentication) from the UI thread. A recent addition related to "[authority aliases](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/AuthenticationContext:-the-connection-to-Azure-AD)" surfaced the issue in a more repeatable way.

### Reminder: Do not call ADAL or MSAL async methods synchronously from the UI thread

We mentioned the UI thread in the previous paragraph. As we had a number of questions recently about this subject, we thought that this post would be an opportunity to remind you that:

> you should not call ADAL's async method in a blocking way (using `.Result`, or `.Wait`), except from a `Main()` in a console application.

This statement is true in all applications, but in particular in UWP applications as it will systematically end up with a deadlock. This is a well-known rule (UI thread should never be blocked), and the explanation is the following: By synchronously calling an Adal API from UI thread you block the UI Synchronization Context and wait for completion of the ADAL API, but it cannot proceed with running the web UI on the UI Context since this one is blocked by the calling thread. As a result, the application has a deadlock - the calling thread waits for completion of Adal API also holding the UI context, and the Adal API waits for the UI context to proceed with the Web UI.

### In closing

As usual we'd love to hear your feedback. Please:

- Ask questions on  Stack Overflow using the [ADAL tag](http://stackoverflow.com/questions/tagged/adal) or the [MSAL tag](http://stackoverflow.com/questions/tagged/msal)
- Use [GitHub Issues](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/issues) on the ADAL.Net repo or [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues) open-source repository to report bugs or request features.
- Use the [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory) to provide recommendations and/or feedback
