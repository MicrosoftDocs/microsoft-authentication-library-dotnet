---
title: Microsoft Identity Web scenarios
description: Learn the application scenarios and authentication flows supported by Microsoft Identity Web.
services: active-directory
author: Dickson-Mwendia
manager: CelesteDG

ms.service: active-directory
ms.subservice: develop
ms.topic: reference
ms.workload: identity
ms.date: 03/17/2023
ms.author: dmwendia
ms.reviewer: localden
ms.custom: devx-track-csharp, aaddev, engagement-fy23
# Customer intent: As an application developer, I want to know the application scenarios and authentication flows supported by Microsoft Identity Web. 
---

# Microsoft identity web scenarios

## Introduction

Microsoft.Identity.Web proposes a higher level API, over MSAL.NET, to protect web APIs, and acquire tokens in web apps, web APIs and services/daemon apps. You can choose to describe your application through a configuration file, or programmatically, or use a combination of both approaches.

## The Scenarios

Microsoft identity web is focused on services, and confidential client applications.

### Web app that signs in users and calls a web API on behalf of the user

To protect a web app (signing in the user) you'll use ASP.NET or ASP.NET Core with the ASP.NET Open ID Connect middleware. You'll reference the [Microsoft.Identity.Web](https://www.nuget.org/packages/Microsoft.Identity.Web) NuGet package if you use ASP.NET Core, and [Microsoft.Identity.Web.OWIN](https://www.nuget.org/packages/Microsoft.Identity.Web.OWIN) if you are still using ASP.NET (OWIN).

If moreover, your web apps calls web APIs in the name of the user (or in its own name), you'll add the following NuGet packages:

- [Microsoft.Identity.Web.MicrosoftGraph](https://www.nuget.org/packages/Microsoft.Identity.Web.MicrosoftGraph) if you want to call Microsoft Graph
- [Microsoft.Identity.Web.Azure](https://www.nuget.org/packages/Microsoft.Identity.Web.Azure) if you want to access an Azure resource with one of the Azure SDKs (Storage, etc ...)
- [Microsoft.Identity.Web.DownstreamApi](https://www.nuget.org/packages/Microsoft.Identity.Web.DownstreamApi) if you want to call a downstream web API

### Desktop or service daemon app that calls a web API as itself (in its own name)

You can write a daemon app that acquires a token using its own identity with a few lines of code, using the [Microsoft.Identity.Web.TokenAcquisition](https://www.nuget.org/packages/Microsoft.Identity.Web.TokenAcquisition) Nuget package.

### Web API calling another downstream Web API in the name of the user for whom it was called, or in its own name

To protect a web API you'll use ASP.NET or ASP.NET Core. You'll reference the [Microsoft.Identity.Web](https://www.nuget.org/packages/Microsoft.Identity.Web) NuGet package if you use ASP.NET Core, and [Microsoft.Identity.Web.OWIN](https://www.nuget.org/packages/Microsoft.Identity.Web.OWIN) if you are still using ASP.NET (OWIN).

If moreover, your web apps calls web APIs in the name of the user (or in its own name), you'll add the following NuGet packages:

- [Microsoft.Identity.Web.MicrosoftGraph](https://www.nuget.org/packages/Microsoft.Identity.Web.MicrosoftGraph) if you want to call Microsoft Graph
- [Microsoft.Identity.Web.Azure](https://www.nuget.org/packages/Microsoft.Identity.Web.Azure) if you want to access an Azure resource with one of the Azure SDKs (Storage, etc ...)
- [Microsoft.Identity.Web.DownstreamApi](https://www.nuget.org/packages/Microsoft.Identity.Web.DownstreamApi) if you want to call a downstream web API