---
title: Choosing a version of MSAL.NET
description: Learn how to choose a version of MSAL.NET that suits your development scenario, based on the type of application and the underlying platform.
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 03/17/2023
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: reference
ms.custom: devx-track-csharp, aaddev, sfi-image-blocked
# Customer intent: As an application developer, I want to know which version of MSAL.NET I'll use for my scenario based on the type of app I'm building and the platform I'm using. 
---

# Choosing a version of MSAL.NET

> [!IMPORTANT]
> Effective May 1, 2025, Azure AD B2C will no longer be available to purchase for new customers. [Learn more in our FAQ](/azure/active-directory-b2c/faq?tabs=app-reg-ga#azure-ad-b2c-end-of-sale).

Depending on the type of application you're building, and its underlying platform, you can choose to use MSAL.NET, [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web), or both.

Microsoft Identity Web is a set of ASP.NET Core libraries that simplifies adding authentication and authorization support to web apps and web APIs integrating with the Microsoft identity platform. It provides a single-surface API convenience layer that ties together ASP.NET Core, its authentication middleware, and the Microsoft Authentication Library (MSAL) for .NET.

Follow the decision tree below to determine whether your scenario requires MSAL.NET, Microsoft Identity Web, or both.

![Image of the decision tree when working with .NET auth libraries](../media/idweb-msal.png)

## When do I use MSAL.NET

You're building a desktop or mobile app. Use MSAL.NET directly and start acquiring tokens for your public client application. For details see:

- [Acquiring token in a desktop app](/azure/active-directory/develop/scenario-desktop-acquire-token?tabs=dotnet), and using [WAM](../acquiring-tokens/desktop-mobile/wam.md)
- [Acquiring token in a mobile application](/azure/active-directory/develop/scenario-mobile-acquire-token)

## Use [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web/)

You're building a confidential client application (Web app, web API, daemon/service app) running on ASP.NET Core, ASP.NET OWIN, or .NET framework/.NET Core. See what Microsoft Identity Web has to offer:

- Sign users in via web apps in Microsoft Entra ID, Azure AD B2C, and Microsoft Entra External ID applications
  - Support Microsoft personal accounts
  - Support guest users
  - Incremental consent and conditional access in web apps
  - Handle SameSite
  - Integrates with "App services authentication"
  - Supports PKCE for confidential client applications
  - Brings performant token cache serializers, including distributed
- Protect web API (with Microsoft Entra ID, Azure AD B2C, or Microsoft Entra External ID)
  - Validates the issuer (including in-multi-tenant apps, any cloud)
  - supports token decrypt certificates in Web APIs
  - Validates Scope and app role in Web APIs
  - Generates `WWW-authenticate` headers in APIs (CA, CAE)
  - Protect gRPC services and Azure functions
- Web app/API calling downstream APIs (including graph except for B2C)
  - Call downstream APIs without having to manage authentication and tokens yourself.
  - Integrates with the graph SDK, and the Azure SDKs
  - Describe the client credentials, and Microsoft.Identity.Web fetches them for you (for
instance certificates from Key Vault, or [workload identity federation](/azure/active-directory/workload-identities/workload-identity-federation) with [Azure Kubernetes Service (AKS)](https://azure.microsoft.com/products/kubernetes-service) and [Managed Identities](/azure/active-directory/managed-identities-azure-resources/overview))
- Supports multiple Authentication schemes in ASP.NET Core
- Supports Proof of Possession protocol
- Resilient (supports regional token acquisition and  routing hint for the token backup system)

### You're building a new application

Use the Project Templates and the `msidentity-app-sync` tool. We have web app templates for web MVC, Razor, Blazor server, Blazorwasm hosted and not hosted. All for Microsoft Entra ID or Azure AD B2C.

![Image showing ASP.NET Core projects templates for building web apps](../media/aspnet-core-project-templates.png)

[Web app project templates](https://github.com/AzureAD/microsoft-identity-web/wiki/web-app-template).

We have web API templates for gRPC and Azure Functions.

[Web API project templates](https://github.com/AzureAD/microsoft-identity-web/wiki/web-api-template).

Here's information on how to run the [msidentity-app-sync-tool](https://github.com/AzureAD/microsoft-identity-web/blob/master/tools/app-provisioning-tool/README.md) which is a command line tool which creates Microsoft identity platform applications in a tenant (Microsoft Entra ID or Azure AD B2C) and updates the configuration code of your ASP.NET Core applications. The tool can also be used to update code from an existing Microsoft Entra application or Azure AD B2C application.

It's available on [NuGet](https://www.nuget.org/packages/msidentity-app-sync/).

### You're adding auth to an existing app or I'm migrating from ADAL

Just take the code you need from Microsoft Identity Web to update your app. Here's an example:

![image showing code updates when building a web app that calls a web API](../media/azure-ad-calling-api.png)

![image showing code updates when building a B2C web app or API](../media/configureservices-startup.png)

![image showing code updates for a B2C web app that signs in users and a protected web API](../media/azure-ad-b2c-appsettings.png)

![image showing code updates in a web app or web API that calls a downstream API](../media/azure-ad-b2c-controller.png)

## When do you use the hybrid model (MSAL.NET and [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web/))

You are building a SDK for confidential client applications and want to use MSAL.NET low level APIs. In MSAL.NET, an in-memory token cache is provided by default, however, in the case of web apps or web APIs, caching should be managed differently than for public client applications (desktop or mobile apps) as it requires to be partitioned correctly. It is highly recommended to leverage a token cache serializer, which can be a distributed cache, (e.g., Redis, Cosmos, or SQL Server, distributed in memory cache), or a correctly partitioned in memory cache.

By using token cache serializers you partition the token caches depending on the cache key that is used because the cache is swapped between the storage and MSAL's memory. This cache key is computed by MSAL.NET as a function of the flow you use

![Image showing token caches with and without custom serializers](../media/msal-serializers.png)

### Why do you need Microsoft.Identity.Web.TokenCache?

[Microsoft.Identity.Web.TokenCache](https://www.nuget.org/packages/Microsoft.Identity.Web.TokenCache) provides token cache serialization for you. See [Token cache serialization](../how-to/token-cache-serialization.md) for details.

Examples of how to use token caches for web apps and web APIs are available in the [ASP.NET Core web app tutorial](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/) in the phase [2-2 Token Cache](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/2-WebApp-graph-user/2-2-TokenCache). For implementations have a look at the [TokenCacheProviders](https://github.com/AzureAD/microsoft-identity-web/tree/master/src/Microsoft.Identity.Web/TokenCacheProviders) folder in the [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web) repository.

Microsoft Identity Web also helps with [certificate loading](https://github.com/AzureAD/microsoft-identity-web/wiki/asp-net#help-loading-certificates).
