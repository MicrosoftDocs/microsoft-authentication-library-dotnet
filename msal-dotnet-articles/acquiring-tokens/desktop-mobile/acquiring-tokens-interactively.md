---
title: Acquiring tokens interactively
description: "How to acquire tokens with MSAL.NET and user interaction."
---

# Acquiring tokens interactively

Interactive token acquisition requires that the user _interacts_ with an authentication dialog that requests credentials as input and is only available for public client applications. In MSAL.NET, the method to use to acquire a token interactively is <xref:Microsoft.Identity.Client.PublicClientApplication.AcquireTokenInteractive(System.Collections.Generic.IEnumerable{System.String})>.

The following example shows barebones code to get a token for reading the user's profile with Microsoft Graph:

```csharp
string[] scopes = new string[] { "user.read" };

var app = PublicClientApplicationBuilder.Create("YOUR_CLIENT_ID")
    .WithDefaultRedirectUri()
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
    result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
}
```

>[!NOTE]
>To use <xref:Microsoft.Identity.Client.ClientApplicationBase.AcquireTokenSilent(System.Collections.Generic.IEnumerable{System.String},Microsoft.Identity.Client.IAccount)> the developer needs to set up a token cache. Without a token cache, the interactive prompt will always be shown, even if the user has previously logged in. To learn more about setting up a token cache, refer to [Token cache serialization in MSAL.NET](../../how-to/token-cache-serialization.md).

## Mandatory parameters

`AcquireTokenInteractive` has only one mandatory parameter - `scopes`, which contains an enumeration of strings that define the scopes for which a token is required. If the token is for Microsoft Graph, the required scopes can be found in the API reference of each Microsoft Graph API in the section named **Permissions**. For instance, to [list the user's contacts](/graph/api/user-list-contacts), the `User.Read` and `Contacts.Read` scopes will need to be used. For additional information, refer to the [Microsoft Graph permissions reference](/graph/permissions-reference).

On Android, you also need to specify the parent activity using <xref:Microsoft.Identity.Client.PublicClientApplicationBuilder.WithParentActivityOrWindow(System.Func{System.IntPtr})>, ensuring that the token gets back to the parent activity after the interaction is complete. If you don't specify it, an exception will be thrown.

## Specific optional parameters

### WithParentActivityOrWindow

<xref:Microsoft.Identity.Client.PublicClientApplication.AcquireTokenInteractive(System.Collections.Generic.IEnumerable{System.String})> has one optional parameter that enables developers to provide a reference to the parent UI component (e.g., window in Windows, activity in Android). This parent UI is specified using <xref:Microsoft.Identity.Client.PublicClientApplicationBuilder.WithParentActivityOrWindow(System.Func{System.IntPtr})>. The UI dialog will typically be centered on that parent. As explained above, on Android the parent activity is a _required_ parameter.

<xref:Microsoft.Identity.Client.PublicClientApplicationBuilder.WithParentActivityOrWindow(System.Func{System.IntPtr})> has a different argument type, depending on the platform where it is used:

```csharp
// Android
WithParentActivityOrWindow(Activity activity)

// .NET Framework
WithParentActivityOrWindow(IntPtr windowPtr)
WithParentActivityOrWindow(IWin32Window window)

// macOS
WithParentActivityOrWindow(NSWindow window)

// iOS
WithParentActivityOrWindow(IUIViewController viewController)

// .NET Standard (this will be on all platforms at runtime, but only on .NET Standard at build time)
WithParentActivityOrWindow(object parent).
```

Remarks:

- On .NET Standard, the expected `object` is:
  - `Activity` on Android.
  - `UIViewController` on iOS.
  - `NSWindow` on macOS.
  - `IWin32Window` or `IntPr` on Windows.
- On Windows, you must call <xref:Microsoft.Identity.Client.PublicClientApplication.AcquireTokenInteractive(System.Collections.Generic.IEnumerable{System.String})> from the UI thread so that the embedded browser gets the appropriate UI synchronization context.  Not calling from the UI thread may cause messages to not pump properly and/or deadlock scenarios with the UI. One way of achieving this if you are not on the UI thread is to use <xref:System.Windows.Threading.Dispatcher>.
- If you are using WPF to get a window from a WPF control you can use  <xref:System.Windows.Interop.WindowInteropHelper.Handle>:
  
  ```csharp
  result = await app.AcquireTokenInteractive(scopes)
                    .WithParentActivityOrWindow(new WindowInteropHelper(this).Handle)
                    .ExecuteAsync();
  ```

### WithPrompt

With Prompt is used to control the interactivity with the user by specifying a Prompt.

The class defines the following constants:

- `SelectAccount`: will force the STS to present the account selection dialog containing accounts for which the user has a session. This is useful when applications developers want to let user choose among different identities. This is done by sending `prompt=select_account` to the identity provider. This is the default, and it does of good job of providing the best possible experience based on the available information (account, presence of a session for the user, etc ...). You should not change it unless you have good reason to do it.
- `Consent`: enables the application developer to force the user be prompted for consent even if consent was granted before. This is done by sending `prompt=consent` to the identity provider. This can be used in some security focused applications where the organization governance demands that the user is presented the consent dialog each time the application is used.
- `ForceLogin`: enables the application developer to have the user prompted for credentials by the service even if this would not be needed. This can be useful if Acquiring a token fails, to let the user re-sign-in. This is done by sending `prompt=login` to the identity provider. Again, we've seen it used in some security focused applications where the organization governance demands that the user re-logs-in each time they access specific parts of an application.
- `Create` triggers a sign-up experience, which is used for External Identities, by sending `prompt=create` to the identity provider. This is available in MSAL.NET 4.29.0+. This prompt should not be sent for Azure AD B2C apps. For more information, see [Add a self-service sign-up user flow to an app](/azure/active-directory/external-identities/self-service-sign-up-user-flow).
- `Never` (for .NET 4.5 and WinRT only) will not prompt the user, but instead will try to use the cookie stored in the hidden embedded web view (See below: Web Views in MSAL.NET). This might fail, and in that case `AcquireTokenInteractive` will throw an exception to notify that a UI interaction is needed, and you'll need to use another `Prompt` parameter.
- `NoPrompt`: Won't send any prompt to the identity provider. This is actually only useful in the case of B2C edit profile policies (See [B2C specifics](./social-identities.md)).

### WithUseEmbeddedWebView

Enables you to specify if you want to force the usage of an embedded web view or the system web view (when available). For more details see [Usage of Web browsers](/azure/active-directory/develop/msal-net-web-browsers).

 ```csharp
 result = await app.AcquireTokenInteractive(scopes)
                   .WithUseEmbeddedWebView(true)
                   .ExecuteAsync();
```

### WithExtraScopeToConsent

This is used in an advanced scenario where you want the user to pre-consent to several resources upfront (and don't want to use the incremental consent which is normally used with MSAL.NET / the Microsoft identity platform v2.0). For details see [How-to : have the user consent upfront for several resources](#have-the-user-consent-upfront-for-several-resources) below

```csharp
var result = await app.AcquireTokenInteractive(scopesForCustomerApi)
                     .WithExtraScopeToConsent(scopesForVendorApi)
                     .ExecuteAsync();
```

## The different browsers

| Browser           | Pro                  | Con                |
|:------------------|:---------------------|:-------------------|
| Embedded WebView1 (based on Internet Explorer) | - Ships with all supported versions of Windows <br>- In use by identity libraries for 10+ years | - No FIDO support (e.g., YubiKey)<br>- No support for Windows Hello<br>- Conditional Access problems on older Windows versions<br>- Windows only |
| Embedded WebView2 (based on Microsoft Edge)    | - FIDO and Windows Hello support | - Conditional Access problems on some older Windows versions.<br>- Windows only |
| System browser                                 | - Uses default system browser.<br>- Chrome, Edge, and Firefox have integration with Conditional Access, Windows Hello, and FIDO.<br>- Works on macOS, Linux, and every possible version of Windows. | - Somewhat disruptive user experience (context switches to the browser). |
| [Windows Broker](./wam.md)                     | - Support for FIDO, Windows Hello, and Conditional Access policies.<br>- Fully integrated with Windows.<br>- Better security.<br>- Long-term strategic component for authentication on Windows.<br> | - Legacy MSA-passthrough configuration does not work. We recommend creating a new app if you move away from MSA-passthrough.<br>- Windows only (10+, Server 2016, and Server 2019+). |

## How to

### Have the user consent upfront for several resources

>[!NOTE]
> Getting consent for several resources works for Azure AD v2.0, but not for Azure AD B2C. B2C supports only admin consent, not user consent.

The Azure AD v2.0 endpoint does not allow you to get a token for several resources at once. Therefore the scopes parameter should only contain scopes for a single resource. However, you can ensure that the user pre-consents to several resources by using the `extraScopesToConsent` parameter.

For instance if you have two resources, which have 2 scopes each:

- `https://mytenant.onmicrosoft.com/customerapi` (with 2 scopes `customer.read` and `customer.write`)
- `https://mytenant.onmicrosoft.com/vendorapi` (with 2 scopes `vendor.read` and `vendor.write`)

you should use the .WithAdditionalPromptToConsent modifier which has the `extraScopesToConsent` parameter

For instance:

```csharp
string[] scopesForCustomerApi = new string[]
{
  "https://mytenant.onmicrosoft.com/customerapi/customer.read",
  "https://mytenant.onmicrosoft.com/customerapi/customer.write"
};
string[] scopesForVendorApi = new string[]
{
 "https://mytenant.onmicrosoft.com/vendorapi/vendor.read",
 "https://mytenant.onmicrosoft.com/vendorapi/vendor.write"
};

var accounts = await app.GetAccountsAsync();
var result = await app.AcquireTokenInteractive(scopesForCustomerApi)
                     .WithAccount(accounts.FirstOrDefault())
                     .WithExtraScopeToConsent(scopesForVendorApi)
                     .ExecuteAsync();
```

This will get you an access token for the first Web API. Then when you need to call the second one, you can call

```csharp
AcquireTokenSilent(scopesForVendorApi, accounts.FirstOrDefault()).ExecuteAsync();
```

See [this](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/550#issuecomment-383572227) GitHub issue for more context.

## Microsoft personal account require re-consenting each time the app is run

For Microsoft personal accounts users, re-prompting for consent on each native client call to authorize is the intended behavior. Native client identity is inherently insecure, and the Microsoft identity platform chose to mitigate this insecurity for consumer services by prompting for consent each time the application is authorized.

## More specificities depending on the platforms

Depending on the platforms, you will need to do a bit of extra work to use MSAL.NET. For more details on each platform, see:

- [Configuration requirements and troubleshooting tips for Xamarin Android with MSAL.NET](/azure/active-directory/develop/msal-net-xamarin-android-considerations)
- [Considerations for using Xamarin iOS with MSAL.NET](/azure/active-directory/develop/msal-net-xamarin-ios-considerations)
- [UWP specifics](./uwp.md)

## Samples illustrating acquiring tokens interactively with MSAL.NET

| Sample | Platform | Description |
|------ | -------- | ----------- |
| [active-directory-dotnet-desktop-msgraph-v2](https://github.com/azure-samples/active-directory-dotnet-desktop-msgraph-v2) | Desktop (WPF) | Windows Desktop .NET (WPF) application calling the Microsoft Graph API. ![WPF app topology](../../media/wpf-app-topology.png) |
| [active-directory-dotnet-native-uwp-v2](https://github.com/azure-samples/active-directory-dotnet-native-uwp-v2) | UWP | A Windows Universal Platform client application using MSAL.NET, accessing the Microsoft Graph for a user authenticating with Azure AD v2.0 endpoint. ![UWP app topology](../../media/uwp-app-topology.png) |
| [https://github.com/Azure-Samples/active-directory-xamarin-native-v2](https://github.com/Azure-Samples/active-directory-xamarin-native-v2) | Xamarin iOS, Android, UWP | A simple Xamarin Forms app showcasing how to use MSAL to authenticate Microsoft accounts and Microsoft Entra ID via the Microsoft identity platform endpoint, and access the Microsoft Graph with the resulting token. ![Xamarin Forms app topology](../../media/xamarin-forms-topology.png) |
| [https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2) | WPF, ASP.NET Core 2.0 Web API | A WPF application calling an ASP.NET Core Web API using Azure AD v2.0. ![Desktop and web app interaction topology](../../media/desktop-web-topology.png) |
