# Using MSAL.NET for client credential flow in multi-tenant services

## Decision point - Microsoft.Identity.Web or Microsoft.Identity.Client (MSAL)?

If you use ASP.NET Core, you are encouraged to adopt [Microsoft.Indentity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki), which provides a higher level API over token acquisition and has better defaults. See [Is MSAL.NET right for me?](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Is-MSAL.NET-right-for-me%3F)

## Decision point - token caching

MSAL maintains a token cache which grows with each token acquired. MSAL manages token lifetimes in a smart way, so you should use its cache. You have the option of using in-memory caching or distributed caching. 

See /azure/active-directory/develop/msal-net-token-cache-serialization?tabs=aspnet

We recommend using persisted distributed caches (e.g. Redis, Cosmos etc.) for all user flows. 
We also recommend that multi-tenant service 2 service apps use persisted distributed caches. But you may get away with using a memory cache with evictions if you know that your service needs app tokens for a limited number of tenants. 

