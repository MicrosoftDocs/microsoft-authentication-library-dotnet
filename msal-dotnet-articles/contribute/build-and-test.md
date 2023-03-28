---
title: Building and testing MSAL.NET
---

# Building and testing MSAL.NET

## Prerequisites to build MSAL.NET

Prerequisites to build MSAL.NET can be found [in our documentation](prerequisites-to-build.md).

## Fast build

MSAL.Net supports MANY target frameworks (seven!), but most of the time contributors are only interested in one or two. To get MSAL to build for all the frameworks, you'll need a hefty Visual Studio 2019 installation plus several Windows SDKs.

To work around against this, open [this file](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/src/client/Microsoft.Identity.Client/Microsoft.Identity.Client.csproj#L3) and comment out the targets you are not interested in. Keeping the pure .NET targets and eliminating UWP and Xamarin results in a fast build and you can run all unit tests.

You may need to restart VS after you comment out targets.

### Visual Studio for Mac

MSAL is a multi-target library and at the time of writing, VS for Mac is not able to understand and layout this project correctly. You can still build from the command line.

## Build

1. Load LibsAndSamples.sln for a bigger solution with lots of apps that exercise MSAL. Load LibsNoSamples.sln for a small solution that has the library and the tests.
2. Build in VS or via the command line with `msbuild /t:restore` and `msbuild`

Note: if you run into strong name validation issues, please log a bug. Workaround is to disable it on your dev box by running Admin Dev Prompt > `sn -Vr *`

## Run tests

You won't be able to run the Integration test or Automation tests because they require access to a Microsoft KeyVault which is locked down. These tests will run as part of our DevOps pipelines though.

## Package

From VS or from the command line if you wish to control the versioning:

`msbuild <msal>.csproj /t:pack /p:MsalClientSemVer=1.2.3-preview`

### Command Line

Use `msbuild` commands - `msbuild /t:restore` and `msbuild`. Do not rely on `dotnet` command line because it is only for .Net Core, but this library has many other targets.

Note: To enable us to target Xamarin as well as .Net core, we took a dependency on the MsBuild SDK extras - https://github.com/onovotny/MSBuildSdkExtras See this [issue](https://github.com/onovotny/MSBuildSdkExtras/issues/102) about using `dotnet`.
