---
title: Installing MSAL.NET from custom NuGet package source
description: "How to install NuGet from a source other than the NuGet package feed."
ms.service: msal
ms.subservice: msal-dotnet
ms.date: 05/20/2025
ms.reviewer: 
---

# Installing MSAL.NET from custom NuGet package source

There are times when you need to take a dependency on a non official version of MSAL:

* A MSAL developer hands has put in a fix for a bug and would like you to validate it
* You are making changes to MSAL on your own, package MSAL and want to try it out with an app

## Install a package from a local source

The easiest approach is to use a [local folder](/nuget/hosting-packages/local-feeds) as a NuGet package source. This enables developers to read content from their own machine without uploading the package to a remote server.

## What not to do

* Do not extract the `*.nupkg` file and take a reference on the dynamic linked library (DLL) itself - there are many DLLs in the package and you might use the wrong one.
* Do not try to copy, paste, or rename the new package over an existing package in the NuGet cache - you'll have problems moving away from the non-official version back to an official version and clearing out the cache properly.

## Check the signatures

You should check that a package is signed. MSAL has to be signed by Microsoft. NuGet will display this information and you can always check a package with a tool such as [NuGet Package Explorer](https://github.com/NuGetPackageExplorer/NuGetPackageExplorer). Microsoft will always sign both packages as well as the DLLs inside, even for non-official and preview releases.
