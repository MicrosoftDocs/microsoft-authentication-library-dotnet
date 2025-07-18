---
title: Microsoft Authentication Library for .NET
description: Learn how you can use the Microsoft Authentication Library for .NET (MSAL.NET) to acquire tokens from the Microsoft identity platform and access protected web APIs. 
author: Dickson-Mwendia
manager: CelesteDG
ms.author: dmwendia
ms.date: 06/04/2024
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: article
ms.custom: devx-track-csharp, aaddev
#Customer intent: 
# Customer intent: As an application developer, I want to learn how MSAL.NET can help me acquire tokens from the Microsoft identity platform and access protected web APIs. 

---

# Microsoft Authentication Library for .NET

MSAL.NET ([Microsoft.Identity.Client](https://www.nuget.org/packages/Microsoft.Identity.Client)) is an authentication library that enables you to acquire tokens from Microsoft Entra ID to access protected web APIs (Microsoft APIs or applications registered with Microsoft Entra ID).

MSAL.NET is available on several .NET platforms (desktop, mobile, and web).

> [!div class="nextstepaction"]
> [Get MSAL.NET >](https://www.nuget.org/packages/Microsoft.Identity.Client/)

## Supported platforms and application architectures

MSAL.NET supports different application topologies, including:

- [Native clients](/azure/active-directory/develop/active-directory-dev-glossary#native-client) (mobile or desktop applications) calling the Microsoft Graph API on behalf of a user.
- Daemons, services, or [web clients](/azure/active-directory/develop/active-directory-dev-glossary#web-client) (web apps or web APIs) calling the Microsoft Graph API on behalf of a user, or without a user.

For more information about the supported scenarios, see [Scenarios](./getting-started/scenarios.md).

MSAL.NET supports multiple platforms, including [.NET](https://dotnet.microsoft.com/), [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework), and [.NET MAUI](https://dotnet.microsoft.com/apps/maui).

> [!NOTE]
> Not all the authentication features are available in all platforms.
>
>- Mobile platforms do not allow confidential client flows. They are not meant to function as a backend and cannot store secrets securely.
>- On public clients (mobile and desktop) the default browser and redirect URIs are different from platform to platform, and broker availability varies (details [in browser usage documentation](./acquiring-tokens/using-web-browsers.md)).

> [!NOTE]
> MSAL.NET is optimized for use with Microsoft Entra ID as the identity provider (IDP). 
> While it is possible to use MSAL.NET with third-party IDPs that support OAuth 2.
> 0—particularly when using embedded or system browsers((see [in browser usage documentation](./acquiring-tokens/using-web-browsers.md)))
> —interoperability is not guaranteed. Microsoft does not provide support for issues arising from third-party IDP integrations. Such scenarios are considered
> best-effort and may not be addressed.

> [!NOTE]
> MSAL.NET versions 4.61.0 and above do not provide support for Universal Windows Platform, Xamarin Android, and Xamarin iOS. Read more about the deprecation in [Announcing the Upcoming Deprecation of MSAL.NET for Xamarin and UWP](https://devblogs.microsoft.com/identity/uwp-xamarin-msal-net-deprecation/).

## Why use MSAL.NET?

MSAL.NET provides several ways of getting a token. Using MSAL.NET is easier than using generic OAuth libraries or writing calls against the protocol. MSAL.NET provides several out-of-the-box benefits that simplify the developer workflow:

- Maintain a **token cache** and **refresh tokens** for you when they're close to expiry.
- Helps you specify which **audience** you want your application to sign-in (your organization, several organizations, work, school, and Microsoft personal accounts, social identities with Microsoft Entra External ID, or users in sovereign and national clouds).
- Helps you set up the application through **configuration** files.
- Helps you troubleshoot the app by exposing actionable exceptions, logging, and telemetry.

## Getting started with MSAL.NET

1. Learn about [MSAL.NET usage scenarios](./getting-started/scenarios.md).
1. [Register your app](/azure/active-directory/develop/quickstart-register-app) with Microsoft Entra ID.
1. Learn about the [types of client applications](/entra/identity-platform/msal-client-applications): public client and confidential client.
1. Learn about [acquiring tokens](acquiring-tokens/overview.md) to access a protected API.

## Considerations

MSAL.NET is used to acquire tokens. It's not used to protect a Web API. If you're interested in protecting a Web API with Microsoft Entra ID, check out:

- [Microsoft Entra ID with ASP.NET Core](/aspnet/core/security/authentication/azure-active-directory/). Examples showcase web apps that call a web API with MSAL.NET.
- [active-directory-dotnet-native-aspnetcore-v2](https://github.com/azure-samples/active-directory-dotnet-native-aspnetcore-v2) shows how to call an ASP.NET Core Web API from a WPF application using Microsoft Entra ID.
- The [IdentityModel extensions for .NET](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet) open source library provides middleware used by ASP.NET and ASP.NET Core to protect APIs.

## Migration from Azure Active Directory Authentication Library (ADAL)

Microsoft Authentication Library (MSAL) for .NET is the supported library that can be used for authentication token acquisition. If you or your organization are using the Azure Active Directory Authentication Library (ADAL), you should [migrate to MSAL](/entra/identity-platform/msal-migration). ADAL reached end-of-life on **June 30, 2023**.

> [!NOTE]
> While ADAL is deprecated since June 30, 2023, applications depending on ADAL should not break as the underlying endpoint will remain active. However, no new features or support will be offered for ADAL.

## Releases

For previous releases, see the [Releases on GitHub](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases).

For work-in-progress and future releases, see [Milestones](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/milestones).

For more information on versioning, see [Semantic versioning - API change management](resources/semantic-versioning-api-change-management.md) to understand changes in MSAL.NET public API.

## Samples

See [our comprehensive sample list](/entra/identity-platform/sample-v2-code).
