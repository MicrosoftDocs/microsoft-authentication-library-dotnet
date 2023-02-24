# Choosing a version of MSAL.NET

Follow the decision tree to see if MSAL.NET alone is enough, or if you need [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web), or both.

![image](https://user-images.githubusercontent.com/19942418/110971276-83910700-830f-11eb-8c89-35bf10291ef3.png)

## Use MSAL.NET

You're building a desktop or mobile app. Use MSAL.NET directly and start acquiring tokens for your public client application. For details see:
- [Acquiring token in a desktop app](https://docs.microsoft.com/azure/active-directory/develop/scenario-desktop-acquire-token?tabs=dotnet), and using [WAM](wam)
- [Acquiring token in a mobile application](https://docs.microsoft.com/azure/active-directory/develop/scenario-mobile-acquire-token)

## Use **hybrid model** MSAL.NET and [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web/)

You're building a web app or a web API, or a daemon application (a confidential client application) running on .NET Framework or pure .NET Core (not ASP.NET Core). In MSAL.NET, an in-memory token cache is provided by default, however, in the case of web apps or web APIs, caching should be handled differently than for public client applications (desktop or mobile apps) as it requires to be partitioned correctly. It's highly recommended to leverage a token cache serializer, which can be a distributed cache, (e.g. Redis, Cosmos, or SQL Server, distributed in memory cache), or a correctly partitioned in memory cache.

By using token cache serializers you partition the token caches depending on the cache key that is used because the cache is swapped between the storage and MSAL's memory. This cache key is computed by MSAL.NET as a function of the flow you use

![image](https://user-images.githubusercontent.com/13203188/110454488-9618ff80-80c7-11eb-86a1-48ccd8ddaea4.png)

### Why do I need Microsoft Identity Web?

Microsoft Identity Web provides token cache serialization for you.  See [Token cache serialization](https://github.com/AzureAD/microsoft-identity-web/wiki/asp-net#token-cache-serialization-for-msalnet) for details.

Another example of leveraging Microsoft Identity Web from .NET classic (MVC) can be found in that [ConfidentialClientTokenCache sample](https://github.com/Azure-Samples/active-directory-dotnet-v1-to-v2/tree/master/ConfidentialClientTokenCache).

Examples of how to use token caches for web apps and web APIs are available in the [ASP.NET Core web app tutorial](https://docs.microsoft.com/samples/azure-samples/active-directory-aspnetcore-webapp-openidconnect-v2/enable-webapp-signin/) in the phase [2-2 Token Cache](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/2-WebApp-graph-user/2-2-TokenCache). For implementations have a look at the [TokenCacheProviders](https://github.com/AzureAD/microsoft-identity-web/tree/master/src/Microsoft.Identity.Web/TokenCacheProviders) folder in the [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web) repository.

Microsoft Identity Web also helps with [certificate loading](https://github.com/AzureAD/microsoft-identity-web/wiki/asp-net#help-loading-certificates). 

## Use [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web/)

I'm using ASP.NET Core. See what Microsoft Identity Web has to offer:

![image](https://user-images.githubusercontent.com/19942418/125811549-88eedf0f-81ab-456e-9503-3393a5ba0306.png)

### I'm building a new application

Use the Project Templates and the msidentity-app-sync tool. We have web app templates for web MVC, Razor, Blazor server, Blazorwasm hosted and not hosted. All for Azure AD or Azure AD B2C.

![image](https://user-images.githubusercontent.com/13203188/107696478-4acf2500-6cb2-11eb-9e78-2f211cd3f6ab.png)


[Web app project templates](https://github.com/AzureAD/microsoft-identity-web/wiki/web-app-template).

We have web API templates for gRPC and Azure Functions.

[Web API project templates](https://github.com/AzureAD/microsoft-identity-web/wiki/web-api-template).

Here's information on how to run the [msidentity-app-sync-tool](https://github.com/AzureAD/microsoft-identity-web/blob/master/tools/app-provisioning-tool/README.md) which is a command line tool which creates Microsoft identity platform applications in a tenant (Azure AD or Azure AD B2C) and updates the configuration code of your ASP.NET Core applications. The tool can also be used to update code from an existing Azure AD/Azure AD B2C application.

It's available on [NuGet](https://www.nuget.org/packages/msidentity-app-sync/).

### I'm adding auth to an existing app or I'm migrating from ADAL

Just take the code you need from Microsoft Identity Web to update your app. Here's an example:

![image](https://user-images.githubusercontent.com/13203188/95241144-aaea2200-080d-11eb-8633-51e7796750ce.png)


![image](https://user-images.githubusercontent.com/13203188/95241423-03212400-080e-11eb-99a3-6fbb7a38cd0c.png)


![image](https://user-images.githubusercontent.com/13203188/95241601-47142900-080e-11eb-9c0c-6ebf2febb9db.png)


![image](https://user-images.githubusercontent.com/13203188/95241777-8e9ab500-080e-11eb-92d7-dca52d37ec8b.png)
