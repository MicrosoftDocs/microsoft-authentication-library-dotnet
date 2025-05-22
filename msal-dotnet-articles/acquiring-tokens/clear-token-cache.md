---
title: Clear the token cache (MSAL.NET)
description: Learn how to clear the token cache using the Microsoft Authentication Library for .NET (MSAL.NET).
services: active-directory
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 08/24/2023
ms.service: msal
ms.subservice: msal-dotnet
ms.workload: identity
ms.reviewer:
ms.topic: how-to
ms.custom: devx-track-csharp, aaddev, devx-track-dotnet
#Customer intent: As an application developer, I want to learn how how to clear the token cache so I can .
---

# Clear the token cache using MSAL.NET

## Web API and daemon apps

There is no API to remove the tokens from the cache. Cache size should be handled by setting eviction policies on the underlying storage. See [Cache Serialization](../how-to/token-cache-serialization.md?tabs=aspnetcore) for details on how to use a memory cache or distributed cache.

## Desktop, command line and mobile applications

When you [acquire an access token](/azure/active-directory/develop/msal-acquire-cache-tokens) using the Microsoft Authentication Library for .NET (MSAL.NET), the token is cached. When the application needs a token, it should first call the `AcquireTokenSilent` method to verify if an acceptable token is in the cache. 

Clearing the cache is achieved by removing the accounts from the cache. This does not remove the session cookie which is in the browser, though.  The following example instantiates a public client application, gets the accounts for the application, and removes the accounts.

```csharp
private readonly IPublicClientApplication _app;
private static readonly string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
private static readonly string Authority = string.Format(CultureInfo.InvariantCulture, AadInstance, Tenant);

_app = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(Authority)
                .Build();

var accounts = (await _app.GetAccountsAsync()).ToList();

// clear the cache
while (accounts.Any())
{
   await _app.RemoveAsync(accounts.First());
   accounts = (await _app.GetAccountsAsync()).ToList();
}

```

To learn more about acquiring and caching tokens, read [acquire an access token](/azure/active-directory/develop/msal-acquire-cache-tokens)
