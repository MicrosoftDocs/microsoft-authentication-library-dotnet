---
title: Microsoft Authentication Library for .NET
description: Learn how you can use the Microsoft Authentication Library for .NET (MSAL.NET) to acquire tokens from the Microsoft identity platform and access protected web APIs. 
services: active-directory
author: Dickson-Mwendia
manager: CelesteDG

ms.service: active-directory
ms.subservice: develop
ms.topic: reference
ms.workload: identity
ms.date: 03/16/2023
ms.author: dmwendia
ms.reviewer: localden, jmprieur
ms.custom: devx-track-csharp, aaddev, engagement-fy23
# Customer intent: As an application developer, I want to learn how MSAL.NET can help me acquire tokens from the Microsoft identity platform and access protected web APIs. 
---

# Microsoft Authentication Library for .NET

MSAL.NET ([Microsoft.Identity.Client](https://www.nuget.org/packages/Microsoft.Identity.Client)) is an authentication library that enables you to acquire tokens from Azure Active Directory (Azure AD), to access protected web APIs (Microsoft APIs or applications registered with Azure AD). MSAL.NET is available on several .NET platforms (Desktop, Universal Windows Platform, Xamarin Android, Xamarin iOS, Windows 8.1, and .NET Core).

## Supported platforms and application architectures

MSAL.NET supports different application topologies, including:

- [Native clients](/azure/active-directory/develop/active-directory-dev-glossary#native-client)  (mobile or desktop applications) calling the Microsoft Graph API on behalf of a user,
- Daemons, services, or [web clients](/azure/active-directory/develop/active-directory-dev-glossary#web-client) (web apps or web APIs) calling the Microsoft Graph API on behalf of a user, or without a user.

With the exception of [User-agent based client](/azure/active-directory/develop/active-directory-dev-glossary#user-agent-based-client) which is only supported in JavaScript.

For more details about the supported scenarios, see [Scenarios](./getting-started/scenarios.md).

MSAL.NET supports multiple platforms, including .NET Framework, [.NET Core](https://www.microsoft.com/net/learn/get-started/windows)(including .NET 6), [Xamarin](https://www.xamarin.com/) Android, Xamarin iOS, and [UWP](/windows/uwp/get-started/universal-application-platform-guide).

> [!NOTE]
> Not all the authentication features are available in all platforms, mostly because:
>
>- Mobile platforms (Xamarin and UWP) do not allow confidential client flows, because they are not meant to function as a backend and cannot  store secrets securely.
>- On public clients (mobile and desktop), the default browser and redirect URIs are different from platform to platform and broker availability varies (details [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/MSAL.NET-uses-web-browser#at-a-glance)).

Most of the articles in this MSAL.NET reference content describe the most complete platform (.NET Framework), but, topic by topic, it occasionally calls out differences between platforms.

## Why use MSAL.NET ?

MSAL.NET ([Microsoft Authentication Library for .NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet)) enables developers of .NET applications to acquire tokens in order to call secured web APIs. These web APIs can be the Microsoft Graph API, other Microsoft APIS, 3rd party Web APIs, or your own Web API.

As a token acquisition library, MSAL.NET provides various ways of getting a token, with a consistent API for a number of platforms. Using MSAL.NET adds value over using OAuth libraries and coding against the protocol by:

- Maintains a **token cache** and **refreshes tokens** for you when they are close to expire.
- Eliminates the need for you to handle token expiration by yourself.
- Helps you specify which **audience** you want your application to sign-in (your organization, several organizations, work and school and Microsoft personal accounts, social identities with Azure AD B2C, users in sovereign and national clouds).
- Helps you set-up the application through **configuration** files.
- Helps you troubleshoot the app by exposing actionable exceptions, logging, and telemetry.

### MSAL.NET is about acquiring tokens, not protecting an API

MSAL.NET is used to acquire tokens. It's not used to protect a Web API. If you are interested in protecting a Web API with Azure AD, you might want to check out:

- [Azure Active Directory with ASP.NET Core](/aspnet/core/security/authentication/azure-active-directory/). Note that some of these examples present web apps which also call a web API with MSAL.NET.
- [Active-directory-dotnet-native-aspnetcore-v2](https://github.com/azure-samples/active-directory-dotnet-native-aspnetcore-v2) which shows how to call an ASP.NET Core Web API from a WPF application using Azure AD V2.
- The [IdentityModel extensions for .Net](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet) open source library providing middleware used by ASP.NET and ASP.NET Core to protect APIs.

## Conceptual documentation

### Getting started with MSAL.NET

1. Learn about [MSAL.NET usage scenarios](./getting-started/scenarios.md).
1. You will need to [register your app](/azure/active-directory/develop/quickstart-register-app) with Azure Active Directory.
1. Learn about the [types of client applications](/azure/active-directory/develop/msal-client-applications): public client and confidential client.
1. Learn about [acquiring tokens](acquiring-tokens/overview.md) to access a protected API.

### Acquiring tokens

#### Acquiring tokens from cache in any app

- [AcquireTokenSilentAsync](acquiring-tokens/acquiretokensilentasync-api.md) enables you to get a previously cached token.

#### Acquiring tokens in desktop and mobile apps (public client applications)

- [Acquiring a token interactively](acquiring-tokens/desktop-mobile/acquiring-tokens-interactively.md) enables the application to acquire a token after authenticating the user through an interactive sign-in. There are implementation-specific details depending on the target platforms, such as [Xamarin Android](acquiring-tokens/desktop-mobile/xamarin.md) or [UWP](acquiring-tokens/desktop-mobile/uwp.md).
- Acquiring a token silently on a Windows domain or Azure Active Directory joined machine with [Integrated Windows Authentication](./acquiring-tokens/desktop-mobile/integrated-windows-authentication.md) or by using [Username/passwords](./acquiring-tokens/desktop-mobile/username-password-authentication.md) (not recommended).
- Acquiring a token on a text-only device, by directing the user to sign-in on another device with the [Device Code Flow](./acquiring-tokens/desktop-mobile/device-code-flow.md).

#### Acquiring tokens in web apps, web APIs, and daemon apps (confidential client applications)

- Acquiring a token for the app (without a user) with [client credential flows](acquiring-tokens/web-apps-apis/client-credential-flows.md).
- Acquiring a token [on behalf of a user](acquiring-tokens/web-apps-apis/on-behalf-of-flow.md) in service-to-service calls.
- Acquiring a token for the signed-in user [by authorization code](acquiring-tokens/web-apps-apis/authorization-codes.md) in Web Apps.

#### Confidential client availability

MSAL.NET is a multi-framework library. All confidential client flows **are available on**:

- .NET Core
- .NET Desktop
- .NET Standard

They are not available on the mobile platforms, because the OAuth2 spec states that there should be a secure, dedicated connection between the application and the identity provider. This secure connection can be achieved on web servers and web API back-ends by deploying a certificate (or a secret string, but this is not recommended for production). It cannot be achieved on mobile apps and other client applications that are distributed to users. As such, these confidential flows **are not available on**:

- Xamarin.Android / MAUI Android
- Xamarin.iOS / MAUI iOS
- UWP

## Releases

For previous releases, see the [Releases page on GitHub](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases). Minor (feature) releases are published every month. A feature could be included in a release or not depending on its complexity. Smaller patch or urgent fixes can be releases more frequently. Some of the security issues are back ported to the last major/minor release.

For work-in-progress and future releases, see [Milestones](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/milestones).

For additional information on versioning, see [Semantic versioning - API change management](resources/semantic-versioning-api-change-management.md) to understand changes in MSAL.NET public API.

## Samples

See [our comprehensive sample list](/azure/active-directory/develop/active-directory-v2-code-samples).

## FAQ

- How MSAL.NET uses [web browsers](/azure/active-directory/develop/msal-net-web-browsers) for interactive authentication.
- If you have issues with Xamarin.Forms applications leveraging MSAL.NET please read [Troubleshooting Xamarin.Android issues with MSAL](/azure/active-directory/develop/msal-net-xamarin-android-considerations).
