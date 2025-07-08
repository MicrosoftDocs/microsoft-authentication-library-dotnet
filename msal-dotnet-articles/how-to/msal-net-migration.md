---
title: Migrating to MSAL.NET and Microsoft.Identity.Web
description: Learn why and how to migrate from  Azure AD Authentication Library for .NET (ADAL.NET) to Microsoft Authentication Library for .NET (MSAL.NET) or Microsoft.Identity.Web
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 06/04/2024
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: conceptual
ms.custom: devx-track-csharp, aaddev, has-adal-ref, devx-track-dotnet
#Customer intent: As an application developer, I want to learn why and how to migrate from ADAL.NET and MSAL.NET or Microsoft.Identity.Web libraries.
---

# Migrating applications to MSAL.NET or Microsoft.Identity.Web

[!INCLUDE [ADAL migration note](../includes/adal-migration-note.md)]

## Why migrate

Azure AD Authentication Library for .NET (ADAL.NET) [has been deprecated](https://devblogs.microsoft.com/identity/update-your-applications-from-adal-to-msal/) and no new features or bug fixes, including security bugs will be implemented. 
Application using ADAL will continue to work.

### Migration guide for apps using ADAL directly

Before digging in the details of MSAL.NET vs ADAL.NET, you might want to check if you want to use MSAL.NET or a higher-level library like [`Microsoft.Identity.Web`](../microsoft-identity-web/index.md). For details about the decision tree below, read [MSAL.NET or Microsoft.Identity.Web](../getting-started/choosing-msal-dotnet.md).

- Learn how to [migrate confidential client applications built on top of ASP.NET MVC or .NET classic from ADAL.NET to MSAL.NET](migrate-confidential-client.md).
- Learn how to [migrate public client applications built on top of .NET or .NET classic from ADAL.NET to MSAL.NET](migrate-public-client.md).
- Learn how to migrate confidential client applications built on top of ASP.NET Core from ADAL.NET to Microsoft.Identity.Web:
  - [Web apps](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apps#migrating-from-previous-versions--adding-authentication)
  - [Web APIs](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apis)

### Migration guide for apps using ADAL indirectly 

You might unknowingly consume ADAL dependencies from other SDKs. In other words, ADAL is a transitive depdendency. This still represents a risk to your application, as your application cannot upgrade ADAL to fix a potential security issue or to benefit from a security improvement.

To migrate, you first have to indentify the root depedency that consumes ADAL. In most cases, the root depedency is itself deprecated. To identify the root depedency, you can get use [Visual Studio nuget interface](https://learn.microsoft.com/nuget/consume-packages/install-use-packages-visual-studio) or the `dotnet nuget why` [command](https://learn.microsoft.com/dotnet/core/tools/dotnet-nuget-why)

Below are the most common deprecated packages and their MSAL alternatives. For more detailed migration information, see [AppAuthentication to Azure.Identity Migration Guidance](/dotnet/api/overview/azure/app-auth-migration) and **Migration guide** links in the specific [Azure SDK for .NET](/dotnet/api/overview/azure/) library pages.

|  Legacy Package (ADAL-dependent, deprecated)                  | Supported Package (MSAL-dependent, current) |
| ------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `Microsoft.Azure.KeyVault`                                    | `Azure.Security.KeyVault.Secrets, Azure.Security.KeyVault.Keys, Azure.Security.KeyVault.Certificates`|
| `Microsoft.Azure.Management.Compute`                          | `Azure.ResourceManager.Compute`                                                                      |
| `Microsoft.Azure.Services.AppAuthentication`                  | `Azure.Identity`                                                                                     |
| `Microsoft.Azure.Management.StorageSync`                      | `Azure.ResourceManager.StorageSync`                                                                  |
| `Microsoft.Azure.Management.Fluent`                           | `Azure.ResourceManager`                                                                              |
| `Microsoft.Azure.Management.EventGrid`                        | `Azure.ResourceManager.EventGrid`                                                                    |
| `Microsoft.Azure.Management.Automation`                       | `Azure.ResourceManager.Automation`                                                                   |
| `Microsoft.Azure.Management.Compute.Fluent`                   | `Azure.ResourceManager.Compute`                                                                      |
| `Microsoft.Azure.Management.MachineLearning.Fluent`           | `Azure.ResourceManager.MachineLearningCompute`                                                       |
| `Microsoft.Azure.Management.Media, windowsazure.mediaservices`| `Azure.ResourceManager.Media`                                                                        |
| `Microsoft.Azure.Management.Media, windowsazure.mediaservices`| `Azure.ResourceManager.Media`                                                                        |
| `Microsoft.Kusto.Client`                                      | `Microsoft.Azure.Kusto.Data`                                                                         |
| `Microsoft.Kusto.Ingest`                                      | `Microsoft.Azure.Kusto.Ingest`                                                                       |

## Next steps

- Learn about [public client and confidential client applications](/entra/identity-platform/msal-client-applications).
- Learn how to [migrate confidential client applications built on top of ASP.NET MVC or .NET classic from ADAL.NET to MSAL.NET](migrate-confidential-client.md).
- Learn how to [migrate public client applications built on top of .NET or .NET classic from ADAL.NET to MSAL.NET](migrate-public-client.md).
- Learn more about the [Differences between ADAL.NET and MSAL.NET apps](differences-adal-msal-net.md).
- Learn how to migrate confidential client applications built on top of ASP.NET Core from ADAL.NET to Microsoft.Identity.Web:
  - [Web apps](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apps#migrating-from-previous-versions--adding-authentication)
  - [Web APIs](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apis)
