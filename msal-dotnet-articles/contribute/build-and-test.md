---
title: Building and testing MSAL.NET
description: "How to build and test MSAL.NET on your local machine."
---

# Building and testing MSAL.NET

## Prerequisites to build MSAL.NET

Prerequisites to build MSAL.NET can be found in [Prerequisites to build MSAL.NET](prerequisites-to-build.md).

## Fast build

MSAL.NET supports several target frameworks, but most of the time contributors are only interested in one or two. To get MSAL to build for all frameworks, contributors will need a [hefty Visual Studio installation as well as several SDKs](prerequisites-to-build.md).

To work around this requirement, open [`Microsoft.Identity.Client.csproj`](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/src/client/Microsoft.Identity.Client/Microsoft.Identity.Client.csproj) and comment out the targets you are not interested in. Keeping the pure .NET targets and eliminating UWP and Xamarin results in a fast build as well as the ability to run all unit tests.

Visual Studio may need to be restarted to ensure that updated target frameworks take effect.

### Visual Studio for Mac

MSAL is a multi-target library and at the time of writing, Visual Studio for Mac is not able to understand and layout this project correctly. The library can still be built from the command line on macOS.

### Visual Studio

1. Load `LibsAndSamples.sln` for a bigger solution with lots of apps that showcase and exercise MSAL. Load `LibsNoSamples.sln` for a small solution that has the library and the tests.
2. Build in Visual Studio (if configured) or via the command line with `msbuild /t:restore` and `msbuild`. If using the command line, developers might need to use the [Visual Studio Developer Command Prompt](/visualstudio/ide/reference/command-prompt-powershell?view=vs-2022).

>[!NOTE]
>If you run into strong name validation issues, please [log a bug](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues). Workaround is to disable strong name validation on your dev box by running the following command in the Visual Studio Developer Command Prompt with Administrator permissions:
>
>```bash
>sn -Vr *
>```

>[!WARNING]
>You won't be able to run the integration or automation tests because they require access to a Microsoft Key Vault instance which is only accessible to the MSAL.NET engineering team. These tests will run as part of our automation pipelines in GitHub.

#### Package

You can create a package from Visual Studio or from the command line with custom version parameters:

```bash
msbuild <msal>.csproj /t:pack /p:MsalClientSemVer=1.2.3-preview
```

### Command Line

You can use `msbuild` commands to build the solution. Use `msbuild /t:restore` and `msbuild`.

>[!NOTE]
>Do not use the `dotnet` command line because it is only for .NET and .NET Core - this library has many other targets that are not possible to build with the .NET tooling.

>[!NOTE]
>To enable the library to target Xamarin as well as .NET and .NET Core, there is a dependency on [MsBuild SDK Extras](https://github.com/novotnyllc/MSBuildSdkExtras). See the ["Support for command line dotnet build or dotnet msbuild"](https://github.com/novotnyllc/MSBuildSdkExtras/issues/102) issue in regards to using `dotnet` CLI commands.
