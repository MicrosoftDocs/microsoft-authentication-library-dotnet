## Use your own Browser 

A Web UI is a mechanism to invoke a browser. This can be a dedicated UI WebBrowser control or a way to delegate opening the browser.

#### Why?

MSAL provides Web UI implementations for most platforms, but there are still cases where may want to host the browser yourself: 

- platforms not explicitly covered by MSAL, e.g. Blazor, Unity, Mono on desktop
- you want to UI test your application and want to use an automated browser that can be used with Selenium 
- the browser and the app running MSAL are in separate processes

#### At a glance

To achieve this, MSAL  gives the developer a `start Url`, which needs to displayed in a browser of choice so that the end-user can enter his username etc. Once authentication completes, the developer needs to pass back to MSAL the `end Url`, which contains a code. The host of the `end Url` is always the `redirectUri`. To intercept the `end Url` you can: 

- monitor browser redirects until the `redirect Url` is hit OR
- have the browser redirect to an URL which you monitor

#### WithCustomWebUi is an extensibility point

`WithCustomWebUi` is an extensibility point that allows you provide your own UI in public client applications, and to let the user go through the /Authorize endpoint of the identity provider and let them sign-in and consent. MSAL.NET will then be able to redeem the authentication code and get a token. 

It's used in Visual Studio Electron applications (for instance VS Feedback) to provide the web interaction. The Electron app can display the browser, while 
MSAL runs as part of Visual Studio to maintain the user cache. You can also use it if you want to provide UI automation. 

Note that, in public client applications, MSAL.NET leverages the PKCE standard ([RFC 7636 - Proof Key for Code Exchange by OAuth Public Clients](https://tools.ietf.org/html/rfc7636)) to ensure that security is respected: Only MSAL.NET can redeem the code.

#### How to use WithCustomWebUi

To leverage this you need to:
  
  1. Implement the `ICustomWebUi`  interface (See [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/053a98d16596be7e9ca1ab916924e5736e341fe8/src/Microsoft.Identity.Client/Extensibility/ICustomWebUI.cs#L32-L70). You'll need to implement a single method `AcquireAuthorizationCodeAsync` accepting the authorization code URL (computed by MSAL.NET), letting the user go through the interaction with the identity provider, and then returning back the URL by which the identity provider would have called your implementation back (including the authorization code). This is a long running operations, so don't forget to check for cancellation and throw `OperationCancelledException` to allow apps consuming the custom web ui to transmit cancellation.
  2. In your `AcquireTokenInteractive` call you can use `.WithCustomUI()` modifier passing the instance of your custom web UI

```CSharp
using Microsoft.Identity.Client.Extensions;

     result = await app.AcquireTokenInteractive(scopes)
                       .WithCustomWebUi(yourCustomWebUI)
                       .ExecuteAsync();
```

#### Examples of implementation of ICustomWebUi in test automation - SeleniumWebUI

We have rewritten our UI tests to leverage this extensibility mechanism. In case you are interested you can have a look at the [SeleniumWebUI](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/053a98d16596be7e9ca1ab916924e5736e341fe8/tests/Microsoft.Identity.Test.Integration/Infrastructure/SeleniumWebUI.cs#L15-L160) class in the MSAL.NET source code

