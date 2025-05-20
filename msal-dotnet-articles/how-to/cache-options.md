---
title: Cache options in MSAL.NET
ms.service: msal
ms.subservice: msal-dotnet
---

# Cache options in MSAL.NET

## Setting cache options

```csharp

var app = ConfidentialClientApplicationBuilder.Create(ClientId)
               .WithCertificate(cert)                                                               
               .Build();

// The App token cache is used by `AcquireTokenForClient`, which gets tokens on behalf of service principals
app.AppTokenCache.SetCacheOptions(CacheOptions.EnableSharedCacheOptions);

// The User token cache is used by all other AcquireToken* methods, which get tokens on behalf of users
app.UserTokenCache.SetCacheOptions(CacheOptions.EnableSharedCacheOptions);
```

## Cache options

`EnableSharedCacheOptions` - makes the cache static, so that it is shared between all instances of `ConfidentialClientApplication`.
