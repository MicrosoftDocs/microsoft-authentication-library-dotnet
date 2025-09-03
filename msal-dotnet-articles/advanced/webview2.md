---
title: Using WebView2 with MSAL.NET
description: "How to use the modern embedded browser based on Microsoft Edge with MSAL.NET applications."
author: Dickson-Mwendia
manager: 
ms.author: dmwendia
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: concept-article
ms.custom: 
#Customer intent: 
---

# Using WebView2 with MSAL.NET

WebView2 is a modern embedded browser runtime based on Microsoft Edge, capable of performing Windows Hello authentication, log in with FIDO keys, and more. This browser replaces the legacy web view based on Internet Explorer.

## Availability

To use WebView2 in your application, the following requirements must be met:

- Windows 10 OS or newer (supported by the WebView2 runtime).
- MSAL.NET version 4.28.0 and higher.
- [WebView2 runtime](/microsoft-edge/webview2/) must be installed on the machine.

### Package Requirements

The package required depends on your target framework and application type:

- **Legacy frameworks** (.NET Framework, .NET Core 3.1): Use [`Microsoft.Identity.Client.Desktop`](https://www.nuget.org/packages/Microsoft.Identity.Client.Desktop/)
- **.NET MAUI 9+ and WinAppSDK packaged apps**: Use [`Microsoft.Identity.Client.Desktop.WinUI3`](https://www.nuget.org/packages/Microsoft.Identity.Client.Desktop.WinUI3/)

## Call pattern

When attempting to use the new WebView2 runtime, keep in mind the following required changes:

- In applications written against `net6.0-windows` or newer, no changes are necessary.
- In applications written against .NET Framework, .NET Core, or base .NET 5 or above, add a reference to [`Microsoft.Identity.Client.Desktop`](https://www.nuget.org/packages/Microsoft.Identity.Client.Desktop/) and add [`.WithWindowsEmbeddedBrowserSupport()`](xref:Microsoft.Identity.Client.Desktop.DesktopExtensions.WithWindowsEmbeddedBrowserSupport*) when instantiating the public client application.

```csharp
var pca = PublicClientApplicationBuilder
        .Create("0f887a65-3c78-4b16-9529-6209b8741c26")
        .WithWindowsEmbeddedBrowserSupport()
        .Build();
```

>[!IMPORTANT]
>WebView2 is **not supported** for Microsoft Entra ID (formerly known as Azure Active Directory) authorities. Using [`.WithWindowsEmbeddedBrowserSupport()`](xref:Microsoft.Identity.Client.Desktop.DesktopExtensions.WithWindowsEmbeddedBrowserSupport*) in that context is always going to default to the legacy web view, regardless of the setting provided during the instantiation of the public client application. This is caused by stability bugs identified during MSAL development. For B2C and Active Directory Federation Services (ADFS) authorities, WebView2 will be shown.

## Behaviour

|  Framework      | Embedded web view             | Default web view |
|-----------------|-------------------------------|------------------|
|  .NET Framework | Legacy, WebView2 with fallback to Legacy<sup>1</sup>  | Legacy embedded |  
|  .NET Core      | WebView2 with fallback to Legacy<sup>2</sup> | System |
|  .NET 6<sup>3</sup> | WebView2 with fallback to Legacy<sup>2</sup> | System |
|  .NET 6 Windows | WebView2, fallback to Legacy | WebView2, embedded |
|  MAUI 9+/WinAppSDK packaged apps | WebView2 with WinUI 3, no fallback to legacy<sup>4</sup> | WebView2, embedded |

<sup>1</sup> Legacy web view is the default in .NET Framework apps. WebView2 can be used via Microsoft.Identiy.Client.Desktop package.
<sup>2</sup> .NET Core and .NET 6+ apps can only use web view via Microsoft.Identity.Client package.
<sup>3</sup> MSAL.NET doesn't provide explicit `net5.0` binaries. .NET 5 apps will follow .NET Core behavior.
<sup>4</sup> Use WinUI 3-based WebView2 implementation with B2C/ADFS support for MAUI 9+ and WinAppSDK packaged applications.

### When to Use

Use `Microsoft.Identity.Client.Desktop.WinUI3` when:
- Building .NET MAUI 9+ applications
- Developing WinAppSDK packaged applications
- Requiring reliable B2C or ADFS authentication flows
- Requiring WinRT framework compatibility

Use `Microsoft.Identity.Client.Desktop` in all other scenarios.

## Troubleshooting

### Package Selection Issues

If you encounter issues with WebView2 integration, ensure you're using the correct package for your target framework:

- **Error**: Could not load file or assembly 'Microsoft.Web.WebView2.Core' or WinRT-related exceptions in .NET MAUI 9+ or WinAppSDK packaged apps
- **Solution**: Switch from `Microsoft.Identity.Client.Desktop` to `Microsoft.Identity.Client.Desktop.WinUI3` for MAUI 9+ and WinAppSDK packaged applications.

### WebView2 on .NET Framework

There's a scenario when an app that targets .NET Framework and references the [MSAL.NET NuGet package](https://www.nuget.org/packages/Microsoft.Identity.Client/) tries to acquire token interactively with WebView2 embedded browser, WebView2 will throw exceptions. These errors can be `System.BadImageFormatException: An attempt was made to load a program with an incorrect format.` or `System.DllNotFoundException: 'Unable to load DLL 'WebView2Loader.dll': The specified module could not be found.` As of MSAL.NET 4.32.0 these exceptions are wrapped into [`MsalClientException`](xref:Microsoft.Identity.Client.MsalClientException).

This occurs due to an existing bug in the WebView2 SDK on the .NET Framework platform because the library cannot figure out the target platform of its dependencies. One workaround is to add a `<PlatformTarget>` with values `AnyCPU`, `x86`, or `x64` to your application project file. `x86` or `x64` have to match the target framework of the WebView2 installed on the machine. Another workaround is to add `<PlatformTarget>AnyCPU</PlatformTarget>` in your app's project file and also directly reference the [WebView2 NuGet package](https://www.nuget.org/packages/Microsoft.Web.WebView2). For details, see issues [#2482](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/2482), [#730](https://github.com/MicrosoftEdge/WebView2Feedback/issues/730#issuecomment-803132248).
