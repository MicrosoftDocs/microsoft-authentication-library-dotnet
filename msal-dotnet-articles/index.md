# Microsoft Authentication Library for .NET

MSAL.NET ([Microsoft.Identity.Client](https://www.nuget.org/packages/Microsoft.Identity.Client)) is an authentication library that enables you to acquire tokens from Azure AD, to access protected Web APIs (Microsoft APIs or applications registered with Azure Active Directory). MSAL.NET is available on several .NET platforms (Desktop, Universal Windows Platform, Xamarin Android, Xamarin iOS, Windows 8.1, and .NET Core).

- [Getting Started](#conceptual-documentation)
  - [Getting started with MSAL.NET](#getting-started-with-msalnet)
  - [Details on the ways to acquire tokens](#details-on-the-ways-to-acquire-tokens)
  - [Advanced topics](#advanced-topics)
  - [Testimonials](#testimonials)
- [Roadmap](#roadmap)
- [Samples](#samples)
- [FAQ](#faq)

## Conceptual documentation

### Getting started with MSAL.NET

1. Learn about [authentication scenarios](scenarios) and [why use MSAL.NET](getting-started/overview.md).
1. You will need to [Register your app](getting-started/register-your-application.md) with Azure Active Directory.
1. Learn about the [types of client Applications](https://learn.microsoft.com/azure/active-directory/develop/msal-client-applications): public client and confidential client.
1. Learn about [Acquiring Tokens](acquiring-tokens/overview.md) to access a protected API.

### Acquiring Tokens

#### Acquiring tokens from cache in any app

- [AcquireTokenSilentAsync](acquiring-tokens/acquiretokensilentasync-api.md) enables you to get a previously cached token.

#### Acquiring tokens in Desktop/Mobile apps (public client applications)

- [Acquiring a token interactively](acquiring-tokens/desktop-mobile/acquiring-tokens-interactively.md) enables the application to acquire a token after authenticating the user through an interactive sign-in. There are implementation-specific details depending on the target platforms, such as [Xamarin Android](acquiring-tokens/desktop-mobile/xamarin.md) or [UWP](acquiring-tokens/desktop-mobile/uwp.md).
- Acquiring a token silently, on a Windows domain or AAD joined machine with [Integrated Windows Authentication](https://aka.ms/msal-net-iwa), or by using [Username/passwords](https://aka.ms/msal-net-up) (not recommended)
- Acquiring a token on a text only device, by directing the user to sign-in on another device, by leveraging the [Device Code Flow](https://aka.ms/msal-net-device-code-flow)

#### Acquiring tokens in Web Apps / Web APIs / daemon apps (confidential client applications)

- Acquiring a token for the app (without a user) with [client credential flows](Client-credential-flows) 
- Acquiring a token [on behalf of a user](on-behalf-of) in services to services calls
- Acquiring a token for the signed-in user [by authorization code](Acquiring-tokens-with-authorization-codes-on-web-apps) in Web Apps

### Advanced topics

- [Handling Exceptions in MSAL](exceptions) in MSAL.Net
  - [Retry-After](retry-after)
- How to customize the [Token cache serialization](token-cache-serialization)
- How to enable diagnostics and [Logging](logging) in MSAL.NET apps
- [Differences between ADAL.NET and MSAL.NET apps](Adal-to-Msal), and how to migrate and ADAL.NET app to MSAL.NET

#### Confidential client availability

MSAL is a multi-framework library. All Confidential Client flows **are available on**:

- .NET Core
- .NET Desktop
- .NET Standard

They are not available on the mobile platforms, because the OAuth2 spec states that there should be a secure, dedicated connection between the application and the Identity Provider. This secure connection can be achieved on web server / web API back-ends by deploying a certificate (or a secret string, but this is not recommended for production). It cannot be achieved on mobile and other client applications that are distributed to users. As such, these flows **are not available on**: 

- Xamarin.Android
- Xamarin.iOS
- UWP

### Testimonials

See our [Testimonials document](resources/testimonials.md).

## Roadmap
Date | Release | Blog post| Main features
------| ------- | ---------| ---------
*(Not Started)* | *MSAL Future* | | Optional Claims, Note: Managed identity support is now available in Azure Identity in preview, this means all Azure SDK's will have this support built in)
*(Next/In progress)* | [See milestones](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/milestones) | | 
*Releases* | [All releases](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases) |   |
Dec 19, 2022 | [4.49.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.49.1) | [MSAL 4.49.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.49.1) | Managed identity support for Azure Arc
Dec 16, 2022 | [4.49.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.49.0) | [MSAL 4.49.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.49.0) | Added managed identity support, ability to disable instance discovery, new APIs to work with WWW-Authenticate and Authentication-Info headers, ability to acquire Work and School accounts in new WAM broker preview, performance improvements.
Nov 2, 2022 | [4.48.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.48.0) | [MSAL 4.48.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.48.0) | Added .NET 6 targets; removed .NET 6 and Xamarin.Mac targets; GA'ed public client PoP API; bug fixes.
Oct 3, 2022 | [4.47.2](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.47.2) | [MSAL 4.47.2](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.47.2) | Hide legacy API's that are available only to internal Microsoft only (1P) applications; Soft deprecate WithAuthority API on AcquireTokenXXX methods. Instead use WithTenantId or WithTenantIdFromAuthority, or WithB2CAuthority for B2C authorities; Logging error codes to MSAL Telemetry; Improve extensibility APIs to support new POP; bug fixes.
Sep 17, 2022 | [4.47.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.47.1) | [MSAL 4.47.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.47.1) | Fixes an internal (Microsoft 1P only) NuGet feed issue.
Sep 16, 2022 | [4.47.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.47.0) | [MSAL 4.47.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.47.0) | Support for .NET MAUI is now generally available for iOS, Windows and Android targets; MSAL logging feature is now generally available; Added IsProofOfPosessionSupportedByClient to determine if the current broker supports PoP; ability to turn off the default retry-once policy on 5xx errors; new public builder API accepting instances of ITelemetryClient; bug fixes.
Aug 29th, 2022 | [4.46.2](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.46.2) | [MSAL 4.46.2](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.46.2) | Changed to an improved implementation of HTTP client factory on .NET Framework to improve resiliency; Logging additional exceptions to telemetry; bug fixes.
Aug 17th, 2022 | [4.46.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.46.1) | [MSAL 4.46.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.46.1) | Added Explicit .net 461 support to new WAM Preview broker; Added MSALRuntime TelemetryData to verbose logging when a broker exception is thrown; Minor clarifications in caching logs.
Aug 4th, 2022 | [4.46.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.46.0) | [MSAL 4.46.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.46.0) | `AcquireTokenByUsernamePassword` with PoP support in WAM broker preview; deprecated `SecureString`; exposed Identity Logger in caching code.
July 8th, 2022 | [4.46.0-preview](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.46.0-preview) | [MSAL 4.46.0-preview](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.46.0-preview) | Support for .NET 6 iOS and Android targets.
Jun 23rd, 2022 | [4.45.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.45.0) | [MSAL 4.45.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.45.0) | Upgrade to .NET Standard 2.0; improved logger API. **Please note the changes developers need to make to their apps.**
May 20th, 2022 | [4.44.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.44.0) | [MSAL 4.40.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.44.0) | Public Preview of Proof of Possession tokens for public client desktop Windows apps, based on new integration with Windows Broker.
May 2nd, 2022 | [4.43.2](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.43.2) | [MSAL 4.43.2](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.43.2) | Fix for Service Principals using refresh tokens in long-running OBO.
April 18th, 2022 | [4.43.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.43.1) | [MSAL 4.43.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.43.1) | Embedded WebView SSO bug fix for desktop apps.
April 5th, 2022 | [4.43.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.43.0) | [MSAL 4.43.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.43.0) | MAM support in Android; WAM bug fixes; system browser support in WSL2; UWP app packaging bug fix.
March 15th, 2022 | [4.42.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.42.1) | [MSAL 4.42.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.42.1) | WAM bug fix related to `/organizations` authority. Fix for packaging UWP apps.
March 1st, 2022 | [4.42.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.42.0) | [MSAL 4.42.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.42.0) | Multi-cloud support in interactive flow; MAM support in iOS; expose region used in `AuthenticationResult`; bug fixes. 
February 7th, 2022 | [4.41.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.41.0) | [MSAL 4.41.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.41.0) | WAM bug fixes and support improvements; support for Microsoft Edge as broker in Linux.
January 7th, 2022 | [4.40.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.40.0) | [MSAL 4.40.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.40.0) | Hybrid SPA is generally available. Allow POP token to be created externally. Improved performance, logging.
November 29th, 2021 | [4.39.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.39.0) | [MSAL 4.39.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.39.0) | Additional health metrics logging, multi-threading bug fix.
November 19th, 2021 | [4.38.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.38.0) | [MSAL 4.38.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.38.0) | Added Hybrid SPA support. Added new specific API for long running web APIs, in addition to `AcquireTokenOnBehalfOf`**, which no longer requests refresh tokens. Added the correlation ID used in calls to Azure AD as part of cache callback (`TokenCacheNotificationArgs`).
October 22nd, 2021 | [4.37.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.37.0) | [MSAL 4.37.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.37.0) | Improved user token cache performance, improved token refresh performance, added ability to enable shared internal cache, improved support for regional endpoints, ability to specify tenant ID at the request level, added cache refresh and token endpoint to `AuthenticationResultMetadata`.
October 6th, 2021 | [4.37.0-preview](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.37.0-preview) | [MSAL 4.37.0-preview](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.37.0-preview) | Improved user token cache performance, improved token refresh performance, added ability to enable shared internal cache, improved support for regional endpoints.
September 29th, 2021 | [4.36.2](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.36.2) | [MSAL 4.36.2](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.36.2) | Fixed a regression in authentication with the iOS broker.
September 8th, 2021 | [4.36.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.36.1) | [MSAL 4.36.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.36.1) | Support for Application ID URIs to be used in confidential client applications.
August 31st, 2021 | [4.36.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.36.0) | [MSAL 4.36.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.36.0) | Improved app token cache performance, improved token refresh timing, MSA-Passthrough with WAM, more actionable error messaging.
July 30th, 2021 | [4.35.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.35.1) | [MSAL 4.35.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.35.1) | Performance fixes
July 23rd, 2021 | [4.35.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.35.0) | [MSAL 4.35.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.35.0) | IAccount now provides TenantProfiles
July 8th, 2021 | [4.34.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.34.0) | [MSAL 4.34.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.34.0) | WAM is now generally available. WWW-Authenticate support
June 15th, 2021 | [4.32.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.32.1) | | Improved logging for cache performance and bug fixes
June 3rd, 2021 | [4.32.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.32.0) | [MSAL 4.32.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.32.0) | Kerberos support. Allow developers to inject XML federation metadata for IWA, bug fixes for IWA, fix UWP cache for multi-threaded operations, WAM fixes.
May 11th, 2021 | [4.31.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.31.0) | [MSAL 4.31.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.31.0) | Additional metrics in `AuthenticationResultMetadata`, option to hide iOS security prompt for system browser, WAM related fixes.
April 27th, 2021 | [4.30.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.30.1) | [MSAL 4.30.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.30.1) | MSAL.NET encodes data correctly when communicating with Android broker.
April 22nd, 2021 | [4.30.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.30.0) | [MSAL 4.30.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.30.0) | PKCE support during confidential client auth code flow and bug fixes. Partitioned token serialization cache for client credential flow
March 23rd, 2021 | [4.28.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.28.1) | [MSAL 4.28.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.28.1) | MSAL.NET now honors the `shouldClearExistingCache` when deserializing a null or empty blob.
February 19th, 2021 | [4.28.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/milestone/75?closed=1) | [MSAL 4.28.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.28) | A modern embedded browser on all platforms, helpers methods for public client apps.
February 19th, 2021 | [4.27.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/milestone/74?closed=1) | [MSAL 4.27.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.27.0) | Updated communication mechanism used in brokered authentication on Android to improve reliability and avoid power optimization issues
February 10th, 2021 | [4.26.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/milestone/73?closed=1) | [MSAL 4.26.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.26.0) | Added support for MSA-passthrough with WAM. Bug fixes.
January 20th, 2021 | [4.25.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/milestone/72?closed=1) | [MSAL 4.25.0](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.25.0) | Improvements to WAM and regional auth. WAM support moved to `Microsoft.Identity.Client.Desktop` package. Bug fixes.
*History* |  |  | [Memory lane](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Roadmap-History) 
 
For previous, or intermediate releases, see [releases](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases). See also [Semantic versioning - API change management](Semantic-versioning.-API-change-management) to understand changes in MSAL.NET public API, and [ADAL Release Cadence](msal-release-cadence) to understand when MSAL.NET is released

## Samples

See [our comprehensive sample list](https://docs.microsoft.com/azure/active-directory/develop/active-directory-v2-code-samples).

## FAQ

- How MSAL.NET uses [Web browsers](MSAL.NET-uses-web-browser) for interactive authentication
- if you have issues with Xamarin.Forms applications leveraging MSAL.NET please read [Troubleshooting-Xamarin.Android-issues-with-MSAL](./Troubleshooting-Xamarin.Android-issues-with-MSAL)
