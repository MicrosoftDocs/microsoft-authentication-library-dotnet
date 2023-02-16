# Brokers first

MSAL libraries are transitioning to use brokers (Authenticator app on mobile, WAM on Windows) instead of browsers. These provide enhanced security. It is strongly recommended that you use a broker-first approach. If the broker is not available, MSAL will fallback to a webview.

For desktop broker support, see https://aka.ms/msal-net-wam
For mobile, see chapter 2 of the sample https://github.com/Azure-Samples/active-directory-xamarin-native-v2

## At a glance  

The following tables focus on public client availability of web views and how "Is device managed" Conditional Access policy can be satisfied by these web views.
Note that username / password, integrated windows authentication and device code flow CANNOT satisfy the "Is device managed" CA policy.

### Availability per platform

| Platform     | Embedded WebView| System Browser| Broker (Authenticator / Company Portal / WAM) | MSAL Default |
| ------------ | ---------------- | -------------- | --------------------------------------------- | ------------ |
| Android      | ADAL + MSAL      | MSAL           | MSAL + ADAL (broken on ADAL due to OS updates)              | system       |
| iOS          | ADAL + MSAL      | MSAL           | MSAL + ADAL                                   | system       |
| UWP          | ADAL + MSAL      | N/A            | MSAL                                          | embedded     |
| .NET Classic | ADAL + MSAL      | MSAL           | MSAL                                          | embedded     |
| .NET Core    | ADAL + MSAL      | MSAL           | MSAL                                          | system       |
| .NET 5| MSAL             | MSAL           | MSAL                                          | embedded     |

### Can it satisfy the "Device is managed" CA?

| Device is Managed CA? | Embedded WebView | System Browser | Broker (Authenticator / Company Portal / WAM) |
| --------------------- | ---------------- | -------------- | --------------------------------------------- |
| Android               | no               | no             | yes                                           |
| iOS                   | no               | no             | yes                                           |
| MacOS                 | no               | yes (3)        | no (3)                                        |
| UWP                   | no               | no             | yes                                           |
| .NET Classic          | yes, but buggy (1)   | yes (2)            | yes                                           |
| .NET Core             | yes, but buggy (1)   | yes (2)           | yes                                           |
| .NET 5                 | yes, but buggy (1)  | yes  (2)          | yes                                           |

- (1) - doesn't work well on older, but supported, operating systems like Win Server 2016
- (2) - works with IE, Edge and Chrome + Windows 10 accounts extension (usually installed by Office)
- (3) - works with Safari and SSO extensions enabled on an Intune enrolled device

## System browser on desktop

For MSAL to use a system browser on desktop, it needs to: 

- find a local port which is unused, e.g. `1234`
- start listening to it (i.e. `http://localhost:1234`)
- start the browser process, by calling on the operating system to open the authorization url, e.g. `https://login.microsoftonline.com/common/oauth/v2.0/authorize?redirect_uri=http://localhost:1324`
- allow the user to perform interactive steps for auth (credentials, Win Hello, MFA etc.)
- AAD redirects the browser to `http://localhost:1234?auth_code=secret-auth-code` which MSAL can then exchange for a token

### Is http://localhost safe?

- There are no known attacks for http://localhost redirect uri
- A process cannot listen to a local socket which is already listened to
- Even if somehow a malicious app intercepts the auth code (no such known attacks, but possible if malicious app has admin access to the box), it cannot exchange it for a token because it needs a temporary secret which only your app knows, as described by [PKCE](https://oauth.net/2/pkce/) protocol 

### Why don't you use https://localhost ?

This confusion arises by misunderstanding public client (deskop / mobile apps) with confidential client (web site / web api). 

- Desktop apps cannot be deployed with certificates. 
- No network communication happens when the browser redirects to http://localhost
- PKCE protects against Man-in-the-Middle attacks


## Summary 
On Xamarin.Android and Xamarin.iOS, MSAL is able to use app specific urls to intercept a code from AAD.

- [Web browsers](#web-browsers-in-msalnet) are required for interactive authentication
- By default, MSAL.NET supports the [system web browser](#by-default-msalnet-supports-a-system-web-browser-on-xamarinios-and-xamarinandroid) on Xamarin.iOS [Xamarin.Android](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/system-browser) and also on .NET Core, .NET Standard and .NET Classic [see details](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/System-Browser-on-.Net-Core)
- But [you can also enable the Embedded Web browser](#you-can-also-enable-embedded-webviews-in-xamarinios-and-xamarinandroid-apps) depending on your requirements (UX, need for SSO, security)  in [Xamarin.iOS](#choosing-between-embedded-web-browser-or-system-browser-on-xamarinios) and [Xamarin.Android](#choosing-between-embedded-web-browser-or-system-browser-on-xamarinandroid) apps.
- And you can even [choose dynamically](#detecting-the-presence-of-chrome--chrome-tabs-on-xamarinandroid) which web browser to use based on the presence of Chrome or a browser supporting Chrome custom tabs in Android.

## Web browsers in MSAL.NET

One important understanding with authentication libraries and Azure AD is that, when acquiring a token interactively, the content of the dialog box is not provided by the library, but by the STS (Security Token Service). The authentication endpoint sends back some HTML and JavaScript that control the interaction, and it's rendered in a web browser or web control. Allowing the STS to handle the HTML interaction has many advantages:

- The password (if one was typed) is never stored by the application, nor the authentication library.
- Enabling redirections to other identity providers (for instance login-in with a work school account or a personal account with MSAL, or with a social account with Azure AD B2C).
- Letting the STS control conditional access, for instance by having the user do multiple factor authentication (MFA) during this authentication phase (entering a Windows Hello pin, or being called on their phone, or on an authentication app on their phone). In cases where multi factor authentication is required and the user has not set it up yet, they can even set it up just in time in the same dialog: they enter their mobile phone number, and are guided to install an authentication application and scan a QR tag to add their account. This server driven interaction is a great experience!
- Letting the user change their password in this same dialog when the password has expired (providing additional fields for the old password and the new password).
- Enabling branding of the tenant, or the application (images) controlled by the Azure AD tenant admin / application owner.
- Enabling the users to consent to let the application access resources / scopes in their name just after the authentication.

## .NET Core experience
Please see [System Browser on .NET Core](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/System-Browser-on-.Net-Core) for details

##  Xamarin.iOS and Xamarin.Android experience

MSAL.NET leverages by default the **system web browser** for Xamarin iOS and Xamarin Android applications. On iOS, it even choses the web view to use depending on the version of the Operating System (iOS12, iOS11, and earlier)

Using the system browser has the significant advantage of sharing the SSO state with other applications and with web applications without needing a broker (Company portal / Authenticator). The system browser was used, by default, in the MSAL.NET for the Xamarin iOS and Xamarin Android platforms because, on these platforms, the system web browser occupies the whole screen, and the user experience is better. The system web view is not distinguishable from a dialog. On iOS, though, the user might have to give consent for the browser to call back the application, which can be annoying.

#### UWP does not use the System Webview

For desktop applications, however, launching a System Webview leads to a sub-par user experience, as the user sees the browser, where they might already have other tabs opened. And when authentication has happened, the users gets a page asking them to close this window. If the user does not pay attention, they can close the entire process (including other tabs, which are unrelated to the authentication). Leveraging the system browser on desktop would also require opening local ports and listening on them, which might require advanced permissions for the application. You, as a developer, user, or administrator, might be reluctant about this requirement.

## You can also enable Embedded Webviews in Xamarin.iOS and Xamarin.Android apps

MSAL.NET also supports using the **embedded** webview option. Note that for ADAL.NET, embedded webview is the only option supported.
As a developer using MSAL.NET targeting Xamarin, you may choose to use either embedded webviews or system browsers. This is your choice depending on the user experience and security concerns you want to target, but it's not recommended for B2C as some integrated identity providers don't allow it.

### Visual Differences Between Embedded Webview and System Browser in MSAL.NET

**Interactive sign-in with MSAL.NET using the Embedded Webview:**

![embedded](https://user-images.githubusercontent.com/19942418/40319714-f5df7a36-5cdd-11e8-9efc-9f1b6661f4be.PNG)

**Interactive sign-in with MSAL.NET using the System Browser:**

![systemBrowser](https://user-images.githubusercontent.com/19942418/40319616-a563346c-5cdd-11e8-82d3-2328bef9c172.PNG)

### Developer Options

> Note: The way you chose between the system browser and the embedded webview will change some time in the near future (before official release).

As a developer using MSAL.NET, you have several options for displaying the interactive dialog from STS:

- **System browser.** The system browser is set by default in the library. If using Android, please see [system browsers](Android-system-browser) with specific information about which browsers are supported for authentication. Note that when using system browser in Android, we recommend the device have a browser which supports Chrome custom tabs, otherwise, authentication may fail. For more information about these issues, read the section on [Android system browsers](Android-system-browser).
- **Embedded webview.** Enables you to specify if you want to force the usage of an embedded web view. For more details see [Usage of Web browsers](MSAL.NET-uses-web-browser)

```CSharp
 result = await app.AcquireTokenInteractive(scopes)
                   .WithUseEmbeddedWebView(true)
                   .ExecuteAsync();
```

`AcquireTokenInteractive` has one specific optional parameter enabling it to specify, for platforms supporting it, the parent UI (window in Windows, Activity in Android). This parent UI is specified using `.WithParentActivityOrWindow()`. The UI dialog will typically be centered on that parent. On Android the parent activity is a mandatory parameter.

```CSharp
// Android
WithParentActivityOrWindow(Activity activity)

// iOS
WithParentActivityOrWindow(IUIViewController viewController)
```

#### Detecting the presence of custom tabs on Xamarin.Android

If you want to use the system web browser to enable SSO with the apps running in the browser, but are worried about the user experience for Android devices not having a browser with custom tab support, you have the option to decide by calling `IsSystemWebViewAvailable`. This method returns `true` if the PackageManager detects custom tabs and `false` if they are not detected on the device.

Based on the value returned by this method, and your requirements, as the developer, you can make a decision:

- You can return a custom error message to the user. For example: "Please install Chrome to continue with authentication" -OR-
- You can fallback to the embedded webview option and launch the UI as an embedded webview

The code below shows how you would do the later:

```CSharp
bool useEmbeddedWebView = !app.IsSystemWebViewAvailable;

var authResult = AcquireTokenInteractive(scopes)
 .WithParentActivityOrWindow(parentActivity)
 .WithEmbeddedWebView(useEmbeddedWebView)
 .ExecuteAsync();
```

## .NET Core does not support embedded browser

For .NET Core, by default an embedded browser is not available because .NET Core does not provide UI yet.