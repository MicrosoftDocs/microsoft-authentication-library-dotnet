# MSAL.NET 4.0 released

We are excited to announce that one month after MSAL.NET GA-ed, we are now releasing a first incremental update bringing features you've been asking for:

- [ADFS 2019 support](#adfs-2019)
- [Asynchronous token cache serialization](#asynchronous-token-cache-serialization)
- [Interactive token acquisition on .NET Core](#net-core-now-support-interactive-authentication) (through the OS browser), including on Linux and Mac
- Fixes of [bugs that you raised](#bug-fixes)
- [Moving directly from MSAL 2.x?](#moving-from-msal-2x)

Unfortunately, the asynchronous token cache serialization introduced a [breaking change](#a-breaking-change), so we bumped-up the major version of MSAL. In practice, you should not be impacted at all, or very little (See [what's the impact](#what-is-the-impact)). As we were taking a breaking change we've decided to take other breaking changes as well on Telemetry (which, we are pretty sure won't impact you). See [changes in the interface to use to send telemetry](#telemetry).

## ADFS 2019

You can now connect directly to ADFS 2019. This is especially important if you intend to write an app working with Azure Stack.

To connect directly to ADFS, you'll use the existing `WithAdfsAuthority` Builder method:

```CSharp
var app = PublicClientApplicationBuilder.Create(clientId)
                                        .WithAdfsAuthority("https://somesite.contoso.com/adfs/")
                                        .Build();
```

You can then use the AcquireTokenXX methods as usual.

For more details see [ADFS support](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/ADFS-support)

## Asynchronous token cache serialization

Until MSAL.NET 3.1, when you wanted to customize token cache serialization, you had to provide synchronous methods. This meant that the whole process was blocked when storage was happening, which could be damageable for performance, for instance of Web Apps or Web APIs using a SQL token cache. Indeed, it's a frequent use case to persist the Token Cache in a distributed manner. Several among you have asked to have BeforeAccess/AfterAccess with an async signature, since most of the time the implementation is doing some IO, which can take time.

The `ITokenCache` interface now contains three new methods to set asynchronous callbacks: `SetAfterAccessAsync`, `SetBeforeAccessAsync`, `SetBeforeWriteAsync`

```CSharp
public interface ITokenCache 
{
 ...       
 void SetAfterAccessAsync(Func<TokenCacheNotificationArgs, Task> afterAccess);
 void SetBeforeAccessAsync(Func<TokenCacheNotificationArgs, Task> beforeAccess);
 void SetBeforeWriteAsync(Func<TokenCacheNotificationArgs, Task> beforeWrite);
 ...
}
```

For an example of usage of the async serialization see the code for the `MSALAppSessionTokenCacheProvider` class in the ASP.NET Core Web app tutorial: the PR proposing the change is [PR 107](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/pull/107/files#diff-0ecf9c19b4a4a4a209075405b65db9a5L85) of that sample.

### A breaking change

### ITokenCache's responsibility splatted between `ITokenCache` and `ITokenCacheSerializer`
In order to enable the async methods you need to use to subscribe to cache events, we have rewritten the non-async ones by calling the async ones. While doing that we splatted the responsibility of the `ITokenCache` interface between
`ITokenCache` which now contains the methods to subscribe to the cache serialization events, and a new interface `ITokenCacheSerializer` which exposes the methods that you need to use in the cache serialization events, in order to serialize/deserialize the cache

```CSharp
public interface ITokenCacheSerializer 
{
 void DeserializeAdalV3(byte[] adalV3State);
 void DeserializeMsalV2(byte[] msalV2State);
 void DeserializeMsalV3(byte[] msalV3State, bool shouldClearExistingCache=false);
 byte[] SerializeAdalV3();
 byte[] SerializeMsalV2();
 byte[] SerializeMsalV3();
}
```

And now, the `TokenCache` member of the `TokenCacheNotificationArgs` is of type `ITokenCacheSerializer`, which means you can only use these methods from the events themselves where we are sure that the right synchronization will happen.

### What is the impact?

In practice, even if this is a binary breaking change, you should be able to build your code without changing anything **provided you were only using the serialization and deserialization methods using the args.TokenCache instance provided in the token cache serialization events**.

> For instance the following sample did not require any change [ms-identity-dotnet-desktop-msgraph](https://github.com/Azure-Samples/ms-identity-dotnet-desktop-msgraph/pull/20)

If, however, you had kept a reference on the `ITokenCache` from `app.UserTokenCache` or `app.AppTokenCache`, and used this one in the serialization events, you will now get an explicit exception advising you to visit this article:

```Text
NotImplementedException: This is removed in MSAL.NET v4. Read more: https://aka.ms/msal-net-4x-cache-breaking-change
Microsoft.Identity.Client.TokenCache.DeserializeMsalV3(byte[] msalV3State, bool shouldClearExistingCache) in TokenCache.MigrationAid.cs, line 299
```

In that case what you need to do is use the `args.TokenCache.SerializeMsalV3` and not the instance of token cache you kept (probably as a member variable, which you can now remove).

```CSharp
 public static void AfterAccessNotification(TokenCacheNotificationArgs args)
 {
 // if the access operation resulted in a cache update
 if (args.HasStateChanged)
 {
  lock (FileLock)
  {
   // reflect changes in the persistent store
    File.WriteAllBytes(CacheFilePath,
                       ProtectedData.Protect(args.TokenCache.SerializeMsalV3(), 
                       null, 
                       DataProtectionScope.CurrentUser)
                      );
   }
  }
 }
```

## .NET Core now supports interactive authentication

### User experience

Given that .NET Core does not provide a Web browser control, until MSAL.NET 3.1, the interactive token acquisition was not supported. From this version, you can now use `AcquireTokenInteractive` with MSAL.NET. The experience for the end user will be the following
- The default browser for the operating system will be launched (a new tab in an existing browser can be opened)
- The user will then go through the sign-in and consent (if needed) in this browser/tab
- When the interaction is done, the browser will display just that the authentication is successful. The sentence is currently `Authentication complete. You can return to the application. Feel free to close this browser tab`. It's not yet customizable (See below).

Note that the experience is what it is, and in particular, when the interactive authentication has happened, the end user sees that the page which was displayed is localhost:someport?code=XYZTqlsdkfslkhskgh
The code is the authorization code. Even if the code was copied, it would not be usable as MSAL.NET uses the PKCE to protect the authorization code flow.

Even if this experience is not as neat as with embedded web view used on the .NET Framework platform, it has the advantage of enabling SSO with Web applications on the platform, which is a great plus. Chances are that your users won't even need to sign-in!

### How to enable it?

#### App registration

- You'll need to register "http://localhost" as a **Public client (mobile & desktop)** redirect URI for your application. In that case, Azure AD accepts any http://localhost:port. This is used by MSAL which finds an empty port, serves an HTML page and listen to this port to get the authentication code.
- Alternatively you can specify a localhost URL with a port if you don't want to let MSAL.NET choose a port.

#### .NET Code

The code is almost the same as if you were writing .NET Framework code, except that:
- This is .NET Core 
- You need to specify a RedirectUri set to "http://localhost" (or "http://localhost:port" if you registered a URL with a port)

Here is the complete code for a .NET Core console application using this feature:

```CSharp
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
 class Program
 {
  static async Task Main(string[] args)
  {
   string[] scopes = new[] { "user.read" };
   string clientId = "e9f70606-879c-4f0b-87cd-2754fccc4f44";
   var app = PublicClientApplicationBuilder.Create(clientId)
                                           .WithRedirectUri("http://localhost")
                                           .Build();
   var accounts = await app.GetAccountsAsync();
   AuthenticationResult result;
   try
   {
    result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                      .ExecuteAsync();
   }
   catch (MsalUiRequiredException)
   {
    result = await app.AcquireTokenInteractive(scopes)
                      .ExecuteAsync();
   }

   Console.WriteLine($"Hello {result.Account.Username}");
  }
 }
}
```

This kind of code used to throw a `PlatformNotSupportedException` before MSAL 3.1, with an explicit message telling you that on .NET Core interactive authentication was not supported, and advising to use Device Code Flow. This now works with the user experience described above.

### Error handling

As a support for this new feature, `MSALError` gets four additional strings used in MsalClientExceptions when things don't work as expected.

```CSharp
public static class MsalError 
{
 ...
 public const string InvalidAuthorizationUri = "invalid_authorization_uri";
 public const string LinuxXdgOpen = "linux_xdg_open_failed";
 public const string LoopbackRedirectUri = "loopback_redirect_uri";
 public const string LoopbackResponseUriMisatch = "loopback_response_uri_mismatch";
 ...
}

```
Error | Description
----- | ------------
LoopbackRedirectUri | This error happens when you forgot to add the `.WithRedirectUri("http://localhost")` modifier when building the application. An `MsalClientException` is then thrown by MSAL.NET with the following error message `'Only loopback redirect uri is supported, but urn:ietf:wg:oauth:2.0:oob was found. Configure http://localhost or http://localhost:port both during app registration and when you create the PublicClientApplication object. See https://aka.ms/msal-net-os-browser for details'`. 
LinuxXdgOpen | On Linux, this MsalClientException occurs when MSAL.NET is unable to open a web page specified at the redirect URI or selected port) using `xdg-open`. The inner exception provides more details. Possible causes for this error are that xdg-open is not installed or it cannot find a way to open an url. As a first mitigation, the end user needs to make sure they can open a web page by invoking from a terminal: xdg-open https://www.bing.com
InvalidAuthorizationUri  and LoopbackResponseUriMisatch | An MSALClientException with one of these ErrorCode is thrown when the response from the Microsoft identity platform v2.0 authorize endpoint is not what MSAL.NET expected, and therefore MSAL.NET cannot extract the authorization code. The best is to look at the inner exception for details 

### More to come

This is a start. In a next version to come we'll add additional customization so that you can provide URLs to have your browser navigate in case of success and failure;

## Telemetry

### [Breaking change] Replacing TelemetryCallback by TelemetryConfig

Until MSAL.NET 3.0.8, you could subscribe to telemetry by adding a telemetry callback .WithTelemetry(), and then sending to your telemetry pipeline of choice a list of events (which themselves were dictionaries of name, values) 

From MSAL.NET 4.0, if you want to add telemetry to your application, you need to create a class implementing `ITelemetryConfig`. MSAL.NET provides such a class (`TraceTelemetryConfig`) which does not send telemetry anywhere, but uses `System.Trace.TraceInformation` to trace the telemetry events. You could take it from there and add trace listeners to send telemetry.

```CSharp
public interface ITelemetryConfig
{
 TelemetryAudienceType AudienceType { get; }
 Action<ITelemetryEventPayload> DispatchAction { get; }
 string SessionId { get; }
}

public class TraceTelemetryConfig : ITelemetryConfig 
{
 public TraceTelemetryConfig();
 public IEnumerable<string> AllowedScopes { get; }
 public TelemetryAudienceType AudienceType { get; }
 public Action<ITelemetryEventPayload> DispatchAction { get; }
 public string SessionId { get; }
}
```

You initialize this config object with:
- The audience (pre-production or production)
- A callback that will process the `ITelemetryEventPayload` which is a set of typed dictionary containing telemetry values by their name. The allowed types are `bool`, `long`, `int`, and `string`

```CSharp
public enum TelemetryAudienceType 
{
 PreProduction = 0,
 Production = 1,
}

public interface ITelemetryEventPayload 
{
 IReadOnlyDictionary<string, bool> BoolValues { get; }
 IReadOnlyDictionary<string, long> Int64Values { get; }
 IReadOnlyDictionary<string, int> IntValues { get; }
 string Name { get; }
 IReadOnlyDictionary<string, string> StringValues { get; }
 string ToJsonString();
}
```

Finally, you create you app by passing the telemetry config

```CSharp
var app = PublicClientApplication.Create(clientId)
                                 .WithTelemetry(telemetryConfig)
                                 .Build();
```

## Bug fixes

This release also contains a number of bug fixes:

- In confidential client applications, MSAL.NET was not returning a URL in the `GetAuthorizationRequestUrl()` flow. See MSAL.NET issues [1193](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1193) and [1184](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1184)
- MSAL.NET now correctly handles the X509 cert on .NET Core. MSAL issue [1139](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1139)
- MSAL.NET now resolves the `TeamID` in the Keychain Access Group for the default configuration. Keychain sharing groups should be prefixed with the TeamID. Now, if the developer does not explicitly set the keychain access group through the `WithIosKeychainSecurityGroup` api, MSAL.NET will use the default "com.microsoft.adalcache", appended with the TeamID. Previously the TeamID was not included.

## Moving from MSAL 2.x?

If you are moving directly to MSAL.NET 4.0.0 from MSAL 2.x, nice reminder that you can learn about the breaking changes from 2.x to 3.x in [in MSAL 3.x released](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/MSAL.NET-3-released)