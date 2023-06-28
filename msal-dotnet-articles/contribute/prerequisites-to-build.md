---
title: Prerequisites to build MSAL.NET
description: "Requirements to build MSAL.NET on your local computer."
---

# Prerequisites to build MSAL.NET

The following are instructions to setup Visual Studio to build various MSAL.NET solution files on Windows and Mac platforms.

## Windows

### Minimal Visual Studio installation

* Install or update Visual Studio 2022. Any edition, such as Community, Pro, or Enterprise will work.
* Install the following workloads:
  * .NET desktop development
  * Universal Windows Platform development
  * Mobile Development with .NET
  * .NET Core cross-platform development
* From the **Individual Components** tab, make sure these items are selected:
  * .NET Framework 4.5.2 targeting pack
  * .NET Framework 4.6.1 SDK
  * .NET Framework 4.6.1 targeting pack
  * .NET Framework 4.6.2 targeting pack
  * Android SDK setup (API level 29)
  * Windows 10 SDK 10.0.17134.0
  * Windows 10 SDK 10.0.17763.0
* Android 9.0 Pie and Android 8.1 Oreo are required. These are not installed through the Visual Studio Installer. Instead, use the Android SDK Manager (**Visual Studio** > **Tools** > **Android** > **Android SDK Manager…**)

With the setup above, you should be able to open and compile `Libs.sln` and `LibsAndSamples.sln` from the [MSAL.NET repository](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet).

### Troubleshooting

* If you get an exception similar to `"System.InvalidOperationException: Could not determine Android SDK location"` while restoring NuGet packages, make sure you have the latest Android SDK installed. If you do, you probably hit a bug with the Visual Studio Installer - uninstall and reinstall the SDK from the Visual Studio Installer.
* If you get an exception similar to `"System.TypeLoadException: Could not set up parent class, due to: Could not load type of field 'Microsoft.Identity.Core.UI.WebviewBase:asWebAuthenticationSession' (5) due to: Could not resolve type with token 0100004c from typeref (expected class AuthenticationServices.ASWebAuthenticationSession in assembly 'Xamarin.iOS)"` when running on an iOS simulator, **make sure that you have installed the latest Visual Studio 2022 release and the latest version of [Xcode](https://developer.apple.com/xcode/) on your Mac** to get the classes needed to run `ASWebAuthenticationSession` (e.g., `AuthenticationServices`).

## macOS

### [Install Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/)

* During setup, install
  * .NET Core
  * Android
  * iOS
  * MacOS
* In Visual Studio for Mac, select **Tools** > **SDK Manager** and install Android SDK with API 29.

The steps above should enable you to compile `Libs.sln`. You will need a developer certificate to compile `LibsMacOS.sln`.
