---
title: Using MSAL.NET with Azure Functions
description: Learn how to use MSAL.NET in Azure Functions
author: Dickson-Mwendia
manager: CelesteDG
ms.service: msal
ms.subservice: msal-dotnet
ms.topic: reference
ms.workload: identity
ms.date: 03/17/2023
ms.author: dmwendia
ms.reviewer: localden
ms.custom: devx-track-csharp, aaddev, engagement-fy23
# Customer intent: As an application developer, I want to learn how to use MSAL.NET in Azure Functions
---

# Using MSAL.NET with Azure Functions

When using MSAL.NET in Azure Functions, it can happen that libraries are not copied to the directory.

You can add `<_FunctionsSkipCleanOutput>true</_FunctionsSkipCleanOutput>` to your .csproj file to prevent that.

See details in [Azure/azure-functions-host#5894](https://github.com/Azure/azure-functions-host/issues/5894)

See also how to build [Azure functions with Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Azure-Functions)
