---
title: Backup authentication system
description: "The Azure Active Directory backup authentication system enables the caching of credentials processed by the Evolved Security Token Service (ESTS) in order to provide resiliency during outages in Azure AD's authentication services."
---

# Backup authentication system

The Azure Active Directory has a backup authentication system that enables the caching of credentials in order to provide resiliency during outages in Azure AD's authentication services.

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
