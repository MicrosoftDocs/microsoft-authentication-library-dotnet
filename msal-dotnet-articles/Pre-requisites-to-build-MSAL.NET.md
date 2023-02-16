## Components required to build the library

The following are instructions to setup Visual Studio (Mac) to build various solution files on Windows and Mac platform.

# Windows
## Minimal Visual Studio 2019 installation 

* Install or update Visual Studio 2019 (any edition: Community, Pro, or Enterprise) with the following workloads: 

  * .NET desktop development
  * Universal Windows Platform development
  * Mobile Development with .NET 
  * .NET Core cross-platform development 

* Then from the "Individual Components" tab,make sure these additional items are selected:

    * .NET Framework 4.5 targeting pack
    * .NET Framework 4.5.2 targeting pack
    * .NET Framework 4.6.1 SDK 
    * .NET Framework 4.6.1 targeting pack
    * .NET Framework 4.6.2 targeting pack
    * Android SDK setup (API level 29)
    * Windows 10 SDK 10.0.17134.0
    * Windows 10 SDK 10.0.17763.0


* Android 9.0 Pie and Android 8.1 Oreo are also required. These are not necessarily installed through the VS Installer, so instead use the Android SDK Manager (Visual Studio > Tools > Android > Android SDK Manager…)

* With the above setup, you shoud be able to open and compile Libs.sln and LibsAndSamples.sln 

 ### Troubleshooting
* If you get an exception similar to `"System.InvalidOperationException: Could not determine Android SDK location"` while restoring the NuGet packages, make sure you have the latest Android SDK installed (29 at the time of writing). If you do, you probably hit a bug with the VS installer - uninstall and reinstall the SDK from the Visual Studio Installer. 
* IF you get an exception similar to "{System.TypeLoadException: Could not set up parent class, due to: Could not load type of field 'Microsoft.Identity.Core.UI.WebviewBase:asWebAuthenticationSession' (5) due to: Could not resolve type with token 0100004c from typeref (expected class '`AuthenticationServices.ASWebAuthenticationSession`' in assembly 'Xamarin.iOS" when running on an iOS simulator, **make sure that you have installed the latest Visual Studio 2019 and at least XCode 10 on the Mac** to get the classes needed to run ASWebAuthenticationSession (ex. AuthenticationServices)

# Mac
## [Install Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/)
- In the set up, install
  - Dot Net Core
  - Android
  - iOS 
  - MacOS

 -  In Visual Studio for Mac, select Tools -> SDK Manager, install Android SDK with API 29
 - This should enable you to compile Libs.sln
 - You will need a developer certificate to compile LibsMacOS.sln

