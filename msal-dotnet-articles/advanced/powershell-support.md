---
title: Using MSAL.NET with PowerShell
description: "How to use MSAL.NET to acquire tokens from a PowerShell script."
author: Dickson-Mwendia
manager: 
ms.author: dmwendia
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: conceptual
ms.custom: 
#Customer intent: 
---

# Using MSAL.NET with PowerShell

There is **no official PowerShell module or wrapper** for MSAL libraries maintained by the Entra SDK team. Consider using maintained higher level SDKs:

- [Microsoft Graph PowerShell SDK](/powershell/microsoftgraph/installation)
- [Azure PowerShell SDK](/powershell/azure/new-azureps-module-az)

PowerShell was designed to be able to call into .NET code and there are [additional resources](https://stackoverflow.com/questions/3079346/how-to-reference-net-assemblies-using-powershell) that describe how to do this.
