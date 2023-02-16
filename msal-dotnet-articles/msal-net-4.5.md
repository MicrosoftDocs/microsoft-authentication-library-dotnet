# MSAL.NET 4.5 released

We are excited to announce the release of MSAL .NET 4.5.0, which brings improvements to the device code flow both for Azure AD and ADFS, as well as several bug fixes, in particular around iOS13 and UWP:

- [Device code flow improvements](#device-code-flow-improvements), with support of Microsoft personal accounts and ADFS 2019
- added [Telemetry](#telemetry-data) to monitor the health of the library and the service
- [Bug fixes](#bug-fixes)

## Device code flow improvements

The device code flow is used in the case of devices and operating systems that do not provide a web browser, such as applications running on iOT, or Command-Line tools (CLI). See [more information on the device code flow](https://aka.ms/msal-net-device-code-flow).

### Device code flow now works with Microsoft Personal Accounts

Starting with MSAL.NET 4.5 release, the device code flow is possible with Microsoft Personal Accounts. This means the device code flow will work with:
  - Any work and school accounts (tenanted authority, `https://login.microsoftonline.com/organizations/`), and
  - Microsoft personal accounts (`/common` or `/consumers` tenants)

### Device code flow now works with ADFS 2019

Starting with MSAL.NET 4.5 release, MSAL .NET supports the device code grant for [ADFS 2019](https://docs.microsoft.com/en-us/windows-server/identity/ad-fs/overview/whats-new-active-directory-federation-services-windows-server#suppport-for-building-modern-line-of-business-apps). 

## Telemetry Data

To better understand the reliability of the library and the Azure AD service across public client application calls and to try and detect outages and customer issues preemptively, MSAL .NET now sends telementy data to the /token endpoint in regards to the error code of the previous request, if applicable. This will help us be more proactive in detecting and fixing issues.

## Bug Fixes

MSAL.NET 4.5 and 4.5.1 also contains a number of bug fixes:
- **Customers reported a nonce mismatch error when signing in with the Authenticator app on iOS 13**. The issue has been resolved and increased logging included in the iOS broker scenario. See [issue](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1421) for more details.
- **On iOS 13, when using the system browser, authentication was broken**. This was because Apple now requires a presentationContext when signing in with the system browser. More information on this requirement [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/iOS-13-issue-with-system-browser-on-MSAL-.NET). And more details in the [issue](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1399)
- **At times, MSAL .NET would randomly fail on UWP.** MSAL .NET now implements retry logic and has improved logging around the cache in UWP. See this [issue](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1098) and this [issue](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1064) for more details.
- **During a client credential flow, MSAL .NET would throw a client exception stating the users should not add their own reserved scopes.** MSAL .NET now merges the scopes if they are already in the reserved list and does not throw. See [issue](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1422) for more details.
- **At times, during an interactive authentication, MSAL .NET would throw an ArgumentNullException**. MSAL .NET now checks for null values when handling the authorization result parsing. See [issue](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1418) for details. 