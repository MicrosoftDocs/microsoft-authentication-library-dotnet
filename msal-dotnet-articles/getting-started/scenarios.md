---
title: MSAL.NET Scenarios
---

# MSAL.NET Scenarios

## Introduction

The .NET authentication libraries support scenarios involving Protecting a Web API ![image](https://user-images.githubusercontent.com/13203188/44856754-0c993480-ac23-11e8-82ef-e0eaa586b9c8.png) and **Acquiring tokens** for a protected Web API ![image](https://user-images.githubusercontent.com/13203188/44856748-060abd00-ac23-11e8-8b69-cbe928bec23c.png). MSAL.NET is only about the later. 

The token can be acquired from a number of **application types**: Web applications, Mobile or Desktop applications, Web APIs, and application running on devices that don't have a browser (or iOT). Applications tend to be separated into two categories:

- Public client applications (Desktop / Mobile) use the <xref:microsoft.identity.client.publicclientapplication> class
- Confidential client applications (Web apps, Web APIs, and daemon applications - desktop or Web). These type of apps use the <xref:microsoft.identity.client.confidentialclientapplication>.

MSAL.NET supports acquiring tokens either in the name of a **user** ![image](https://user-images.githubusercontent.com/13203188/44856646-c93ec600-ac22-11e8-85d0-12eaa505b123.png), or, (and only for confidential client applications), in the name of the application itself (for no user). In that case the confidential client application shares a secret with Azure AD ![image](https://user-images.githubusercontent.com/13203188/44856653-cd6ae380-ac22-11e8-95d7-52527361ff89.png)

MSAL.NET supports a number of **platforms** (.NET Framework, .NET Core, Windows 10/UWP, Xamarin.iOS, Xamarin.Android). .NET Core apps can also run on different operating systems (Windows, but also Linux and MacOs). The scenarios can be different depending on the platforms

## The Scenarios

The picture below summarizes the supported scenarios and shows on which platform, and to which Azure AD protocol this corresponds:

![image](https://user-images.githubusercontent.com/13203188/44857925-ad88ef00-ac25-11e8-8ef1-b9fca3671323.png)

### Web Application signing in a user and calling a Web API in the name of the user

To protected a Web App (signing in the user) you'll use ASP.NET or ASP.NET Core with the ASP.NET Open ID Connect middleware. Under the hood. This involves validating the token which is done by the [IdentityModel extensions for .NET](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki) library, not MSAL.NET.

To call the Web API in the name of the user you'll use MSAL.NET ConfidentialClientApplication, leveraging the [Authorization code flow](Acquiring-tokens-with-authorization-codes-on-web-apps), then storing the acquired token in the token cache, and [acquiring silently a token](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/AcquireTokenSilentAsync-using-a-cached-token#recommended-call-pattern-in-web-apps-using-the-authorization-code-flow-to-authenticate-the-user) from the cache when needed. MSAL refreshes the token if needed.

![image](https://user-images.githubusercontent.com/13203188/44857464-b6c58c00-ac24-11e8-9509-103ada932b09.png)

### Mobile application calling a Web API in the name of the user who's signed-in interactively

To call a Web API from a mobile application, you will use MSAL.NET's PublicClientApplication's [interactive](../acquiring-tokens/desktop-mobile/acquiring-tokens-interactively.md) token acquisition methods. These interactive methods enable you to control the sign-in UI experience, as well as the location of the interactive dialog on some platforms.

To enable this interaction, MSAL.NET leverages a [web browser](/azure/active-directory/develop/msal-net-web-browsers). There are specificities depending on the mobile platform [UWP](../acquiring-tokens/desktop-mobile/uwp.md), [iOS](/azure/active-directory/develop/msal-net-xamarin-ios-considerations), [Android](/azure/active-directory/develop/msal-net-xamarin-android-considerations)). On iOS and Android, you can even choose if you want to leverage the system browser (the default), or an embedded web browser. You can enable some kind of token cache sharing on iOS

![image](https://user-images.githubusercontent.com/13203188/44857487-c2b14e00-ac24-11e8-95bc-55d559c7c17b.png)

#### Protecting the app itself with Intune

Your mobile app (written in Xamarin.iOS or Xamarin.Android) can have app protection policies applied to it, so that it can be [managed by InTune](/intune/app-sdk) and recognized by InTune as a managed app. The [InTune SDK](/intune/app-sdk-get-started) is separate from MSAL, and it talks to AAD on its own.

### Desktop/service daemon application calling Web API in without a user (in its own name)

You can write a daemon app acquiring a token for the app on top using MSAL.NET's ConfidentialClientApplication's [client credentials](../acquiring-tokens/web-apps-apis/client-credential-flows.md) acquisition methods. These suppose that the app has previously registered a secret (application password or certificate) with Azure AD, which it then shares with this call.

![image](https://user-images.githubusercontent.com/13203188/44857500-ccd34c80-ac24-11e8-8438-be5e329c6126.png)

### Desktop application calling a Web API in the name of the signed-in user

Desktop applications can use the same [interactive authentication](https://aka.ms/msal-net-acquire-token-interactively) as the [mobile applications](#mobile-application-calling-a-web-api-in-the-name-of-the-user-whos-signed-in-interactively).

![image](https://user-images.githubusercontent.com/13203188/44857519-d52b8780-ac24-11e8-943c-684b3e9114ce.png)

For Windows hosted applications, it's also possible for applications running on computers joined to a Windows domain or AAD joined to acquire a token silently by using [Integrated Windows Authentication](https://aka.ms/msal-net-iwa)

If your desktop application is a .NET Core application running on Linux or Mac, you will be able to use neither the interactive authentication (as .NET Core does not provide a [Web browser](/azure/active-directory/develop/msal-net-web-browsers)), nor Integrated Windows Authentication. The best option in that case is to use device code flow (See [Application without a browser, or iOT application calling an API in the name of the user](#application-without-a-browser-or-iot-application-calling-an-api-in-the-name-of-the-user)) below

Finally, and although it's not recommended, you can use [Username/Password](../acquiring-tokens/desktop-mobile/username-password-authentication.md) in public client applications; It's still needed in some scenarios (like DevOps), but beware that using it will impose constraints on your application. For instance it won't be able to sign-in user who need to perform Multi Factor Authentication (conditional access) and it won't enable your application to benefit from Sigle Sign On. It's also against the principles of modern authentication and is only provided for legacy reasons.

In desktop applications, if you want the token cache to be persistent, you should [customize the token cache serialization](/azure/active-directory/develop/msal-net-token-cache-serialization). You can even enable backward and forward compatible token caches with ADAL.NET 3.x and 4.x by implementing dual token cache serialization.

### Application without a browser, or iOT application calling an API in the name of the user

Applications running on a device without a browser will still be able to call an API in the name of a user, after having the user sign-in on another device which has a Web browser. For this you'll need to use the [Device Code flow](../acquiring-tokens/desktop-mobile/device-code-flow.md)

![image](https://user-images.githubusercontent.com/13203188/44857536-dbb9ff00-ac24-11e8-9d03-37b06bd36a5b.png)

### Web API calling another downstream Web API in the name of the user for whom it was called

If you want your ASP.NET or ASP.NET Core protected Web API to call another Web API on behalf of the user represented by the access token was used to call you API, you will need to:

- Validate the token. For this you'll use the ASP.NET JWT middleware. Under the hood. This also involves validating the token which is done by the [IdentityModel extensions for .NET](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki) library, not MSAL.NET
- Then you will need to acquire a token for the downstream Web API by using the ConfidentialClientApplication's method Acquiring a token on [behalf of a user](on-behalf-of) in Service to Services calls.
- Web APIs calling other web API will also need to provide a [custom cache serialization](token-cache-serialization#token-cache-for-a-web-app-confidential-client-application)

![image](https://user-images.githubusercontent.com/13203188/44857544-dfe61c80-ac24-11e8-8682-f697d6fe07c6.png)

### Web API calling another API in its own name

like in the case of a desktop/service daemon application, a daemon Web API (or a daemon Web App) can use MSAL.NET's ConfidentialClientApplication's [client credentials](Client-credential-flows) acquisition methods

## Transverse features

In all the scenarios you might want to:

- Troubleshoot yourself by activating [logs](../advanced/logging.md) or Telemetry
- Understand how to react to [exceptions](../advanced/exceptions/index.md) due to the Azure AD service [`MsalServiceException`](/dotnet/api/microsoft.identity.client.msalserviceexception?view=azure-dotnet-preview#fields), or to something wrong happening in the client itself [`MsalClientException`](/dotnet/api/microsoft.identity.client.msalclientexception?view=azure-dotnet-preview#fields)
- Use MSAL.NET with a [proxy](../advanced/httpclient.md)
