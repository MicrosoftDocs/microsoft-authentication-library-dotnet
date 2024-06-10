---
title: Migrating to MSAL.NET and Microsoft.Identity.Web
description: Learn why and how to migrate from  Azure AD Authentication Library for .NET (ADAL.NET) to Microsoft Authentication Library for .NET (MSAL.NET) or Microsoft.Identity.Web
author: Dickson-Mwendia
manager: CelesteDG

ms.service: msal
ms.subservice: msal-dotnet
ms.topic: conceptual
ms.workload: identity
ms.date: 06/04/2024
ms.author: dmwendia
ms.reviewer: jmprieur, saeeda
ms.custom: devx-track-csharp, aaddev, has-adal-ref, engagement-fy23, devx-track-dotnet
#Customer intent: As an application developer, I want to learn why and how to migrate from ADAL.NET and MSAL.NET or Microsoft.Identity.Web libraries.
---

# Migrating applications to MSAL.NET or Microsoft.Identity.Web

## Why migrate to MSAL.NET or Microsoft.Identity.Web

Both the Microsoft Authentication Library for .NET (MSAL.NET) and Azure AD Authentication Library for .NET (ADAL.NET) are used to authenticate Microsoft Entra entities and request tokens from Microsoft Entra ID. Up until now, most developers have requested tokens from Azure AD for developers platform (v1.0) using Azure AD Authentication Library (ADAL). These tokens are used to authenticate Microsoft Entra identities (work and school accounts). 

MSAL comes with multiple benefits over ADAL, including the following:

- You can authenticate a broader set of Microsoft identities: work or school accounts, personal Microsoft accounts, and social or local accounts with Azure AD B2C,
- Your users get the best single-sign-on experience,
- Your application can enable incremental consent, Conditional Access,
- You benefit from continuous innovation in term of security and resilience,
- Your application implements the best practices in term of resilience and security.

**MSAL.NET or Microsoft.Identity.Web are now the recommended auth libraries to use with the Microsoft identity platform**. No new features will be implemented on ADAL.NET. The efforts are focused on improving MSAL.NET. For details see the announcement: [Update your applications to use Microsoft Authentication Library and Microsoft Graph API](https://techcommunity.microsoft.com/t5/azure-active-directory-identity/update-your-applications-to-use-microsoft-authentication-library/ba-p/1257363).

## Should you migrate to MSAL.NET or to Microsoft.Identity.Web

Before digging in the details of MSAL.NET vs ADAL.NET, you might want to check if you want to use MSAL.NET or a higher-level abstraction like [Microsoft.Identity.Web](../microsoft-identity-web/index.md).

For details about the decision tree below, read [MSAL.NET or Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/MSAL.NET-or-Microsoft.Identity.Web).

!["Block diagram explaining how to choose if you need to use MSAL.NET and Microsoft.Identity.Web or both when migrating from ADAL.NET"](../media/msal-net-migration/decision-diagram.png)


### Deprecated ADAL.NET NuGet packages and their MSAL.NET equivalents

You might unknowingly consume ADAL dependencies from other Azure SDKs. Below are few of the deprecated packages and their MSAL alternatives. For more detailed migration information, see [AppAuthentication to Azure.Identity Migration Guidance](/dotnet/api/overview/azure/app-auth-migration) and **Migration guide** links in the specific [Azure SDK for .NET](/dotnet/api/overview/azure/) library pages.

|  ADAL.NET Package (Deprecated) | MSAL.NET Package (Current) |
| ----------- | ----------- |
| `Microsoft.Azure.KeyVault`| `Azure.Security.KeyVault.Secrets, Azure.Security.KeyVault.Keys, Azure.Security.KeyVault.Certificates`|
| `Microsoft.Azure.Management.Compute`| `Azure.ResourceManager.Compute`|
| `Microsoft.Azure.Services.AppAuthentication`| `Azure.Identity`| 
| `Microsoft.Azure.Management.StorageSync`| `Azure.ResourceManager.StorageSync`| 
| `Microsoft.Azure.Management.Fluent`| `Azure.ResourceManager`| 
| `Microsoft.Azure.Management.EventGrid`| `Azure.ResourceManager.EventGrid`| 
| `Microsoft.Azure.Management.Automation`| `Azure.ResourceManager.Automation`| 
| `Microsoft.Azure.Management.Compute.Fluent`| `Azure.ResourceManager.Compute`|
| `Microsoft.Azure.Management.MachineLearning.Fluent`| `Azure.ResourceManager.MachineLearningCompute`|
| `Microsoft.Azure.Management.Media, windowsazure.mediaservices`| `Azure.ResourceManager.Media`|

## Next steps

- Learn about [public client and confidential client applications](/azure/active-directory/develop/msal-client-applications).
- Learn how to [migrate confidential client applications built on top of ASP.NET MVC or .NET classic from ADAL.NET to MSAL.NET](migrate-confidential-client.md).
- Learn how to [migrate public client applications built on top of .NET or .NET classic from ADAL.NET to MSAL.NET](migrate-public-client.md).
- Learn more about the [Differences between ADAL.NET and MSAL.NET apps](differences-adal-msal-net.md).
- Learn how to migrate confidential client applications built on top of ASP.NET Core from ADAL.NET to Microsoft.Identity.Web:
  -  [Web apps](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apps#migrating-from-previous-versions--adding-authentication)
  -  [Web APIs](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apis)
