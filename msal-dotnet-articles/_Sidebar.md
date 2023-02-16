### Getting started with MSAL.NET
- [Home](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki)
- [Why use MSAL.NET](MSAL.NET-supports-multiple-application-architectures-and-multiple-platforms)
- [Is MSAL.NET right for me](MSAL.NET-or-Microsoft.Identity.Web)
- [Scenarios](scenarios)
- [Register your app with AAD](Register-your-application-with-Azure-Active-Directory)
- [Client Applications](Client-Applications)
- [Acquiring Tokens](Acquiring-Tokens)
- [MSAL Samples](https://docs.microsoft.com/en-us/azure/active-directory/develop/sample-v2-code)
- [Known Issues](known%20issues)

### Acquiring tokens
- [AcquireTokenSilent](AcquireTokenSilentAsync-using-a-cached-token)

#### Desktop/Mobile apps
- [AcquireTokenInteractive](Acquiring-tokens-interactively)
- [WAM - the Windows broker](wam) 
- [.NET Core](System-Browser-on-.Net-Core)
- [Xamarin Docs](Xamarin-Docs)
- [UWP](UWP-specifics)
- [Custom Browser](CustomWebUi)
- Applying an [AAD B2C policy](AAD-B2C-specifics)
- [Integrated Windows Authentication](Integrated-Windows-Authentication) for domain or AAD joined machines
- [Username / Password](Username-Password-Authentication)
- [Device Code Flow](Device-Code-Flow) for devices without a Web browser
- [ADFS support](ADFS-support)
- [MSAL with Unity](Troubleshooting-Unity)

#### Web Apps / Web APIs / daemon apps
- [Acquiring a token for the app](Client-credential-flows)
- [Acquiring a token on behalf of a user](on-behalf-of) Service to Services calls
- [Acquiring a token by authorization code](Acquiring-tokens-with-authorization-codes-on-web-apps) in Web Apps

### Advanced topics
- [High Availability](High-availability)
- [Token cache serialization](token-cache-serialization)
- [Logging](logging)
- [Exceptions in MSAL](exceptions)
  - [Retry Policy](Retry-Policy)
  - [UiRequired exception classification](MsalUiRequiredException-classification)
  - [State Mismatch Error](StateMismatchError)
- [Provide your own Httpclient and proxy](httpclient)
- [Extensibility Points](Extensibility-Points)
- [Clearing the cache](clearing-token-cache)
- [Client Credentials Multi-Tenant guidance](Multi-tenant-client_credential-use)
- [Performance perspectives](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Performance-testing)
- [Differences between ADAL.NET and MSAL.NET Apps](Adal-to-Msal)
- [PowerShell support](PowerShell-support)
- [Testing apps that use MSAL](Testing-an-app-using-MSAL)
- [Experimental Features](Experimental-Features)
- [Proof of Possession (PoP) tokens](Proof-Of-Possession-(PoP)-tokens)
- Using in [Azure functions](msal-net-in-azure-functions)
- [Extract info from WWW-Authenticate headers](WWW-Authenticate-parameters)
- [SPA Authorization Code](SPA-Authorization-Code)

### News
- [Releases](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases)
- [iOS12 Security Advisory](https://github.com/AzureAd/microsoft-authentication-library-for-dotnet/wiki/iOS12)

### Contribute
- [Overview](https://github.com/AzureAd/microsoft-authentication-library-for-dotnet/wiki/Contributing-overview)
  - [Build & test MSAL.NET](https://github.com/AzureAd/microsoft-authentication-library-for-dotnet/wiki/build-and-test)
- [Submitting Bugs and Feature Requests](Submitting-Bugs-and-Feature-Requests)

### FAQ
- [Device authentication errors](device-authentication-errors)
- [Moving from MSAL 2.x to MSAL 3.x and above](MSAL.NET-2.x-to-MSAL.NET-3.x)
- [Usage of Web browsers](MSAL.NET-uses-web-browser)
- Troubleshooting [Unity issues with MSAL.NET](./Troubleshooting-unity)
- [Install NuGet package from other sources](./Installing-a-nuget-package-from-a-source-other-than-NuGet.org)
- [Getting scopes / consent for several Web APIs](Acquiring-tokens-interactively#withextrascopetoconsent)
- [TLS issues](tls-issues)
- [Troubleshooting](troubleshooting)
- [Synchronous programming](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Synchronous-Programming)
- [Target Framework Override](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Override-the-target-framework)

### Other resources
- [MSAL.NET reference documentation](https://docs.microsoft.com/active-directory/adal/microsoft.identity.client)
- [Azure AD v2.0 Developer guide](https://aka.ms/aadv2)