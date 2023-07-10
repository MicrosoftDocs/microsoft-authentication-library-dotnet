---
title: Using MSAL.NET from PowerShell
description: "How to use MSAL.NET to acquire tokens from a PowerShell script."
---

# Using MSAL.NET from PowerShell

There is no official PowerShell module or wrapper for MSAL libraries maintained by the SDK team - our team recommends using a supported SDK.

Consider using higher level APIs which are officially supported: 

 -  [Microsoft Graph PowerShell SDK](https://learn.microsoft.com/powershell/microsoftgraph/get-started?view=graph-powershell-1.0)
 -  [Azure PowerShell SDK](https://learn.microsoft.com/powershell/azure/new-azureps-module-az?view=azps-10.0.0)

PowerShell was designed to be able to call into .NET code and there are [numerous resources](https://stackoverflow.com/questions/3079346/how-to-reference-net-assemblies-using-powershell) that describe how to do this.

For non-production scenarios or cases where you have the ability to leverage community tools we recommend using [MSAL.PS](https://www.powershellgallery.com/packages/MSAL.PS/).
