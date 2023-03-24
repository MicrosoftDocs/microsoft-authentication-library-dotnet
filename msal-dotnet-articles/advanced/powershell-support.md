---
title: How to use MSAL.NET form PowerShell
description: "How to use MSAL.NET to acquire tokens from a PowerShell script."
---

# How to use MSAL.NET from PowerShell

There is no official PowerShell module or wrapper for MSAL libraries maintained by the SDK team. However, PowerShell was designed to be able to call into .NET code and there are [numerous resources](https://stackoverflow.com/questions/3079346/how-to-reference-net-assemblies-using-powershell) that describe how to do this.

A community project for a PowerShell wrapper exists at https://www.powershellgallery.com/packages/MSAL.PS/ 

## Make sure you load the correct DLL

After you download the [MSAL NuGet package](https://www.nuget.org/packages/Microsoft.Identity.Client/), unzip it, and take a look inside. In the `lib` folder there are the DLLs you are looking for:

![Contents of the MSAL NuGet package](../media/msal-folder-content.png)

If you are writing modules for the new PowerShell Core, then you should load the `netcoreapp2.1` version. If you are writing a module for PowerShell classic, then look into the `net45` directory. If you aren't sure, start with the `net45` version, which only works on Windows.

Avoid loading the `netstandard1.3` DLL, as this version is missing a lot of functionality.

## Don't forget about token caching

MSAL.NET will create and manage a token cache, but it will NOT persist it. You are responsible for persisting and encrypting the token cache. If you do not, MSAL will only keep the token cache in memory, and when the process stops, the tokens are lost, and users will have to relogin.

### Windows

On Windows, all our samples use DPAPI to encrypt a file with the token cache. Inspect this sample for [details](https://github.com/azure-samples/active-directory-dotnet-desktop-msgraph-v2).

### Mac and Linux

If you target PowerShell Core / .NET Core, it important to understand the DPAPI encryption solution above will NOT work. For a cross platform token cache persistence implementation, have a look at:

https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet
