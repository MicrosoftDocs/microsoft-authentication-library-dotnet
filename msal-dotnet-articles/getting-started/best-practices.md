---
title: Best practices for MSAL.NET
description: Learn the best practices when using MSAL.NET in your application development scenario.
author: Dickson-Mwendia
manager: CelesteDG

ms.service: msal
ms.subservice: msal-dotnet
ms.topic: reference
ms.workload: identity
ms.date: 03/17/2023
ms.author: dmwendia
ms.reviewer:
ms.custom: devx-track-csharp, aaddev, engagement-fy23
# Customer intent: As an application developer, I want to learn the best practices for using MSAL.NET in my development scenario
---


# Best practices for MSAL.NET

## Never parse an access token

While you can have a look at the contents of an access token (for instance, using https://jwt.ms), for education, or debugging purposes, you should never parse an access token as part of your client code. The access token is only meant for the Web API or the resource it was acquired for. In most cases, web APIs use a middleware layer (for instance [Identity model extension for .NET](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki) in .NET), as this is complex code, about the protection of your web apps and Web APIs, and you don't want to introduce security vulnerabilities by forgetting some important paths.

<a name='dont-acquire-tokens-from-azure-ad-too-often'></a>

## Don't acquire tokens from Microsoft Entra ID too often

The standard pattern of acquiring tokens is: (i) acquire a token from the cache silently and (ii) if it doesn't work, acquire a new token from Microsoft Entra ID. If you skip the first step, your app may be acquiring tokens from Microsoft Entra too often. This provides a bad user experience, because it is slow and error prone as the identity provider might throttle you.

## Don't handle token expiration on your own

Even if `AuthenticationResult` returns the expiry of the token, you should not handle the expiration and the refresh of the access tokens on your own. MSAL.NET does this for you. For flows retrieving tokens for a user account, you'd want to use the recommended pattern as these write tokens to the user token cache, and tokens are retrieved and refreshed (if needed) silently by `AcquireTokenSilent`

```csharp
AuthenticationResult result;
try
{
 // will handle expired Access Tokens by fetching new ones using the Refresh Token
 result = await AcquireTokenSilent(scopes).ExecuteAsync();
}
catch(MsalUiRequiredException ex)
{
 result = AcquireTokenXXXX(scopes, ..).WithXXX(…).ExecuteAsync();
}
```

If you use `AcquireTokenForClient` in the client credentials flow, you don't need to worry about the cache as this method not only stores tokens to the application cache, but also looks them up and refreshes them if needed. This is the only method interacting with the application token cache, the cache for tokens for the application itself.
