---
title: Overriding authority
description: "How to override the default authority in MSAL.NET applications."
author: 
manager: 
ms.author: 
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: 
ms.custom: 
#Customer intent: 
---

# Overriding authority

In many scenarios, such as [client credential flow in multi-tenant apps](../advanced/client-credential-multi-tenant.md), it is useful to specify the Microsoft Entra tenant in the request builder instead of the application builder. `WithTenantId` is the recommended API to use in this scenario, which accepts the tenant ID string. `WithTenantIdFromAuthority` is another similar method that is available in MSAL 4.46.0+. You can also use `WithAuthority`, however, the authority in the application and the request builders must always be for the same cloud, i.e. the host of the authority URL must not be different.

```csharp
var app =  ConfidentialClientApplicationBuilder
                .Create(PublicCloudConfidentialClientID)
                .WithAuthority("https://login.microsoftonline.com/common", true)
                .Build();

var result = await app.AcquireTokenForClient(scopes)
                      .WithTenantId("123456-1234-2345-1234561234");
// OR
var result = await app.AcquireTokenForClient(scopes)
                      .WithTenantIdFromAuthority("https://login.microsoftonline.com/123456-1234-2345-1234561234");
```

A public or confidential client application instance can only be associated with one cloud. If your client application needs to handle multiple clouds at the same time, create a separate public or confidential client instance for each of them.
