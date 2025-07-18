---
title: Using web browsers (MSAL.NET)
description: Learn about using browsers in Microsoft Authentication Library for .NET (MSAL.NET).
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 08/24/2023
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: how-to
ms.custom: devx-track-csharp, aaddev, has-adal-ref, devx-track-dotnet
#Customer intent: As an application developer, I want to learn about web browsers MSAL.NET so I can decide if this platform meets my application development needs and requirements.
---

# Using web browsers (MSAL.NET)

We recommend using brokers to authenticate as they offer more benefits compared to the browsers. On Windows machines the broker is [Web Account Manager (WAM)](https://aka.ms/msal-net-wam), on Android and iOS - [Microsoft Authenticator or Intune Company Portal](/entra/identity-platform/msal-net-use-brokers-with-xamarin-apps). Interactive authentication requires using a broker or a web browser. MSAL.NET supports a system web browser or an embedded web view.

## Web browsers in MSAL.NET

### Interaction happens in a web browser

It's important to understand that when acquiring a token interactively, the content of the dialog box isn't provided by the library but by Microsoft Entra ID. The authentication endpoint sends back HTML and JavaScript that controls the interaction, which is rendered in a web browser or a web control. Allowing the Microsoft Entra ID to handle the HTML interaction has many advantages:

- The password, if one was typed, is never stored by the application, nor the authentication library.
- It enables redirection to other identity providers (for instance, sign-in with a work or school account, or a personal account with MSAL; or with a social account with Azure AD B2C).
- It lets the Microsoft Entra ID control Conditional Access, for example, by having the user perform [multi-factor authentication (MFA)](/azure/active-directory/authentication/concept-mfa-howitworks) during the authentication phase (like entering a Windows Hello PIN; or being called on their phone or on an authentication app on their phone). In cases where the required multi-factor authentication isn't set it up yet, the user can set it up just-in-time in the same dialog. The user enters their mobile phone number and is guided to install an authentication application and scan a QR tag to add their account. This server-driven interaction is a great experience!
- It lets the user change their password in this same dialog when the password has expired (providing additional fields for the old password and the new password).
- It enables branding of the tenant or the application (images) controlled by the Microsoft Entra tenant admin or an application owner.
- It enables the users to consent to let the application access resources and scopes in their name just after the authentication.

### Embedded web view vs system browser

MSAL.NET is a multi-framework library and has framework-specific code to host a browser in a UI control (for example, on .NET either WinForms or WebView2; on .NET MAUI, native mobile controls, etc.). This control is called an *embedded* web view. Alternatively, MSAL.NET is also able to open a system web browser.

Generally, it's recommended that you use the platform default, and this is typically the system browser. The system browser is better at remembering the users that have logged in before. To change this behavior, use <xref:Microsoft.Identity.Client.AcquireTokenInteractiveParameterBuilder.WithUseEmbeddedWebView(System.Boolean)>

### Browser availability

| Framework                       | Embedded                 | System†                | Default               |
|:--------------------------------|:-------------------------|:-----------------------|:----------------------|
| .NET 6+††                       | ⛔ No                    | ✅ Yes                | System                |
| .NET 6+ Windows                 | ⛔ No†††                 | ✅ Yes                | System                |
| .NET MAUI                       | ✅ Yes                   | ✅ Yes                | System                |
| .NET 5+††                       | ⛔ No                    | ✅ Yes                | System                |
| .NET 4.6.2+                     | ✅ Yes                   | ✅ Yes                | Embedded              |
| .NET Standard                   | ⛔ No†††                 | ✅ Yes                | System                |
| .NET Core                       | ⛔ No†††                 | ✅ Yes                | System                |

**†** System browser requires `http://localhost` redirect URI.

**††** Target `net6.0-windows` or above to use the embedded browser.

**†††** Reference [Microsoft.Identity.Client.Desktop](https://www.nuget.org/packages/Microsoft.Identity.Client.Desktop) and call <xref:Microsoft.Identity.Client.Desktop.DesktopExtensions.WithWindowsDesktopFeatures%2A> to use the embedded browser.

## System web browser

Using the system browser has the significant advantage of sharing the Single Sign-On (SSO) state with web applications and other applications without needing a broker (WAM, Company Portal, Authenticator, etc.).

For desktop applications, however, launching a system browser leads to a subpar user experience, as the user sees the browser, where they might already have other tabs opened. And when authentication has happened, the users get a page asking them to close this window. If the user doesn't pay attention, they can close the entire process (including other tabs, which are unrelated to the authentication). Leveraging the system browser on desktop would also require opening local ports and listening on them, which might require advanced permissions for the application. You, as a developer, user, or administrator, might be reluctant about this requirement.

### How to use the default system browser

On .NET, MSAL will start the system browser as a separate process. MSAL.NET doesn't have control over this browser, but once the user finishes authentication, the web page is redirected in such a way that MSAL.NET can intercept the call to the redirect URI specified when creating a public client instance.

MSAL.NET cannot detect if the user navigates away or simply closes the browser. Apps using this technique are encouraged to define a timeout using a <xref:System.Threading.CancellationToken>. We recommend a timeout of at least a few minutes, to take into account cases where the user is prompted to change password or perform multi-factor authentication.

MSAL.NET needs to listen on `http://localhost:port` to intercept the code that Microsoft Entra ID responds with when the user finishes authenticating. See [Authorization code flow](/azure/active-directory/develop/v2-oauth2-auth-code-flow) for details.

To enable the system browser:

1. During app registration in the portal, configure `http://localhost` as a redirect URI (not currently supported by Azure B2C).
2. When you construct your public client app, specify this redirect URI.
3. Add `.WithUseEmbeddedWebView(false)`.

```csharp
var pca = PublicClientApplicationBuilder
            .Create("<CLIENT_ID>")
            // or use a known port if you wish "http://localhost:1234"
            .WithRedirectUri("http://localhost")
            .Build();

var result = await pca.AcquireTokenInteractive(s_scopes)
                    .WithUseEmbeddedWebView(false)
                    .ExecuteAsync();
```

When you configure `http://localhost`, MSAL.NET will find a random open port and use it. Using `http://localhost` as a redirect URI is safe. Another process cannot listen on a local socket which is already being listened on by MSAL. No network communication happens when the browser redirects to this URI. Even if somehow a malicious app intercepts the authentication code (no such known attacks, but it is possible if a malicious app has admin access to the machine), it cannot exchange it for a token because it needs a temporary secret that only your app knows, as described by the [PKCE](https://oauth.net/2/pkce/) protocol. The app is unable to listen on the HTTPS localhost endpoint (`https://localhost`) because port 443 is reserved and MSAL is unable to listen on it.

#### Limitations

Azure B2C and ADFS 2019 do not yet implement the *any port* option. So, you cannot set `http://localhost` (no port) redirect URI, but only `http://localhost:1234` (with port) URI. This means that you will have to do your own port management, for example, you can reserve a few ports and configure them as redirect URIs. Then your app can cycle through them until a port is free - this can then be used by MSAL.

For more details, see [Localhost exceptions](/azure/active-directory/develop/reply-url#localhost-exceptions).

### Linux and macOS

On Linux, MSAL.NET opens the default system browser with a tool like [xdg-open](http://manpages.ubuntu.com/manpages/focal/man1/xdg-open.1.html). Opening the browser with `sudo` is unsupported by MSAL and will cause MSAL to throw an exception.

On macOS, the browser is opened by invoking `open <url>`.

### Customizing the experience

MSAL.NET can respond with an HTTP message or an HTTP redirect when a token is received or an error occurs.

```csharp
var options = new SystemWebViewOptions()
{
    HtmlMessageError = "<p> An error occurred: {0}. Details {1}</p>",
    BrowserRedirectSuccess = new Uri("https://www.microsoft.com");
}

await pca.AcquireTokenInteractive(s_scopes)
         .WithUseEmbeddedWebView(false)
         .WithSystemWebViewOptions(options)
         .ExecuteAsync();
```

### Opening a specific browser

You may customize the way MSAL.NET opens the browser. For example, instead of using whatever browser is the default, you can force open a specific browser:

```csharp
var options = new SystemWebViewOptions()
{
    OpenBrowserAsync = SystemWebViewOptions.OpenWithEdgeBrowserAsync
}
```

## Web views in mobile applications

> [!NOTE]
> MSAL.NET versions 4.61.0 and above do not provide support for Xamarin Android and Xamarin iOS.

Embedded web views can be enabled in .NET MAUI applications. You may choose to use either embedded web views or system browsers. This is your choice depending on the user experience and security concerns you want to target.

### Differences between embedded web view and system browser

There are some visual differences between the embedded web view and the system browser in MSAL.NET.

**Interactive sign-in with MSAL.NET using the embedded web view:**

![Embedded web view appearance](../media/msal-net-web-browsers/embedded-webview.png)

**Interactive sign-in with MSAL.NET using the system browser:**

![System browser appearance](../media/msal-net-web-browsers/system-browser.png)

### Developer options

As a developer using MSAL.NET, you have several options for displaying the interactive sign-in dialog from Microsoft Entra ID:

- **System browser.** The system browser is set by default in the library. If using Android, see [system browsers](/azure/active-directory/develop/msal-net-system-browser-android-considerations) for specific information about which browsers are supported for authentication. When using the system browser in Android, we recommend for the device to have a browser that supports Chrome custom tabs; otherwise, authentication may fail.
- **Embedded web view.** To use only the embedded web view in MSAL.NET, the `AcquireTokenInteractive` builder contains a <xref:Microsoft.Identity.Client.AcquireTokenInteractiveParameterBuilder.WithUseEmbeddedWebView%2A> method.

In an iOS app:

```csharp
var result = app.AcquireTokenInteractive(scopes)
                .WithUseEmbeddedWebView(useEmbeddedWebview)
                .ExecuteAsync();
 ```

In an Android app:

```csharp
var result = app.AcquireTokenInteractive(scopes)
                .WithParentActivityOrWindow(activity)
                .WithUseEmbeddedWebView(useEmbeddedWebview)
                .ExecuteAsync();
```

#### Choosing between embedded web view or system browser on iOS

In your iOS app, in `AppDelegate.cs` you can initialize the `ParentWindow` to `null`. It's not used in iOS.

```csharp
App.ParentWindow = null; // no UI parent on iOS
```

#### Choosing between embedded web view or system browser on Android

In your Android app, in `MainActivity.cs` you can set the parent activity so that the authentication result gets back to it:

```csharp
 App.ParentWindow = this;
```

Then in the `MainPage.xaml.cs`:

```csharp
var result = await App.PCA.AcquireTokenInteractive(App.Scopes)
                      .WithParentActivityOrWindow(App.ParentWindow)
                      .WithUseEmbeddedWebView(true)
                      .ExecuteAsync();
```

#### Detecting the presence of custom tabs on Android

If you want to use the system web browser to enable Single-Sign On with the apps running in the browser, but are worried about the user experience for Android devices not having a browser with custom tab support, you have the option to decide by calling the <xref:Microsoft.Identity.Client.IPublicClientApplication.IsSystemWebViewAvailable%2A?displayProperty=nameWithType>. This method returns `true` if the Android package manager detects custom tabs and `false` if they aren't detected on the device.

Based on the value returned by this method, and your requirements, you can make a decision:

- You can return a custom error message to the user, for example - "Please install Chrome to continue with authentication", or
- You can fall back to launch the sign-in page in an embedded web view.

```csharp
bool useSystemBrowser = app.IsSystemWebViewAvailable();

authResult = await App.PCA.AcquireTokenInteractive(App.Scopes)
                      .WithParentActivityOrWindow(App.ParentWindow)
                      .WithUseEmbeddedWebView(!useSystemBrowser)
                      .ExecuteAsync();
```
