---
title: Backup authentication system
---

# Backup authentication system

The Azure AD backup authentication system enables the caching of credentials process by ESTS in order to provide resiliency during outages in Azure AD's authentication services. In order to assist in speeding up the token retrieval from the backup authentication system, MSAL will provide a routing hint in the form of a header or an extra query parameter in authentication requests sent to ESTS. MSAL will attempt to do this for most of the authentication scenarios but there will come a situation where MSAL is not able to provide this hint due to the absence of user data. However, this can be resolved by the use of `WithCcsRoutingHint(string userObjectIdentifier, string tenantIdentifier)` or `WithCcsRoutingHint(string userName)`.

Here is an example of how to use the WithCCSRoutingHint api:

```csharp
     ConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(TestConstants.ClientId)
                                                   .WithClientSecret(clientSecret)
                                                   .Build();
     //When creating an authorization Uri
     var uri = await app
                    .GetAuthorizationRequestUrl(TestConstants.s_scope)
                    .WithCcsRoutingHint(userObjectIdentifier, tenantIdentifier)
                    .ExecuteAsync();

     //When Acquiring a Token
     app.AcquireTokenByAuthorizationCode(scopes, authCode)
                    .WithCcsRoutingHint(userObjectIdentifier, tenantIdentifier)
                    .ExecuteAsync()
     
```
