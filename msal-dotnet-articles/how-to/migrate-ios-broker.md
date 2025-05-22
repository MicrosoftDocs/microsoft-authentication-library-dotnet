---
title: Migrate Xamarin apps using brokers to MSAL.NET
description: Learn how to migrate Xamarin iOS apps that use Microsoft Authenticator from ADAL.NET to MSAL.NET.
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 09/08/2019
ms.service: msal
ms.subservice: msal-dotnet
ms.workload: identity
ms.reviewer:
ms.topic: conceptual
ms.custom: devx-track-csharp, aaddev, has-adal-ref
#Customer intent: As an application developer, I want to learn how to migrate my iOS applications that use Microsoft Authenticator from ADAL.NET to MSAL.NET.

---

# Migrate iOS applications that use Microsoft Authenticator from ADAL.NET to MSAL.NET

[!INCLUDE [ADAL migration note](../includes/adal-migration-note.md)]

If you've been using the Azure Active Directory Authentication Library for .NET (ADAL.NET) and the iOS broker, you need to migrate to the Microsoft Authentication Library (MSAL) for .NET, which supports the broker on iOS starting with version 4.3. This article helps you migrate your .NET iOS application from ADAL to MSAL.

## Prerequisites

This article assumes that you have a MAUI or Xamarin iOS app that's integrated with the iOS broker. If you do not have an existing ADAL-based application you can use MSAL.NET directly use the built-in broker implementation in the library. For information on how to invoke the iOS broker in MSAL.NET with a new application, see [Using MSAL.NET With MAUI](../acquiring-tokens/desktop-mobile/mobile-applications.md).

## Background

### What are authentication brokers?

Authentication brokers are applications provided by Microsoft on Android and iOS, such as [Microsoft Authenticator](https://support.microsoft.com/en-us/account-billing/download-microsoft-authenticator-351498fc-850a-45da-b7b6-27e523b8702a) on iOS and Android and the Intune Company Portal app on Android.

Authentication brokers enable the following scenarios:

- Single sign-on (SSO).
- Device identification, which is required by some [Conditional Access policies](/entra/identity/conditional-access/overview). For more information, see [Device management](/entra/identity/conditional-access/concept-conditional-access-conditions#device-platforms).
- Application identification verification, which is also required in some enterprise scenarios. For more information, see [Intune mobile application management (MAM)](/mem/intune/apps/app-management).

## Migrate from ADAL to MSAL

### Step 1: Enable the broker

<table>
<tr><td>Current ADAL code:</td><td>MSAL counterpart:</td></tr>
<tr><td>
In ADAL.NET, broker support was enabled on a per-authentication context basis. It's disabled by default. You had to set a `useBroker` flag to `true` in the `PlatformParameters` constructor to call the broker:

```csharp
public PlatformParameters(
        UIViewController callerViewController,
        bool useBroker)
```

In the platform-specific code, within the page renderer for iOS, set the `useBroker` flag to true:

```csharp
page.BrokerParameters = new PlatformParameters(
          this,
          true,
          PromptBehavior.SelectAccount);
```

Then, include the parameters in the token acquisition call:

```csharp
 AuthenticationResult result =
                    await
                        AuthContext.AcquireTokenAsync(
                              Resource,
                              ClientId,
                              new Uri(RedirectURI),
                              platformParameters)
                              .ConfigureAwait(false);
```

</td><td>
In MSAL.NET, broker support is enabled separately for each [`PublicClientApplication`](xref:Microsoft.Identity.Client.PublicClientApplication) instance. It's disabled by default. To enable it, use [`WithBroker()`](xref:Microsoft.Identity.Client.PublicClientApplicationBuilder.WithBroker(System.Boolean)) (set to true by default) in order to call the broker:

```csharp
var app = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithBroker()
                .WithReplyUri(redirectUriOnIos)
                .Build();
```

In the token acquisition call:

```csharp
result = await app.AcquireTokenInteractive(scopes)
             .WithParentActivityOrWindow(App.RootViewController)
             .ExecuteAsync();
```

</table>

### Step 2: Set a UIViewController()

In ADAL.NET, you passed in a `UIViewController` as part of `PlatformParameters`. In MSAL.NET, to give developers more flexibility, an object window is used, but it's not required in regular iOS scenarios. To use the broker, set the object window in order to send and receive responses from the broker.

<table>
<tr><td>Current ADAL code:</td><td>MSAL counterpart:</td></tr>
<tr><td>
A `UIViewController` is passed into `PlatformParameters`.

```csharp
page.BrokerParameters = new PlatformParameters(
          this,
          true,
          PromptBehavior.SelectAccount);
```

</td><td>
In MSAL.NET, you must do two things to set the object window for iOS:

1. In `AppDelegate.cs`, set `App.RootViewController` to a new `UIViewController()`. This assignment ensures that there's a `UIViewController` with the call to the broker. If it isn't set correctly, you might get this error:

    `"uiviewcontroller_required_for_ios_broker":"UIViewController is null, so MSAL.NET cannot invoke the iOS broker. See https://aka.ms/msal-net-ios-broker"`

1. On the `AcquireTokenInteractive` call, use [`.WithParentActivityOrWindow(App.RootViewController)`](xref:Microsoft.Identity.Client.AcquireTokenInteractiveParameterBuilder.WithParentActivityOrWindow(System.Object)) and pass in the reference to the object window you'll use.

**For example:**

In `App.cs`:

```csharp
   public static object RootViewController { get; set; }
```

In `AppDelegate.cs`:

```csharp
   LoadApplication(new App());
   App.RootViewController = new UIViewController();
```

In the token acquisition call:

```csharp
result = await app.AcquireTokenInteractive(scopes)
             .WithParentActivityOrWindow(App.RootViewController)
             .ExecuteAsync();
```

</table>

### Step 3: Update AppDelegate to handle the callback

Both ADAL and MSAL call the broker, and the broker in turn calls back to your application through the `OpenUrl` method of the `AppDelegate` class. For more information, see [Update AppDelegate to handle the callback](/entra/identity-platform/msal-net-use-brokers-with-xamarin-apps#step-3-update-appdelegate-to-handle-the-callback).

There are no changes here between ADAL.NET and MSAL.NET.

### Step 4: Register a URL scheme

ADAL.NET and MSAL.NET use URLs to invoke the broker and return the broker response back to the app. Register the URL scheme in the `Info.plist` file for your app:

<table>
<tr><td>Current ADAL code:</td><td>MSAL counterpart:</td></tr>
<tr><td>
The URL scheme is unique to your app.
</td><td>
The `CFBundleURLSchemes` name must include `msauth.` as a prefix, followed by your `CFBundleURLName`.

For example: `$"msauth.(BundleId")`

```csharp
<key>CFBundleURLTypes</key>
<array>
  <dict>
    <key>CFBundleTypeRole</key>
    <string>Editor</string>
    <key>CFBundleURLName</key>
    <string>com.yourcompany.xforms</string>
    <key>CFBundleURLSchemes</key>
    <array>
      <string>msauth.com.yourcompany.xforms</string>
    </array>
  </dict>
</array>
```

> [!NOTE]
> This URL scheme becomes part of the redirect URI that's used to uniquely identify the app when it receives the response from the broker.

</table>

### Step 5: Add the broker identifier to the LSApplicationQueriesSchemes section

ADAL.NET and MSAL.NET both use `-canOpenURL:` to check if the broker is installed on the device. Add the correct identifier for the iOS broker to the `LSApplicationQueriesSchemes` section of the `info.plist` file:

<table>
<tr><td>Current ADAL code:</td><td>MSAL counterpart:</td></tr>
<tr><td>
Uses `msauth`

```csharp
<key>LSApplicationQueriesSchemes</key>
<array>
     <string>msauth</string>
</array>
```

</td><td>
Uses `msauthv2`

```csharp
<key>LSApplicationQueriesSchemes</key>
<array>
     <string>msauthv2</string>
     <string>msauthv3</string>
</array>
```

</table>

### Step 6: Register your redirect URI in the Azure portal

ADAL.NET and MSAL.NET both add an extra requirement on the redirect URI when it targets the broker. Register the redirect URI with your application in the Azure or Microsoft Entra portals.

<table>
<tr><td>Current ADAL code:</td><td>MSAL counterpart:</td></tr>
<tr><td>

`"<app-scheme>://<your.bundle.id>"`

Example:

```http
mytestiosapp://com.mycompany.myapp`
```

</td><td>

`$"msauth.{BundleId}://auth"`

Example:

```csharp
public static string redirectUriOnIos = "msauth.com.yourcompany.XForms://auth";
```

</table>

For more information about how to register the redirect URI in the Azure portal, see [Add a redirect URI to your app registration](/entra/identity-platform/msal-net-use-brokers-with-xamarin-apps#step-7-add-a-redirect-uri-to-your-app-registrationn).

### Step 7: Set the Entitlements.plist

Enable keychain access in the `Entitlements.plist` file:

```xml
<key>keychain-access-groups</key>
<array>
  <string>$(AppIdentifierPrefix)com.microsoft.adalcache</string>
</array>
```

For more information about enabling keychain access, see [Enable keychain access](/entra/identity-platform/msal-net-xamarin-ios-considerations#enable-keychain-access).

[!INCLUDE [importance-of-logging](../includes/importance-of-logging.md)]

## Next steps

Learn about [iOS-specific considerations with MSAL.NET](/entra/identity-platform/msal-net-xamarin-ios-considerations).
