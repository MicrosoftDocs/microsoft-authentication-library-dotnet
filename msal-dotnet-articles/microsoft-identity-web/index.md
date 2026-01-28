---
title: Microsoft Identity Web
description: Learn how you can use Microsoft Identity Web to add authentication and authorization to web apps, web APIs, and daemon applications. 
author: Dickson-Mwendia
manager: CelesteDG
ms.author: jmprieur
ms.date: 06/04/2024
ms.service: msal
ms.subservice: microsoft-identity-web
ms.reviewer:
ms.topic: concept-article
ms.custom: devx-track-csharp, aaddev
# Customer intent: As an application developer, I want to learn how Microsoft Identity Web can help me protect my services with the Microsoft identity platform. 
---

# Microsoft Identity Web authentication library

Microsoft Identity Web is a set of ASP.NET Core libraries that simplifies adding authentication and authorization support to web apps,  web APIs, and daemon apps integrating with the Microsoft identity platform. It provides a single-surface API convenience layer  that ties together ASP.NET or ASP.NET Core, their authentication middleware, and the [Microsoft Authentication Library (MSAL) for .NET](https://github.com/azuread/microsoft-authentication-library-for-dotnet) that acquires tokens. It can be installed via NuGet or by using a Visual Studio project template to create a new app project

## Supported application scenarios

When building ASP.NET Core web apps or web APIs that use Microsoft Entra ID or Microsoft Entra External ID for identity and access management (IAM), Microsoft Identity Web is recommended for these scenarios:

- [Service/daemon applications](/azure/active-directory/develop/scenario-daemon-overview)
- [Web app that signs in users](/azure/active-directory/develop/scenario-web-app-sign-user-overview)
- [Web app that signs in users and calls a web API on their behalf](/azure/active-directory/develop/scenario-web-app-call-api-overview)
- [Protected web API that only authenticated users can access](/azure/active-directory/develop/scenario-protected-web-api-overview)
- [Protected web API that calls another (downstream) web API on behalf of the signed-in user](/azure/active-directory/develop/scenario-web-api-call-api-overview)

## Supported platforms

Microsoft identity web is available for .NET 6+, .NET 4.6.2, .NET 4.7.2, and .NET Standard 2.0.

## Install from NuGet

Microsoft Identity Web is available on NuGet as a set of packages that provide modular functionality based on application requirements. Use the .NET CLI's `dotnet add` command or Visual Studio's **NuGet Package Manager** to install the appropriate packages:

- [Microsoft.Identity.Web](https://www.nuget.org/packages/Microsoft.Identity.Web) - The main package for ASP.NET Core applications.
- [Microsoft.Identity.Web.OWIN](https://www.nuget.org/packages/Microsoft.Identity.Web.OWIN) - The main package for ASP.NET (OWIN) applications.
- [Microsoft.Identity.Web.TokenAcquisition](https://www.nuget.org/packages/Microsoft.Identity.Web.TokenAcquisition) - The main package for other types of applications (daemon apps on .NET framework or .NET Core). Microsoft.Identity.Web and Microsoft.Identity.Web.OWIN reference this package. 
- [Microsoft.Identity.Web.UI](https://www.nuget.org/packages/Microsoft.Identity.Web.UI) - Optional, for ASP.NET Core web apps. Adds UI for user sign-in and sign-out and an associated controller for web apps.
- [Microsoft.Identity.Web.MicrosoftGraph](https://www.nuget.org/packages/Microsoft.Identity.Web.MicrosoftGraph) - Optional. Provides simplified interaction with the Microsoft Graph API.
- [Microsoft.Identity.Web.MicrosoftGraphBeta](https://www.nuget.org/packages/Microsoft.Identity.Web.MicrosoftGraphBeta) - Optional. Provides simplified interaction with the Microsoft Graph API [beta endpoint](/graph/api/overview?view=graph-rest-beta&preserve-view=true).
- [Microsoft.Identity.Web.DownstreamApi](https://www.nuget.org/packages/Microsoft.Identity.Web.DownstreamApi) - Optional. Provides simplified and declarative interaction with downstream APIs.
- [Microsoft.Identity.Web.Azure](https://www.nuget.org/packages/Microsoft.Identity.Web.Azure) - Optional. Provides simplified interaction with the Azure SDKs.

The following NuGet packages, which are referenced by the packages above, can also be used directly with MSAL.NET:

- [Microsoft.Identity.Web.TokenCache](https://www.nuget.org/packages/Microsoft.Identity.Web.TokenCache) - Provides simplified token cache serializers for MSAL.NET (In memory, distributed cache)
- [Microsoft.Identity.Web.Certificate](https://www.nuget.org/packages/Microsoft.Identity.Web.Certificate) - Provides simplified interaction with certificates
- [Microsoft.Identity.Web.Certificateless](https://www.nuget.org/packages/Microsoft.Identity.Web.Certificateless) - Provides simplified interaction with other forms of credentials not based on certificates (signed assertions from [workload identity federation](/azure/active-directory/workload-identities/workload-identity-federation), integration with Azure Kubernetes Services (AKS), ...)

## Install by using a Visual Studio project template

Several project templates that use *Microsoft.Identity.Web* are included in .NET SDK versions 6.0 and above.

### .NET 5.0+ - Project templates included

The Microsoft Identity Web project templates are included in .NET SDK versions 5.0 and above.

In the following example, .NET CLI command creates a Blazor Server project that includes Microsoft Identity Web.

```dotnetcli
dotnet new webapp --auth SingleOrg --calls-graph --client-id "00001111-aaaa-2222-bbbb-3333cccc4444" --tenant-id "aaaabbbb-0000-cccc-1111-dddd2222eeee" --output my-blazor-app
```
<!-- 
## Conceptual documentation

-->


### Getting started with MSAL.NET

1. Learn about [Scenarios](./getting-started/scenarios.md).
1. You'll need to [register your app](/azure/active-directory/develop/quickstart-register-app) with Microsoft Entra ID.

## Samples

See [our comprehensive sample list](/azure/active-directory/develop/active-directory-v2-code-samples).
