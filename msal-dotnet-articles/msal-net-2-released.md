> MSAL.NET 2.0.0-preview was released on August 28th, along with ADAL.NET 4.0.0-preview. See details in the [blog post](https://aka.ms/adal-4-msal-2-preview)

> MSAL.NET 2.1.0-preview was released on Sept 27th. This release adds support for [Integrated Windows Auth](https://aka.ms/msal-net-iwa) and [Username/Password](https://aka.ms/msal-net-up)

## Changes in MSAL.NET between 1.x and 2.x

MSAL.NET 2.x brings the following changes:

- [You can now share the token cache](#you-can-enable-sso-between-adalnet-3x-applications-adalnet-4x-applications-and-msal-on-the-same-platform) between ADAL 3.x, ADAL 4.x, and MSAL 2.x applications, and also between Xamarin.iOS and native iOS
- You can now decide to [use an embedded web browser](#you-can-now-leverage-the-embedded-web-browser-in-xamarinios-and-xamarinandroid) on Xamarin.iOS and Xamarin.Android applications instead of the system browser which is used by default.
- [The supported platforms were modernized](#modernization-of-the-platforms-supported-by-msalnet), dropping support for Windows and Windows Phone 8/8.1, and supporting .NET core explicitly
- [`IUser` is replaced with `IAccount`](#iuser-is-replaced-by-iaccount) and the methods to get an `IAccount` are now [asynchronous](#getting-an-account-is-now-asynchronous). These are breaking changes, but we provide you with guidance on the [impact](#impact-of-this-user---account-change-on-the-public-api) on your MSAL 1.x applications

In this article, you will also find pointers on:

- How to [migrate your applications from MSAL 1.x to MSAL 2.x](#migrating-from-msalnet-1x-to-msalnet-2x)
- We've also started documenting how to [migrate your applications from ADAL 3.x or 4.x and MSAL 2.x](#migrating-from-adalnet-3x-or-4x-to-msalnet-2x)

### Modernization of the platforms supported by MSAL.NET

#### Supported platforms

As in ADAL.NET, the platforms supported in MSAL.NET were modernized:

- Support for Windows 8/8.1 and Windows Phone 8/8.1 was removed
- An explicit platform for .NET Core was provided as the previous implementation was falling back to .NET standard, and was still exposing methods which were not usable in .NET Core, such as interactive token acquisition. These methods were throwing exceptions. To understand why .NET Core cannot support interactive token acquisition, see [MSAL.NET uses web browser](https://aka.ms/msal-net-uses-web-browser)

To summarize, MSAL.NET 2.0.0-preview now exposes the following platforms:

Platform | Usage
-------- | ------
net45 | .NET Framework for Desktop / Web apps
netcoreapp1.0 | .NET Core (for portable desktop and web apps)
uap10.0 | Windows 10 applications
Xamarin.iOS10 | Xamarin iOS applications
MonoAndroid8.1 | Xamarin Android applications
Netstandard1.3 | Everything else

#### Details on the new netcoreapp1.0 platform

The `netcoreapp1.0` platform is very similar to the .NET Standard 1.3 platform except for these differences:

- It defines `IPublicClientApplication` / `PublicClientApplication`, but these don't have the methods to acquire tokens interactively as .NET Core does not provide a web browser control. In fact, it currently does not provide any method at all as we have not yet implemented the other public client application flows (Integrated Windows Authentication, Username/Password, and Device code flow). Those are our next priorities, so expect additions here.
- It does not define the `UIBehavior` and `UIParent` classes as these are UI-related.

#### Upgrading portable class libraries to .NET Standard 1.3

MSAL v2.x no longer supports `netstandard1.1`, portable class libraries (PCL). 

This can impact you, in particular Xamarin Forms common project were often PCL at some point. You will need to upgrade your Xamarin PCL library to `netstandard 1.3` or higher. Luckily, this is fairly simple (it took us only a few minutes to update this MSAL Xamarin sample: [active-directory-xamarin-native-v2](https://github.com/Azure-Samples/active-directory-xamarin-native-v2) from a PCL to a netstandard1.3 library). To make the switch, you can, for instance use the following blog post [How to Convert a Portable Class Library to .NET Standard and Keep Git History](https://montemagno.com/how-to-convert-a-pcl-library-to-net-standard-and-keep-git-history/) (there is even a step by step video!). 

In the case of Xamarin.Forms, you might also have to update the `Microsoft.NETCore.UniversalWindowsPlatform` package if you were using the older UWP package. And that's it!
Of course, you'll also have to change some code because of changes to the public APIs. That's addressed in the next paragraph.

### Changes to MSAL.NET's public API

#### IUser is replaced by IAccount

##### Why replace User by Account?

Previous versions of MSAL.NET were defining the notion of user through the `IUser` interface.

![IAccount in MSAL 1.x](https://user-images.githubusercontent.com/13203188/44722725-3ab93000-aace-11e8-8f14-ef350bc975b9.png)

However:

- A user is a human or software agent.
- A user can possess/own/be responsible for one or more accounts in the Microsoft identity system (several Azure AD accounts, Azure AD B2C, Microsoft personal accounts). MSAL currently only supports Microsoft identity platform accounts. It does not support other types of accounts.
- A Microsoft identity platform account is created within a "home" context. For organizational users, this is their home Azure Active Directory.
- A Microsoft identity platform account may be represented in multiple directories within the Microsoft identity system. This is the guest scenario (B2B). In case you are interested in the details, it's represented as a separate account with a link/pointer back to the original. It's represented in those additional directories in order to convey its permissions within the organization that owns that directory. Permissions to the directory itself, Azure Resources, etc.....
- A Microsoft identity platform account may not be represented in multiple cloud environments (like national or sovereign clouds).
- The Identifier of an account was made of two segments separated by a comma. These were the base64 encoded representation of the object ID of the account in the targeted tenant (therefore not the home tenant), and the base64 encoded representation of the tenant ID.
- `Name`  often was pretty strange, tying to concatenate claims as there is no standard claim for a name.

##### `IAccount`

MSAL.NET 2.x now defines the notion of Account (through the `IAccount` interface). This breaking change provides the right semantics: the fact that the same user can have several accounts, in different Azure AD directories. Also MSAL.NET provides better information in the case of guest scenarios, as home account information is provided.
The following diagram shows the structure of the `IAccount` interface:

![IAccount](https://user-images.githubusercontent.com/13203188/44722040-19574480-aacc-11e8-8d88-96e20eb89919.png)

The `AccountId` class identifies an account in a specific tenant. It has the following properties:

Property | Description
--------- | ---------
`TenantId` | A string representation for a GUID, which is the ID of the tenant where the account resides
` ObjectId` | A string representation for a GUID which is the ID of the **user** who owns the account in the tenant
`Identifier` | Unique identifier for the account (this is the concatenation of ObjectId and TenantId separated by a comma and are not base64 encoded)

The `IAccount` interface represents information about a single account. The same user can be present in different tenants, that is, a user can have multiple accounts. Its members are:

Property | Description
---------|---------
`Username` | A string containing the displayable value in UserPrincipalName (UPN) format, for example, [john.doe@contoso.com](mailto:john.doe@contoso.com). This can be null, whereas the `HomeAccountId` and `HomeAccountId.Identifier` won't be null. This property replaces the `DisplayableId` property of `IUser` in previous versions of MSAL.NET.
`Environment` | A string containing the identity provider for this account, for example, login.microsoftonline.com. This property replaces the `IdentityProvider` property of `IUser`, except that `IdentityProvider` also had information about the tenant (in addition to the cloud environment), whereas here this is only the host.
`HomeAccountId` | AccountId of the home account for the user. This uniquely identifies the user across AAD tenants.

##### Token cache index keys

The MSAL.NET 2.x token cache is now indexed by two claims that are required to be present in the IDToken:

- `tid`: the tenant ID
- `preferred_username`: the preferred user name of the user.

This works well in most cases, but Azure AD B2C does not yet provide the `tid` (they are working on it). And the notion of `preferred_username` does not exist given that different social identity providers have different claims. For the moment, if you are targeting B2C and really want to use MSAL 2.x, you will need to setup additional properties so that these claims are produced in the IDToken.

#### Getting an Account is now asynchronous

The methods and properties returning IAccount are now all asynchronous, as in some cases getting the information might require querying the identity provider.

This means that in MSAL.NET 1.x you were writing:

```CSharp
IEnumerable<IUser> users = app.Users;
IUser firstUser = users.FirstOrDefault();
IUser alsoFirstUser = app.GetUser(firstUser.Identifier);
try
{
    result = await app.AcquireTokenSilentAsync(scopes, firstUser);
}
catch (MsalUiRequiredException)
{
    result = await app.AcquireTokenAsync(scopes, firstUser);
}
```

In MSAL.NET 2.x, you need to write:

```CSharp
IEnumerable<IAccount> accounts = await app.GetAccountsAsync();
IAccount firstAccount = accounts.FirstOrDefault();
try
{
    result = await app.AcquireTokenSilentAsync(scopes, firstAccount);
}
catch (MsalUiRequiredException)
{
    result = await app.AcquireTokenAsync(scopes, firstAccount);
}
accounts = await app.GetAccountsAsync();
firstAccount = accounts.FirstOrDefault();
IAccount me = await app.GetAccountAsync(firstAccount.HomeAccountId.Identifier);
```

#### Impact of this user -> account change on the public API

The types that had fields or properties of type IUser in MSAL.NET 1.x now reference `IAccount`.

![Impact of IUser to IAccount change on PublicClientApplication](https://user-images.githubusercontent.com/13203188/44722109-4efc2d80-aacc-11e8-8597-5aac8088ffb5.png)

- `IClientApplicationBase/ClientApplicationBase.Users` becomes `GetAccountsAsync()`
- `IClientApplicationBase/ClientApplicationBase.GetUser()` becomes `GetAccountAsync()`
- `Remove(IUser)` becomes `RemoveAsync(IAccount)`
- All the `AcquireTokenAsync` methods that were taking an `IUser` parameter in `IPublicClientApplication` / `PublicClientApplication` and `IConfidentialClientApplication` / `ConfidentialClientApplication` now take an `IAccount` (The picture above only shows public client application)

And also:

- `AuthenticationResult.User` becomes `AuthenticationResult.Account`
- `TokenCacheNotificationArgs.User` becomes `TokenCacheNotificationArgs.Account`

Finally as some methods are now asynchronous, if your application was interacting with UI based on the result of IUsers, you might need to update it to use the dispatcher

Therefore clearing the cache is also a bit different (For details see [Clearing the cache in MSAL.NET](https://aka.ms/msal-net-clearing-cache))

```CSharp
 await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                           () => {  uiUser.Text = something from IACount});
```

#### You can enable SSO between ADAL.NET 3.x applications, ADAL.NET 4.x applications, and MSAL on the same platform

If you have applications leveraging ADAL.NET and running in production, when upgrading, you probably don't want to force your users to re-sign-in. To preserve the single-sign-on (SSO) state, ADAL 4.x and MSAL 2.x share the same token cache and are capable of both reading and writing the ADAL 3.x token cache, in addition to the new cache format (named unified cache). This means that:
- You can convert an ADAL.NET 3.x or an ADAL.NET 4.x application to MSAL.NET 2.x and preserve the SSO state.
- You can have your users run side by side the ADAL.NET 3.x and ADAL.NET 4.x or MSAL.NET 2.x applications and share the SSO state. This is especially important for Web App and Web API in a farm of servers that need to run without interruption: in that case, you'll upgrade your apps progressively, and they need to keep sharing the security tokens.
Finally, on iOS, you can even have native iOS applications and Xamarin iOS ADAL and MSAL applications share the same token cache provided you set the key chain security group

If your application is a UWP, Xamarin.iOS, or Xamarin.Android application, it will automatically share the token cache between ADAL 3.x, ADAL 4.x, and MSAL 2.x. If it's a .NET Framework or .NET Core application, as usual, you'll need to customize the token cache serialization. For details see [Dual token cache serialization (MSAL unified cache + ADAL V3)](https://aka.ms/msal-net-token-cache-serialization-unified)

#### You can now enable SSO between ADAL and MSAL apps on Xamarin.iOS

In the Xamarin.iOS platform, PublicClientApplication has a new property named **KeychainSecurityGroup**. This Xamarin iOS specific property enables you to direct the application to share the token cache with other applications sharing the same keychain security group. If you provide this key, you must add the capability to your Application Entitlement. For more info, see [https://aka.ms/msal-net-sharing-cache-on-ios](https://aka.ms/msal-net-sharing-cache-on-ios).

**Note:** This API may change in a future release to align better with the  MSAL.iOS (native) library.

#### You can now leverage the embedded web browser in Xamarin.iOS and Xamarin.Android

In the previous versions of MSAL.NET, Xamarin.Android and Xamarin.iOS used the System web browser interacting with Chrome tabs. This was great if you wanted to benefit from SSO, but that was not working on some Android phones which device manufacturers did not provide Chrome, or if the end user had disabled Chrome. As an app developer, you can now leverage an embedded browser. To support this, the UIParent class now has a constructor taking a Boolean to specify if you want to choose the embedded browser. It also has a static method, IsSystemWebviewAvailable(), to help you decide if you want to use it.
For instance, on Android:
bool useSystemBrowser = UIParent.IsSystemWebviewAvailable();
App.UIParent = new UIParent(Xamarin.Forms.Forms.Context as Activity,
                           !useSystemBrowser);

For more details about this possibility see the article in MSAL's conceptual documentation: [https://aka.ms/msal-net-uses-web-browser](https://aka.ms/msal-net-uses-web-browser).
Also the web view implementation might change in the future

#### LogLevel (in the context of Logger) is no longer a nested type

The nested enumeration Logger.LogLevel was replaced by the lLogLevel enum, with the same enumeration values.

![Logger and associated types](https://user-images.githubusercontent.com/13203188/44722143-73f0a080-aacc-11e8-9ea7-3a5afd622dca.png)

For more information about logging in MSAL.NET, see [https://aka.ms/msal-net-logging](https://aka.ms/msal-net-logging).

#### Hiding types that should never have been public

MSAL.NET was defining types as public when they should not have been. This is now fixed in the latest version. These are:

- WebBrowserNavigateErrorEventHandler
- AuthenticationActivity and Resource in the Xamarin.Android platform

### Migrating from MSAL.NET 1.x to MSAL.NET 2.x

If you want to migrate from MSAL.NET 1.x to MSAL.NET 2.x, you'll get a number of compilation errors, but they are pretty straightforward to fix. In most cases you will only need to:

- Replace IUser by IAccount
- Replace the calls to application.Users to asynchronous calls to application.GetAccountsAsync

In advanced multi-account applications, where you were using the IUser.Identifier, you will now need to use the IAccount.HomeAccount.Identifier.

To help you migrate more easily from MSAL 1.x to MSAL 2.x, we have provided meaningful and actionable compiler errors that will tell you exactly what to do and will link to this article to help you migrate.

### Migrating from ADAL.NET 3.x or 4.x to MSAL.NET 2.x

We've started documenting what it will take you to migrate your ADAL.NET 3.x or ADAL.NET 4.x application to MSAL.NET 2.X. You can already find a page:

- [Differences between ADAL.NET and MSAL.NET applications](http://aka.ms/adal-net-to-msal-net), 
- The current state of the [features gap](https://aka.ms/msal-net-feature-gap) between ADAL.NET and MSAL.NET, especially in public client applications
- A sample gathering solutions to help you migrate [active-directory-dotnet-v1-to-v2 ](https://github.com/Azure-Samples/active-directory-dotnet-v1-to-v2)

## What's next?

### Known issues

- A number of issues in MSAL 2.0.0-preview where fixed in MSAL.NET 2.0.1-preview. See details in [MSAL.NET 2.0.1-preview release](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/releases/tag/MSAL-V2.0.1)
- PublicClientApplication's constructor throws a NullReferenceException when ran on an iPhone simulator in one of our samples. We have not reproduced it on other applications. We are investigating (For details see # [611](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/611))

### What's coming next in MSAL.NET?

This release is a first step. We'll release more features soon in MSAL.NET 2.x to bridge the gap with ADAL.NET, including:

- Support for Integrated Windows Authentication.
- Support for the Username / Password flow. Even if we don't recommend it, we recognize that this flow is still needed in a number of scenarios.
- Support for the Device Code Flow, enabling users to sign-in on devices without a browser.

To see the list of features we will be delivering, and their state, see [https://aka.ms/msal-net-feature-gap](https://aka.ms/msal-net-feature-gap).

For the roadmap see [MSAL.NET roadmap](https://aka.ms/msal-net-roadmap)