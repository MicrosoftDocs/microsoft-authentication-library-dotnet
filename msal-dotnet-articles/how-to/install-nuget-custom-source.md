---
title: Installing MSAL.NET from custom NuGet package source
---

# Installing MSAL.NET from custom NuGet package source

There are times when you need to take a dependency on a non official version of MSAL:

* An MSAL developer hands has put in a fix for a bug and would like you to validate it
* You are making changes to MSAL on your own, package MSAL and want to try it out with an app

## Install a package from a different source

Easiest is to use local folder as a NuGet source - see details in this [Stack Overflow post](https://stackoverflow.com/questions/10240029/how-do-i-install-a-nuget-package-nupkg-file-locally)  (from my experience you don't need to use `nuget init` and `nuget add` for simple scenarios like this)

## What not to do

* Do not unzip the `*.nupkg` file and take a reference to the dll itself - there are many DLLs in the package and you might use the wrong one. 
* Do not try to copy-paste and rename the new package over an existing package in the NuGet cache - you'll have problems moving away from the non-official version back to an official version / clearing out the cache etc.

## Check the signatures

You should check that a package is signed, in this case MSAL has to be signed by Microsoft. NuGet will display this, and you can always check a package with [this amazing tool](https://www.microsoft.com/p/nuget-package-explorer/9wzdncrdmdm3?activetab=pivot%3Aoverviewtab). Microsoft will always sign both packages and DLLs inside packages, even for non-official releases. 
