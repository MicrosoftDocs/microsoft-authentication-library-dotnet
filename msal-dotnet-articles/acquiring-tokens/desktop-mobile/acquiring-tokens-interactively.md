# `AcquireTokenInteractive` API

The method to use to acquire a token interactively is `IPublicClientApplication.AcquireTokenInteractive`

The following example shows minimal code to get a token for reading the user's profile with Microsoft Graph.

```csharp
string[] scopes = new string[] {
  "user.read"
};
var app = PublicClientApplicationBuilder.Create(clientId).Build();
var accounts = await app.GetAccountsAsync();
AuthenticationResult result;
try {
  result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
    .ExecuteAsync();
} catch (MsalUiRequiredException) {
  result = await app.AcquireTokenInteractive(scopes)
    .ExecuteAsync();
}
```

## Mandatory parameters

`AcquireTokenInteractive` has only one mandatory parameter ``scopes``, which contains an enumeration of strings which define the scopes for which a token is required. If the token is for the Microsoft Graph, the required scopes can be found in api reference of each Microsoft graph API in the section named "Permissions". For instance, to [list the user's contacts](/graph/api/user-list-contacts?view=graph-rest-1.0&tabs=http), the scope "User.Read", "Contacts.Read" will need to be used. See also [Microsoft Graph permissions reference](/graph/permissions-reference).

On Android, you need to also specify the parent activity (using `.WithParentActivityOrWindow`, see below) so that the token gets back to that parent activity after the interaction. If you don't specify it, an exception will be thrown when calling `.ExecuteAsync()`.

## Specific optional parameters

### WithParentActivityOrWindow

Being interactive, UI is important. `AcquireTokenInteractive` has one specific optional parameters enabling to specify, for platforms supporting it, the parent UI (window in Windows, Activity in Android). This parent UI is specified using `.WithParentActivityOrWindow()`. The UI dialog will typically be centered on that parent. As explained above, on Android the parent activity is a mandatory parameter.

`.WithParentActivityOrWindow` has a different type depending on the platform:

```CSharp
// Android
WithParentActivityOrWindow(Activity activity)

// net45
WithParentActivityOrWindow(IntPtr windowPtr)
WithParentActivityOrWindow(IWin32Window window)

// Mac
WithParentActivityOrWindow(NSWindow window)

// iOS
WithParentActivityOrWindow(IUIViewController viewController)

// .Net Standard (this will be on all platforms at runtime, but only on NetStandard at build time)
WithParentActivityOrWindow(object parent).
```

Remarks:

- On .NET Standard, the expected `object` is an `Activity` on Android, a `UIViewController` on iOS, an `NSWindow` on MAC, and a `IWin32Window` or `IntPr` on Windows.
- On Windows, you must call `AcquireTokenInteractive` from the UI thread so that the embedded browser gets the appropriate UI synchronization context.  Not calling from the UI thread may cause messages to not pump properly and/or deadlock scenarios with the UI. One way of achieving this, if you are not on the UI thread is to use the `Dispatcher` on WPF.
- If you are using WPF, to get a window from a WPF control, you can use  `WindowInteropHelper.Handle` class. The call is then, from a WPF control (this):
  
  ```CSharp
  result = await app.AcquireTokenInteractive(scopes)
                    .WithParentActivityOrWindow(new WindowInteropHelper(this).Handle)
                    .ExecuteAsync();
  ```

### WithPrompt

With Prompt is used to control the interactivity with the user by specifying a Prompt.

The class defines the following constants:

- ``SelectAccount``: will force the STS to present the account selection dialog containing accounts for which the user has a session. This is useful when applications developers want to let user choose among different identities. This is done by sending ``prompt=select_account`` to the identity provider. This is the default, and it does of good job of providing the best possible experience based on the available information (account, presence of a session for the user, etc ...). You should not change it unless you have good reason to do it.
- ``Consent``: enables the application developer to force the user be prompted for consent even if consent was granted before. This is done by sending `prompt=consent` to the identity provider. This can be used in some security focused applications where the organization governance demands that the user is presented the consent dialog each time the application is used.
- ``ForceLogin``: enables the application developer to have the user prompted for credentials by the service even if this would not be needed. This can be useful if Acquiring a token fails, to let the user re-sign-in. This is done by sending `prompt=login` to the identity provider. Again, we've seen it used in some security focused applications where the organization governance demands that the user re-logs-in each time they access specific parts of an application.
- ``Create`` triggers a sign-up experience, which is used for External Identities, by sending `prompt=create` to the identity provider. This is available in MSAL.NET 4.29.0+. This prompt should not be sent for Azure AD B2C apps. For more information, see [Add a self-service sign-up user flow to an app](https://aka.ms/msal-net-prompt-create).
- ``Never`` (for .NET 4.5 and WinRT only) will not prompt the user, but instead will try to use the cookie stored in the hidden embedded web view (See below: Web Views in MSAL.NET). This might fail, and in that case `AcquireTokenInteractive` will throw an exception to notify that a UI interaction is needed, and you'll need to use another `Prompt` parameter.
- ``NoPrompt``: Won't send any prompt to the identity provider. This is actually only useful in the case of B2C edit profile policies (See [B2C specifics](./social-identities.md)).

### WithUseEmbeddedWebView

Enables you to specify if you want to force the usage of an embedded web view or the system web view (when available). For more details see [Usage of Web browsers](../../how-to/usage-of-web-browsers.md).

 ```CSharp
 result = await app.AcquireTokenInteractive(scopes)
                   .WithUseEmbeddedWebView(true)
                   .ExecuteAsync();
  ```

### WithExtraScopeToConsent

This is used in an advanced scenario where you want the user to pre-consent to several resources upfront (and don't want to use the incremental consent which is normally used with MSAL.NET / the Microsoft identity platform v2.0). For details see [How-to : have the user consent upfront for several resources](#have-the-user-consent-upfront-for-several-resources) below

```CSharp
var result = await app.AcquireTokenInteractive(scopesForCustomerApi)
                     .WithExtraScopeToConsent(scopesForVendorApi)
                     .ExecuteAsync();
```

## Other optional parameters (common to many AcquireToken methods)

Builder modifier | Description
-- | --
WithAuthority (7 overrides) | Overrides the authority
WithAdfsAuthority(string) | Overrides the authority
WithB2CAuthority(string) | Overrides the authority
WithAccount(IAccount) | ``account`` (optional) of type ``IAccount``, provides a hint to the STS about the user for which to get the token. This can be set from the `Account` member of a previous AuthenticationResult, or one of the elements of the collection returned by ``GetAccountsAsync()`` method of the ``PublicClientApplication``.
WithLoginHint(string) | ``loginHint`` (optional) offers a hint to the STS about the user for which to get the token alternative to user. It's used like ``userIdentifier`` in ADAL. Needs to be passed the `preferred_username` of the IDToken (contrary to ADAL which was requiring the UPN)
WithClaims(string) | Requests additional claims. This normally is in reaction to an MsalClaimChallengeException which has a Claim member (Conditional Access)
WithPrompt(Prompt) | Is the way to control, in MSAL.NET, the interaction between the user and the STS to enter credentials. It's different depending on the platform (See below). Note that MSAL 3.x is taking a breaking change here. Prompt used to be named ``UIBehavior`` in MSAL 1.x and 2.x
WithExtraQueryParameters(dictionary) | A dictionary of keys / values.
WithExtraScopesToConsent(extraScopes) | Enables application developers to specify additional scopes for which users will pre-consent. This can be in order to avoid having them see incremental consent screens when the Web API require them. This is also indispensable in the case where you want to provide scopes for several resources. See [the paragraph on getting consent for several resources](#have-the-user-consent-upfront-for-several-resources) below for more details.

## The different browsers
|                                       | **Pro**                                                                                                                                                                                                                                                 | **Con**                                                                                                                                                                       |
|------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Embedded WebView1 (based on Internet Explorer) | - Ships with all supported versions of Windows <br> - In use by identity libs for 10+ years                                                                                                                                                             | - No FIDO (e.g. YubiKeys) <br> - No Windows Hello <br> - Conditional Access problems on some older Windows versions <br> - Windows only                                       |
| Embedded WebView2 (based on Microsoft Edge)    | - FIDO and Windows Hello support                                                                                                                                                                                                                        | - Does not ship in the box yet (?). Deployment is difficult. <br> - Conditional Access problems on some older Windows versions. <br> - Windows only                           |
| System browser                                 | - Uses default system browser, to which customer is used to. <br> - Chrome, Edge (and possibly Mozilla) have integration with Conditional Access, Win Hello, FIDO <br> - Works on Mac and Linux and every possible version of Windows.                  | - User experience is not as good, for example if user navigates away from the browser, the app doesn’t know and keep on waiting.                                              |
| WAM                                            | - FIDO, Hello, Conditional Access <br> - Fully integrated with Windows <br> - Better security <br> - This is the North Star of the Identity team; many experiences will light up with WAM use! <br> - See https://aka.ms/msal-net-wam for all benefits. | - Legacy MSA-passthrough config does not work. We recommend creating a new app if to move away from MSA-passthrough. <br> - Windows only (10+, Server 2016 and Server 2019+). |

## How to

### Have the user consent upfront for several resources

> Note: Getting consent for several resources works for Azure AD v2.0, but not for Azure AD B2C. B2C supports only admin consent, not user consent.

The Azure AD v2.0 endpoint does not allow you to get a token for several resources at once. Therefore the scopes parameter should only contain scopes for a single resource. However, you can ensure that the user pre-consents to several resources by using the `extraScopesToConsent` parameter.

For instance if you have two resources, which have 2 scopes each:

- `https://mytenant.onmicrosoft.com/customerapi` (with 2 scopes `customer.read` and `customer.write`)
- `https://mytenant.onmicrosoft.com/vendorapi` (with 2 scopes `vendor.read` and `vendor.write`)

you should use the .WithAdditionalPromptToConsent modifier which has the `extraScopesToConsent` parameter

For instance:

```CSharp
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

This will get you an access token for the first Web API.
Then when you need to call the second one, you can call

```CSharp
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
| [active-directory-dotnet-desktop-msgraph-v2](http://github.com/azure-samples/active-directory-dotnet-desktop-msgraph-v2) | Desktop (WPF) | Windows Desktop .NET (WPF) application calling the Microsoft Graph API. ![](https://github.com/Azure-Samples/active-directory-dotnet-desktop-msgraph-v2/blob/master/ReadmeFiles/Topology.png) |
| [active-directory-dotnet-native-uwp-v2](https://github.com/azure-samples/active-directory-dotnet-native-uwp-v2) | UWP | A Windows Universal Platform client application using msal.net, accessing the Microsoft Graph for a user authenticating with Azure AD v2.0 endpoint. ![](https://github.com/Azure-Samples/active-directory-dotnet-native-uwp-v2/blob/master/ReadmeFiles/Topology.png) |
| [https://github.com/Azure-Samples/active-directory-xamarin-native-v2](https://github.com/Azure-Samples/active-directory-xamarin-native-v2) | Xamarin iOS, Android, UWP | A simple Xamarin Forms app showcasing how to use MSAL to authenticate MSA and Azure AD via the AADD v2.0 endpoint, and access the Microsoft Graph with the resulting token. ![](https://github.com/Azure-Samples/active-directory-xamarin-native-v2/blob/master/ReadmeFiles/Topology.png) |
| [https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2) | WPF, ASP.NET Core 2.0 Web API | A WPF application calling an ASP.NET Core Web API using Azure AD v2.0. ![](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/blob/master/ReadmeFiles/topology.png) |
