MSAL.NET 2.2.0 released on October 2018 on top of [MSAL.NET 2.1](MSAL-.NET-2.1-released)

## MSAL.NET 2.2.0 now supports Device Code Flow. 

When your desktop/mobile app needs to run on a device which does not have a web browser (text-only device), or if it's a .NET Core application (as .NET core does not provide a browser control), you have the following option:
- if this is running on a domain joined, or AAD joined Windows machine, you can user the integrated windows authentication, which is a silent flow that will request a token for the user signed-in in Windows.
- if however, you want the user to sign-in with a different account, this is more complex. You could use the username/password flow, but this is not recommended as this obliges the application to manipulate passwords, and also if the tenant admin has enabled multiple factor authentication, which requires an interaction in a web browser, this is not possible.

There is a third option which is good for command line tools, or apps running on iOT, which is the device code flow. The application displays a text containing a URL and a code, and the user will navigate to the URL on a browser (on the same machine if this is a desktop, but this could be on a mobile device), and will enter the code there. Then the user will sign-in normally, doing multiple factor authentication if necessary. When that's done the application, which was polling Azure AD will receive the access token.

The Device code flow is described in details in [Device Code Flow](https://aka.ms/msal-net-device-code-flow), and there is a new .NET Core 2.1 sample illustrating it: [active-directory-dotnetcore-devicecodeflow-v2](https://github.com/Azure-Samples/active-directory-dotnetcore-devicecodeflow-v2)

## Xamarin.iOS now leverage the best web view depending on the OS version

Xamarin.iOS applications using the system web view now benefit from the integration with:
- SFAuthenticationSession when they run on iOS11 
- and ASWebAuthenticationSession when they run on iOS12 or more

## Improved Error handling - example of HTTP 429 Retry After

The MsalServiceException now exposes the Http Response headers. Applications can now handle service errors better. A typical example is to avoid doing un-necessary calls when Azure AD is unavailable, by correctly managing the HTTP 429 error, which gives hints on when to retry. See https://aka.ms/msal-net-retry-after for a full explanation on how to add such support in your applications.

## Bug fixes

This release also fixes three bugs
- [MSAL issue 489](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/489) where an iOS application could crash when lauched, in the case it was not configured correction. MSAL.NET now throws an MsalClientException exception with a clear and actionable message when the application is not able to access keychain. See https://aka.ms/msal-net-enable-keychain-access for details.

- When enabling PII, messages were logged twice in log files. We removed the double-logging in log files and callbacks.  [#1289](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/pull/1289)

- Fixed a crash in UWP applications when the number of scopes required was too big. See [612](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/612) if you are interested in details.