---
title: Instantiate a confidential client app (MSAL.NET)
description: Learn how to instantiate a confidential client application with configuration options using the Microsoft Authentication Library for .NET (MSAL.NET).
services: active-directory
author: Dickson-Mwendia
manager: CelesteDG
ms.author: dmwendia
ms.date: 08/24/2023
ms.service: msal
ms.subservice: msal-dotnet
ms.workload: identity
ms.reviewer:
ms.topic: how-to
ms.custom: devx-track-csharp, aaddev, devx-track-dotnet
#Customer intent: As an application developer, I want to learn how to use application config options so I can instantiate a confidential client app.

---

# Instantiate a confidential client application with configuration options using MSAL.NET

This article describes how to instantiate a [confidential client application](/azure/active-directory/develop/msal-client-applications) using the Microsoft Authentication Library for .NET (MSAL.NET).  The application is instantiated with configuration options defined in a settings file.

Before initializing an application, you first need to [register](/azure/active-directory/develop/quickstart-register-app) it so that your app can be integrated with the Microsoft identity platform. After registration, you may need the following information (which can be found in the Azure portal):

- The client ID (a string representing a GUID)
- The identity provider URL (named the instance) and the sign-in audience for your application. These two parameters are collectively known as the authority.
- The tenant ID if you are writing a line-of-business application solely for your organization (also named single-tenant application).
- The application secret (client secret string) or certificate (of type X509Certificate2) if it's a confidential client app.
- For web apps, and sometimes for public client apps (in particular when your app needs to use a broker), you'll have also set the redirectUri where the identity provider will contact back your application with the security tokens.

## Configure the application from the config file
The name of the properties of the options in MSAL.NET match the name of the properties of the `AzureADOptions` in ASP.NET Core, so you don't need to write any glue code.

An ASP.NET Core application configuration is described in an *appsettings.json* file:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "[Enter the domain of your tenant, e.g. contoso.onmicrosoft.com]",
    "TenantId": "[Enter 'common', or 'organizations' or the Tenant Id (Obtained from the Azure portal. Select 'Endpoints' from the 'App registrations' blade and use the GUID in any of the URLs), e.g. aaaabbbb-0000-cccc-1111-dddd2222eeee]",
    "ClientId": "[Enter the Client Id (Application ID obtained from the Azure portal), e.g. 00001111-aaaa-2222-bbbb-3333cccc4444]",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath ": "/signout-callback-oidc",

    "ClientSecret": "[Copy the client secret added to the app from the Azure portal]"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Starting in MSAL.NET v3.x, you can configure your confidential client application from the config file.

In the class where you want to configure and instantiate your application, declare a `ConfidentialClientApplicationOptions` object.  Bind the configuration read from the source (including the appconfig.json file) to the instance of the application options, using the `IConfigurationRoot.Bind()` method from the [Microsoft.Extensions.Configuration.Binder NuGet package](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Binder):

```csharp
using Microsoft.Identity.Client;

private ConfidentialClientApplicationOptions _applicationOptions;
_applicationOptions = new ConfidentialClientApplicationOptions();
configuration.Bind("AzureAD", _applicationOptions);
```

This enables the content of the "AzureAD" section of the *appsettings.json* file to be bound to the corresponding properties of the `ConfidentialClientApplicationOptions` object.  Next, build a `ConfidentialClientApplication` object:

```csharp
IConfidentialClientApplication app;
app = ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(_applicationOptions)
        .Build();
```

## Add runtime configuration
In a confidential client application, you usually have a cache per user. Therefore you will need to get the cache associated with the user and inform the application builder that you want to use it. In the same way, you might have a dynamically computed redirect URI. In this case, the code is as follows:

```csharp
IConfidentialClientApplication app;
var request = httpContext.Request;
var currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, _azureAdOptions.CallbackPath ?? string.Empty);
app = ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(_applicationOptions)
       .WithRedirectUri(currentUri)
       .Build();
TokenCache userTokenCache = _tokenCacheProvider.SerializeCache(app.UserTokenCache,httpContext, claimsPrincipal);
```
