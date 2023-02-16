MSAL throws a few types of exceptions, please see [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Exceptions).

# Confidential Client

Please read the guide on [High Availability](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/High-availability)

# Public Client

### Device Compliance failures on Windows 10

Users are unable to login interactively and a "Device is not compliant" error is shown when:

* the tenant admin has enabled the "Require device to be marked as compliant" Conditional Access policy
* the app is invoking public client flows (i.e. rich client apps, not web sites)
* the app is using the embedded browser control available in ADAL or MSAL (this is the default for .NET Framework apps)

#### Mitigation
* the recommended approach is to use [WAM](wam)
* Otherwise, you can also configure MSAL to use the system (default OS) browser (details [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-web-browsers#how-to-use-the-default-os-browser)). Both Chrome and Microsoft Edge browsers are able to satisfy the device policy. 
* if using ADAL, migrate to MSAL first. There is no mitigation for ADAL use.

### Android 

On Android, an `AndroidActivityNotFound` exception is thrown when the device does not have a browser with tabs 
https://docs.microsoft.com/en-gb/azure/active-directory/develop/msal-net-system-browser-android-considerations#known-issues

### iOS

Please see: https://docs.microsoft.com/en-gb/azure/active-directory/develop/msal-net-xamarin-ios-considerations#known-issues-with-ios-12-and-authentication

### UWP

The recommended approach is to use [WAM](wam)

Most issues on UWP occur due to network problems, such as proxies that block the traffic etc. Integrated Windows Auth may also be blocked by admins. For more details see: 

https://docs.microsoft.com/en-gb/azure/active-directory/develop/msal-net-uwp-considerations#troubleshooting

### Desktop

On a Desktop app, a `StateMismatchError` exception is thrown when the using a long Facebook ID (via B2C) in conjunction with the embedded browser.
For more details, please refer: https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/StateMismatchError

# Build issues

Behaviour: an error similar to `Microsoft.Windows.SDK.Contracts.targets(4,5): error : Must use PackageReference` is thrown

Starting with version 4.23, MSAL references `Microsoft.Windows.SDK.Contracts`. NuGet can only resolve this reference if the application consuming MSAL references it as `<PackageReference>` and not via the legacy `packages.config` mechanism. See #2247 for details on how to fix this.