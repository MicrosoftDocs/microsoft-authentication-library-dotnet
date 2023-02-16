## MSAL.NET 2.6.1 released

Since [MSAL.NET 2.2.0 released](MSAL.NET-2.2.0-released), we shipped several versions of MSAL.NET. These releases brought bug fixes, and a few features based on your feedback. However we only communicated the changes through the release notes.

With MSAL.NET 2.6.1 (which  lost the "-preview" in its name), we thought it would be useful to update you on the progress made.

This blog post covers:

- [MSAL.NET helps you build multi-platform applications more easily](#msalnet-helps-you-build-multi-platform-applications-more-easily)
- [Better support for Azure AD B2C](#better-support-for-azure-ad-b2c)
- [Notable bug fixes](#Notable-bug-fixes)
- [Next steps](#Next-steps)

### MSAL.NET helps you build multi-platform applications more easily

#### Building a reusable library supporting several platforms isn't easy

MSAL.NET [supports multiple platforms](MSAL.NET-supports-multiple-application-architectures-and-multiple-platforms#msalnet-supports-multiple-platforms). When you want to write applications working on several platforms, you'll probably create a library targeting the MSAL.NET **.NET Standard 1.3** platform. However, the problem is in the case of multi-target projects, the assembly actually used by .NET is different:

- at coding or build time (IDE, msbuild)
- and at runtime (CLR).

The picture below shows the case of a .NET Core application and an Android application that reference a shared code library targeting the .NET standard platform.

![shared code resolution](https://github.com/bgavrilMS/Test/blob/master/netstandard.png?raw=true)

If you're interested in learning more about what is .NET standard, how .NET resolves platforms, what it means to build against .NET standard and what happens at runtime, read [Understanding multi targeting and NetStandard](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Understanding-multi-targetting-and-NetStandard).

#### Before MSAL.NET 2.6.0

Until MSAL.NET 2.6.0, there were differences between the public API surface exposed in different platforms (that is fine), but these differences were not always consistent with the public API of the .NET standard platform. Because of these inconsistencies, when you were developing shared code libraries, you could hit two kinds of issues:

- The concrete platforms could have more APIs than .NET standard, which was making your life difficult: You basically had to create multi-platform projects yourself, and use conditional compilation.
- Also, because the edit/build time and runtime assemblies are different, and the public API is different, you could get, at runtime, and on some platforms only, a `MethodNotImplementedException`, whereas your code was building fine. Debugging these exceptions was also difficult as the exceptions were not actionable.

#### MSAL.NET 2.6.0 brings a more rational .NET standard public API, with a few actionable PlatformNotSupportedException

With MSAL.NET 2.6.0, the public API of the .NET Standard platform was rethought so that you can more easily build cross-platform libraries:

- The public API was introduced each time it made sense for you to use it in the .NET standard assembly.
- For methods that do not make sense in some platforms, we chose to provide them in the .NET Standard platform: Either they do nothing, or they throw a meaningful and actionable `PlatformNotSupportedException`, which has an aka.ms link to MSAL.NET conceptual documentation.

Part of this rationalization, `UIParent` is now available on all platforms and `UIParent` gets a new constructor taking as parameters (`object parent, bool useEmbeddedWebview`).

The following table summarizes the cases when you can get a `PlatformNotSupportedException` when using a particular API in a .NET standard 1.3 library, depending on the actual platform of the app using this library.

If you attempt to use this API at runtime | in an app of the following platforms | you'll get a PlatformNotSupportedException with the following message
--- | --- | ---
Any confidential client API [ConfidentialClientApplication](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.confidentialclientapplication?view=azure-dotnet), [ClientCredential](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.clientcredential?view=azure-dotnet), [ClientAssertionCertificate](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.clientassertioncertificate?view=azure-dotnet) | Xamarin.iOS, Xamarin.Android, UWP | Confidential Client flows are not available on mobile platforms. See https://aka.ms/msal-net-confidential-availability for details
[Username password](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.publicclientapplication.acquiretokenbyusernamepasswordasync?view=azure-dotnet) | Xamarin.iOS, Xamarin.Android, UWP | The Username / Password flow is not supported on Xamarin.Android, Xamarin.iOS, and UWP. For more information, see https://aka.ms/msal-net-up
[IWA*](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.ipublicclientapplication.acquiretokenbyintegratedwindowsauthasync?view=azure-dotnet) | non-Windows platforms (iOS, Android) | Integrated Windows Authentication is not supported on this platform. For details about this authentication flow, see https://aka.ms/msal-net-iwa"
The overload of  [IWA*](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.ipublicclientapplication.acquiretokenbyintegratedwindowsauthasync?view=azure-dotnet) without username | .NET Core | This overload of AcquireTokenByIntegratedWindowsAuthAsync is not supported on .net core because MSAL cannot determine the username (UPN) of the currently logged in user. Use the overload where you pass in a username (UPN). For more information, see https://aka.ms/msal-net-iwa
The constructor of [PublicClientApplication](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.publicclientapplication.-ctor?view=azure-dotnet#Microsoft_Identity_Client_PublicClientApplication__ctor_System_String_System_String_Microsoft_Identity_Client_TokenCache_) taking a TokenCache parameter | Xamarin.iOS, Xamarin.Android, UWP | Don't use this constructor that takes in a TokenCache object on mobile platforms. This constructor is meant to allow applications to define their own storage strategy on .net desktop and .net core. On mobile platforms, a secure and performant storage mechanism is implemented by MSAL. For more details about custom token cache serialization, visit https://aka.ms/msal-net-serialization
The [methods](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.tokencacheextensions?view=azure-dotnet) enabling customization of token cache serialization | Xamarin.iOS, Xamarin.Android, UWP | You should not use these TokenCache methods object on mobile platforms. They meant to allow applications to define their own storage strategy on .net desktop and .net core. On mobile platforms, a secure and performant storage mechanism is implemented by MSAL. For more details about custom token cache serialization, visit https://aka.ms/msal-net-serialization
Any constructor of UIParent | .NET Core | Interactive Authentication flows are not supported on .net core. Consider using Device Code Flow https://aka.ms/msal-device-code-flow or Integrated Windows Auth https://aka.ms/msal-net-iwa
AcquireTokenAsync | .NET Core | On .NET Core, interactive authentication is not supported. Consider using Device Code Flow https://aka.ms/msal-net-device-code-flow or Integrated Windows Auth https://aka.ms/msal-net-iwa
overloads of AcquireTokenAsync without UIParent | Xamarin.Android | To enable interactive authentication on Android, please call an overload of `AcquireTokenAsync` that takes in a UIParent object, which you should initialize to an Activity. See https://aka.ms/msal-interactive-android for details.

(*) IWA = AcquireTokenByIntegratedWindowsAuthAsync (abbreviated so that the table keeps being readable)

In some cases, the .NET standard 1.3 assembly can also be used at runtime. That is, for instance, the case if the application you are building is for a platform that MSAL.NET does not support explicitly. Examples of such platforms are Xamarin.Mac, Unity, Mono, or old frameworks such as Windows Phone, Windows Phone Silverlight. In that case, things should work correctly (disclaimer: we are not testing them). But for the methods that would not really make sense, MSAL.NET also provides a meaningful explanation if you use this API:

If you attempt to use at runtime | in an app of non-explicitly supported platform (therefore defaulting to .NET Standard 1.3 runtime) | you'll get a PlatformNotSupportedException with the following message
--- | --- | ---
 Constructor of UIParent | .NET Core | Interactive Authentication flows are not supported on .net core. Consider using Device Code Flow https://aka.ms/msal-device-code-flow or Integrated Windows Auth https://aka.ms/msal-net-iwa"

Finally, there are cases where your .NET Standard 1.3 common code library calls AcquireTokenAsync (interactive). You can do it, however, in that particular case, you also need to reference MSAL.NET from the application assembly itself (referencing it from the common code library is not enough). A meaningful PlatformNotSupportedException will help you figure it out. Its message is: "Possible Cause: If you are using an XForms app, or generally a NetStandard assembly, make sure you add a reference to Microsoft.Identity.Client.dll from each platform assembly (e.g. UWP, Android, iOS), not just from the common net standard assembly."

#### MSAL.NET 2.6.0 brings reference assemblies

The MSAL.NET NuGet package now contains reference assemblies in addition to platform assemblies.
Platform assemblies are implementation assemblies. They are the ones used at runtime.
They have a matching set of reference assemblies, which are used at edit and build time. Think of a reference assembly as sort of "interface assembly": it has the .NET metadata, but not the IL body.

By adding references assemblies in the MSAL.NET NuGet package, MSAL.NET offers you the best of both worlds:

- The power of a complete .NET Standard public API when you want to write a common-code library reused on different platforms (see above)
- The IntelliSense when using MSAL.NET directly from a platform.

Reference assemblies, because they contain only metadata (no implementation), are tiny, and therefore their impact on the size of the NuGet package can be neglected.

A big thanks to Oren @onovotny, for his help on improving our .NET Standard story!

> In case you want to know how reference assemblies are generated, checkout Oren's post [Create and pack reference assemblies (made easy)](https://oren.codes/2018/07/09/create-and-pack-reference-assemblies-made-easy/)

### Better support for Azure AD B2C

The next area where you pushed us to improve is the support for Azure AD B2C.
Until MSAL.NET 2.5, you had two significant issues:

- Azure AD B2C has released a new authority (b2clogin.com) and is recommending that you now use `https://{your-tenant-name}.b2clogin.com/tfp/{your-tenant-ID}/{policyname}`  instead of the legacy `https://login.microsoftonline.com/tfp/{tenant}/{policyName}`. For more information, see this [documentation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/b2clogin).
- When you applied certain policies such as EditProfile, or ResetPassword, which apply to the user currently signed-in in your MSAL.NET public client application, your end users were forced to reselect the account on which to apply this policy. The user experience was confusing, as the user is known.

We fixed both:

#### MSAL.NET now supports b2clogin.com (from 2.5.0)

This is well illustrated by a quick code snippet, which previously threw an exception. This now works.

```CSharp
// Azure AD B2C Coordinates
public static string Tenant = "fabrikamb2c.onmicrosoft.com";
public static string ClientID = "90c0fe63-xxxx-yyyy-zzzz-b8bbc0b29dc6";
public static string PolicySignUpSignIn = "b2c_1_susi";
public static string PolicyEditProfile = "b2c_1_edit_profile";
public static string PolicyResetPassword = "b2c_1_reset";
public static string AuthorityBase = $"https://fabrikamb2c.b2clogin.com/tfp/{Tenant}/";

public static string Authority = $"{AuthorityBase}{PolicySignUpSignIn}";
public static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}";
public static string AuthorityPasswordReset = $"{AuthorityBase}{PolicyResetPassword}"

var application = new PublicClientApplication(ClientID, Authority);
```

Note that when you use an authority based on b2clogin.com and you want to use Google as an identity provider, you need to use the system browser, (which is the default on Xamarin.iOS and Xamarin.Android), as Google does not allow authentication from embedded webview other than `login.microsoftonline.com`). For more information, see [Google Auth and Embedded Webview](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/AAD-B2C-specificities#google-auth-and-embedded-webview)

#### Improved experience with EditProfile, ResetPassword

When you want to provide an experience where your end-users sign in with a social authority, and then edit their profile, you want to apply the B2C EditProfile policy. The way to do this, is by calling `AcquireTokenAsync` with
the specific authority for that policy.
Unfortunately, until recently, `AcquireTokenAsync` was always sending a prompt parameter to the B2C service and the UIBehavior did not offer the possibility of not prompting. As a result the users, even if they were signed-in, in the application, had to reselect the identity for which they wanted to edit the profile, which was a confusing experience.

MSAL.NET 2.6.0-preview brings a new constant named `UIBehavior.NoPrompt` to use for the behavior parameter in this case of applying B2C policies not requiring an account selection.

```CSharp
private async void EditProfileButton_Click(object sender, RoutedEventArgs e)
{
 IEnumerable<IAccount> accounts = await app.GetAccountsAsync();
 try
 {
  var authResult = await app.AcquireTokenAsync(scopes:App.ApiScopes,
                                               acount:GetUserByPolicy(accounts, App.PolicyEditProfile),
                                               behavior:UIBehavior.NoPrompt,
                                               extraQueryParameters:string.Empty,
                                               extraScopesToConsent:null,
                                               authority:App.AuthorityEditProfile);
  DisplayBasicTokenInfo(authResult);
 }
 catch
 {
  . . .
}
```

> You can find more information about MSAL.NET specificities for B2C in https://aka.ms/msal-net-b2c-specificities

### Notable bug fixes

We also fixed a number of bugs:

- **AcquireTokenSilentAsync** now correctly handles resources in different tenants, and honors the force refresh parameter correctly (See [MSAL issue #695](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/695) and [MSAL issue #694](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/694))
- **Support for non-connected mode**: To our surprise, several of you asked us to support the non-connected mode on Xamarin.Android and Xamarin.iOS: if the user is already signed-in to the app, but the phone is not connected to the Internet, you wanted to be able to still access the `IAccount`. We also fixed that.
- **Token cache custom serialization**: In some cases where your custom cache serialization logic was using the `Count` method of the cache, you were getting an exception (See [ADAL issue #1186](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/issues/1186)). We fixed it in MSAL.NET, as well as ADAL.NET, and on the way, we deprecated the `HasStateChanged` member of `TokenCache` which was not used by MSAL.NET, removing a flag
- **Obsoleting types from internal namespaces**. We obsoleted the public `WebUI` net45 types from the Internal.UI namespace. They should never have been exposed in the first place, and will be hidden in the next major version of MSAL.NET

### Next steps

The next steps will be about:

- Facilitating the migration of ADAL v2.0 application (for which the refresh token was exposed) to MSAL.NET: we'll offer a new method to AcquireTokenFromRefreshToken.
- Facilitating the initialization of PublicClientApplication and ConfidentialClientApplication from configuration files.
- Facilitating the acquisition of tokens interactively in platforms supporting it: PubliClientApplication currently has 14 overloads of AcquireTokenAsync, and we'd need more parameters to handle conditional access nicely, therefore we'll propose another approach with one overload taking parameters. More about that soon ...
- Enabling Xamarin.iOS and Xamarin.Android applications to leverage a new version of the broker (Company portal, Microsoft Authenticator) supporting the Azure AD v2.0 endpoint. This will enable your mobile applications to benefit from Device identification, which can be a pre-requisite for some Azure AD conditional access policies.