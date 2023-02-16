# MSAL.NET 4.2.1 released

We are excited to announce the release of MSAL.NET 4.2.1 which brings a number of new features:

- [Better control of user experience on invalid grant MsalUiRequiredException](#new-classification-property-on-msaluirequiredexception-enables-you-to-provide-a-better-user-experience-in-your-apps): A new property named `Classification` on `MsalUiRequiredException` to help you providing an optimal user experience when you receive an invalid grant error.
- [Better usability of the API to set the UI parent in cross platform apps](#improved-api-on-xamarin)  
- [Startup performance improvements - Custom Instance discovery](#improved-application-startup-cost-disconnected-scenarios-and-advanced-scenarios), 
- [General performance improvements - Cache Access](#Cache-is-accessed-less-frequently)

## New Classification property on MsalUiRequiredException enables you to provide a better user experience in your apps

One of common status codes returned from MSAL.NET when calling `AcquireTokenSilent()` is `MsalError.InvalidGrantError`. This status code means that the application should call the authentication library again, but in interactive mode (AcquireTokenInteractive or AcquireTokenByDeviceCodeFlow for public client applications, and do a challenge in Web apps). This is because additional user interaction is required before authentication token can be issued.

Most of the time when `AcquireTokenSilent` fails, it is because the token cache does not have tokens matching your request. Access tokens expire in 1h, and `AcquireTokenSilent` will try to fetch a new one based on a refresh token (in OAuth2 terms, this is the "Refresh Token' flow). This flow can also fail for various reasons, for example if a tenant admin configures more stringent login policies, the user's password has expired, the user needs to accept term of usage, etc ... the interaction aims at having the user do an action. Some of those conditions are easy for users to resolve (e.g. accept Terms of Use with a single click), and some cannot be resolved with the current configuration (e.g. the machine in question needs to connect to a specific corporate network). Some help the user setting-up Multi-factor authentication, or install Microsoft Authenticator on their device.

To help you guide the end user and provide a very good UX, you needed more data to make a decision on how to handle the exception. MsalUiRequiredException now exposes a new public property named `Classification` of type `UiRequiredExceptionClassification`. It helps you decide what to do in case of Invalid grant errors, informing the user, or batching conditional access or consent for instance.

The `UiRequiredExceptionClassification` is the following:

```CSharp
public enum UiRequiredExceptionClassification
{
 None = 0,
 MessageOnly = 1,
 BasicAction = 2,
 AdditionalAction = 3,
 ConsentRequired = 4,
 UserPasswordExpired = 5,
 PromptNeverFailed = 6,
 AcquireTokenSilentFailed = 7,
}
```

For details on how to handle if, see https://aka.ms/msal-net-UiRequiredException

## Improved API on Xamarin

On Android, you need to pass the parent activity. On iOS, when using a broker, you need to pass-in the ViewController. In the same way on UWP and Windows/Forms or WPF application you might want to pass-in the parent window. 

So far, the UI Parent (Window on UWP/Activity on Android) was injected when calling AcquireTokenInteractive:


```cs
var pca = PublicClientApplicationBuilder.Create(options.ClientId)
                                        .Build();

var result = await pca.AcquireTokenInteractive(Scopes)
                      .WithParentActivityOrWindow(Parent)
                      .ExecuteAsync();
```

Some of you, asked to allow injecting the UI parent (Parent Activity/Window) when building the public client application. See [#1095](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1095) for details about why this was necessary, and some of the designs we discused. In the end, we two mechanisms are proposed:

- the current mechanism where you can still, if you wish, specify the Ui Parent when calling `AcquireTokenInteractive`
- a new callback enabling you to specify, at app creation time, a delegate returning the UIParent.


```CSharp
IPublicClientApplication application = PublicClientApplicationBuilder.Create(clientId)
  .ParentActivityOrWindowFunc(() => parentUi)
  .Build();
```

On Android, a recommendation is to use the [CurrentActivityPlugin](https://github.com/jamesmontemagno/CurrentActivityPlugin).  Then your PublicClientApplication builder code would look like this:

```CSharp
// Requires MSAL.NET 4.2 or above
var pca = PublicClientApplicationBuilder
  .Create("<your-client-id-here>")
  .WithParentActivityOrWindow(() => CrossCurrentActivity.Current)
  .Build();
```

For details see https://aka.ms/msal-net-android-activity

## Self troubleshooting improvements.

In each release, based on what we learn interacting with you, and based on your pains, we try to improve the way you can troubleshooting issues yourself without raising issues on GitHub or Stack overflow, and without calling support.

### Automatically providing framework and version

When you provide us logs, we used to ask you the platform you were using and the MSAL.NET version. Now, MSAL.NET exceptions contain this information as `MsalException.ToString()` contains the Framework and version. A typical exception now looks like the following:

```Text
MSAL.Desktop.4.2.0.0.MsalUiRequiredException:
ErrorCode: exCode
Microsoft.Identity.Client.MsalUiRequiredException: exMessage
StatusCode: 400
ResponseBody: { "error":"invalid_grant", "suberror":"some_suberror",
"claims":"some_claims",
"error_description":"AADSTS90002: Tenant 'x' not found. ", "error_codes":[90002],"timestamp":"2019-01-28 14:16:04Z",
"trace_id":"43f14373-8d7d-466e-a5f1-6e3889291e00",
"correlation_id":"6347d33d-941a-4c35-9912-a9cf54fb1b3e"}
Headers: Retry-After: 0
```

### Some more error messages, more actionable

MSAL.NET 4.2 brings a number of new MsalError constants with actionalbe error messages to help you troubleshooting your app registration and configuration. Before MSAL 4.2, these used to be more cryptic InvalidOperationException. You'll now receive:

Error | Description
----  | ----------
`ClientIdMustBeAGuid` | if you specify a ClientId at app creation, which is not a `System.Guid`. We hesitated to force MSAL.NET public API to use Guid in the past, but this was not helping the code readability. We prefered to guard you with a meaningfull error message. Before MSAL 4.2, this used to be a more cryptic InvalidOperationException
`NoClientId`|` You used `CreateFromOptions()` to build your application but did not provide a clientID. Be sure to use the application ID from the application registration portal.
`InvalidClient` | when you have forgotten to specifiy that your application is a public client during app registration whereas you are using Integrated Windows Authentication, Device Code Flow or Username/Password. The error message also provides a self-troubleshooting aka.ms link: https://aka.ms/msal-net-invalid-client. Fixes [#1249](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1249)
`InvalidInstance` | AAD service error indicating that the configured authority does not exist
`TelemetryConfigOrTelemetryCallback ` | You have configured both a telememtry callback and a telemetry config. Be sure to configure only one telemetry mechanism

## Improved application startup cost, disconnected scenarios and advanced scenarios

In MSAL.NET 4.1, we started work to improve the application startup cost, and support disconnected scenarios (where you want to have access to the accounts without the device being connected to Internet). See [GetAccounts and AcquireTokenSilent are now less network chatty](msal-net-4.1#getaccounts-and-acquiretokensilent-are-now-less-network-chatty) for details. With MSAL.NET 4.2, we are completing this initiative but letting you speciy yourself the [Instance discovery](Msal-Custom-Authority-Aliases#what-is-instance-discovery) metadata, and [disable the automatic instance discovery](Msal-Custom-Authority-Aliases#disabling-instance-discovery). 

Most of you will never need to use this advanced feature, which should be left to some advanced scenarios where:
- performance of a command line tool frequently called by other processes is crucial (Think of Git Credential Manager for instance called frequently from Git command line tools or Visual Studio or Visual Studio Code)
- you are aware of security implications.

For details, read [Msal Custom Authority Aliases](https://aka.ms/msal-net-custom-instance-metadata)

## Cache is accessed less frequently

MSAL takes care of caching tokens and accounts. App developers can provide persistence mechanisms on some platforms. See [Token cache serialization](token-cache-serialization.md). MSAL refreshes the cache, i.e. reads the contents from the persistent storage into memory and write the contents from memory into the persistent storage. 

Based on your feedback, in MSAL 4.2, we've reduced the number of refreshes for read purposes, by refreshing only once for each `ExecuteAsync()` operation. A comparison chart: 

| Scenario      | Number of reads in MSAL.NET 4.1 | Number of reads in MSAL.NET 4.2 |
| ------------- |:----:| :-----:|
| GetAccountsAsync      | 1 | 1 |
| AcquireTokenInteractive      | 0 |  0|
| AcquireTokenSilent with valid AT in cache | 2 |    1 |
| AcquireTokenSilent without valid AT in cache | 4 |    1 |
| AcquireTokenForClient | 2 | 1 |

The biggest performance improvement occurs when you have an expired Access Token (AT) in the cache and `AcquireTokenSilent` needs to get a new one by refreshing the refresh token. Since the default expiration for ATs is 1h, this is a very common scenario. 

TokenCache access performance is highly dependent on the persistence and encryption you use. We expect the biggest gains to be felt on Web App scenarios, where token caches are stored on distributed caches or databases. 