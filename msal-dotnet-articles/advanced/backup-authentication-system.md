---
title: Backup authentication system
description: "The Microsoft Entra backup authentication system enables the caching of credentials processed by the Evolved Security Token Service (ESTS) in order to provide resiliency during outages in Microsoft Entra authentication services."
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

# Backup authentication system

The Microsoft Entra ID has a backup authentication system that enables the caching of credentials in order to provide resiliency during outages in Microsoft Entra authentication services.

In order to assist in speeding up the token retrieval from the backup authentication system, MSAL will provide a routing hint in the form of a header or an extra query parameter in authentication requests sent to ESTS. MSAL will attempt to do this for most of the authentication scenarios but there may be cases where MSAL is not able to provide this hint due to the absence of user data. This issue can be resolved by the use of <xref:Microsoft.Identity.Client.AcquireTokenByAuthorizationCodeParameterBuilder.WithCcsRoutingHint(System.String)> and <xref:Microsoft.Identity.Client.AcquireTokenByAuthorizationCodeParameterBuilder.WithCcsRoutingHint(System.String,System.String)>.

Here is an example of how to use the <xref:Microsoft.Identity.Client.AcquireTokenByAuthorizationCodeParameterBuilder.WithCcsRoutingHint(System.String,System.String)>:

```csharp
ConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(TestConstants.ClientId)
                                                  .WithClientSecret(clientSecret)
                                                  .Build();
// When creating an authorization Uri
var uri = await app
               .GetAuthorizationRequestUrl(TestConstants.s_scope)
               .WithCcsRoutingHint(userObjectIdentifier, tenantIdentifier)
               .ExecuteAsync();

// When Acquiring a Token
app.AcquireTokenByAuthorizationCode(scopes, authCode)
               .WithCcsRoutingHint(userObjectIdentifier, tenantIdentifier)
               .ExecuteAsync()
```
