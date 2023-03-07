---
title: Using MSAL.NET with .NET Core
---

# Using MSAL.NET with .NET (aka .NET Core)

>[!NOTE]
>This feature is available from version 4.0.0 on .NET Core and from version 4.1.0 on .NET Classic. 

## Embedded vs System Web UI

MSAL is a multi-framework library and has framework specific code to host a browser in a UI control (e.g. on .NET Classic it uses WinForms, on Xamarin it uses native mobile controls etc.). This is called `embedded` web UI. 
Alternatively, MSAL is also able to kick off the system OS browser. 

We recommend that you use the platform default, and this is typically the system browser. The system browser is better at remembering the users that have logged in before. If you need to change this behavior, use `WithUseEmbeddedWebView(bool)`

## Limitations

B2C and ADFS 2019 do not yet implement the "any port" option. So you cannot set "http://localhost" (no port) redirect URI, but only "http://localhost:1234" (with port) URI. This means that you will have to do your own port management, for example you can reserve a few ports and configure them as redirect URIs. Then your app can cycle through them until a port is free - this can then be used by MSAL. 

UWP doesn't support listening to a port and thus doesn't support System Browsers, [Read more](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/MSAL.NET-uses-web-browser#uwp-does-not-use-the-system-webview).

## System Browser Experience

On .NET Core, MSAL will start the system browser as a separate process. MSAL does not have control over this browser, but once the user finishes authentication, the web page is redirected in such a way that MSAL can intercept the Uri. 

You can also configure apps written for .NET Classic to use this browser, by specifying

```csharp
await pca.AcquireTokenInteractive(s_scopes)
         .WithUseEmbeddedWebView(false)
```

MSAL cannot detect if the user navigates away or simply closes the browser. Apps using this technique are encouraged to define a timeout (via `CancellationToken`). We recommend a timeout of at least a few minutes, to take into account cases where the user is prompted to change password or perform 2FA.

## How to use the System Browser (i.e. the default browser of the OS)

MSAL needs to listen on on `http://localhost:port` and intercept the code that Azure AD  sends when the user is done authenticating. 

To achieve this: 

1. During app registration, configure `http://localhost` as a redirect URI (not currently supported by B2C)
2. When you construct your PubliClientApplication, specify this redirect URI:

```csharp
IPublicClientApplication pca = PublicClientApplicationBuilder
                            .Create("<CLIENT_ID>")
                             // or use a known port if you wish "http://localhost:1234"
                            .WithRedirectUri("http://localhost")  
                            .Build();
```

Note: If you configure `http://localhost`, internally MSAL will find a random open port and use it.

## Integration with Windows Broker

WAM - Windows Authentication Manager is a Windows component that can provide additional context to the authentication session. It is mandatory to use WAM for certain Conditional Access scenarios.

MSAL does not not yet integrate directly with WAM, however some browsers (Chrome and Edge) do have WAM integration. So using a system browser can satisfy the requirements of Conditional Access.

## Linux and Mac ## 

On Linux, MSAL will open the default OS browser using the xdg-open, gnome-open, or kfmclient utilities. To troubleshoot, run the tool from a terminal e.g. `xdg-open "https://www.bing.com"`  .
On Mac, the browser is opened by invoking `open <url>`.

## Customizing the experience ##

_Note: customization is available from MSAL 4.1.0 onward_

MSAL is able to respond with an HTTP message when a token is received or in case of error. You can display an HTML message or redirect to an url of your choice: 

```csharp
var options = new SystemWebViewOptions() 
{
    HtmlMessageError = "<p> An error occured: {0}. Details {1}</p>",
    BrowserRedirectSuccess = new Uri("https://www.microsoft.com"); 
}
 
await pca.AcquireTokenInteractive(s_scopes)
         .WithUseEmbeddedWebView(false)
         .WithSystemWebViewOptions(options)
         .ExecuteAsync();
                           
```

#### Opening a specific browser (Experimental) ##

You may customize the way MSAL opens the browser. For example instead of using whatever browser is the default, you can force open a specific browser:

```csharp
var options = new SystemWebViewOptions() 
{
    OpenBrowserAsync = SystemWebViewOptions.OpenWithEdgeBrowserAsync
}
```

## Availability of the WebViews

For more details about WebViews see [Usage of Web Browsers](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/MSAL.NET-uses-web-browser)
