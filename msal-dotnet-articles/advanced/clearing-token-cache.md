---
title: Cleaning the token cache
description: "How to clear the token cache used by MSAL.NET"
author: Dickson-Mwendia
manager: 
ms.author: dmwendia
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: concept-article
ms.custom: 
#Customer intent: 
---

# Clearing the token cache

Clearing the token cache is achieved by removing the accounts from the cache. This does not remove the session cookie which is in the browser.

The example below is using an instance of <xref:Microsoft.Identity.Client.IClientApplicationBase>.

```csharp
// Clear the cache
var accounts = await app.GetAccountsAsync();
while (accounts.Any())
{
   await app.RemoveAsync(accounts.First());
   accounts = await app.GetAccountsAsync();
}
```
