---
title: What is WebView2
---

# What is WebView2

A modern embedded browser based on Microsoft Edge, capable of performing Windows Hello, log-in with FIDO keys, etc. This browser replaces the old embedded WebView, based on an outdated version of Internet Explorer.

## Where is it available?

- All Windows versions
- MSAL.NET version 4.28.0 and higher
- [WebView2 runtime](https://developer.microsoft.com/microsoft-edge/webview2/) must be installed on the machine 

## Evergreen runtime

WebView2 runtime is available on most Windows 10 and Windows 11 machines by default. But it may not be available on older platforms.

## Changes to call pattern

- On .NET5-windows10.xxx, there is no change
- On .NET Classic and .NET Core 3.1, add a reference to [Microsoft.Identity.Client.Desktop]( https://www.nuget.org/packages/Microsoft.Identity.Client.Desktop/) and call `.WithDesktopFeatures()`

```csharp

var pca = PublicClientApplicationBuilder
    .Create("client_id")
    .WithDesktopFeatures()
    .Build()
```

## Behaviour

|  Framework      | Embedded WebView              | Default WebView |
|-----------------|-------------------------------|-----------------|
|  .NET Framework | WebView2, fallback to Legacy  | Embedded |  
|  .NET Core      | WebView2, fallback to Legacy* | Embedded |
|  .NET 5         | WebView2, fallback to Legacy* | Embedded |

*_In .NET Core and .NET 5, fallback to legacy WebView is available starting in MSAL 4.30.0._

## Troubleshooting

### WebView2 on .NET Framework

There's a scenario when an app that targets .NET Framework and references MSAL.NET NuGet package tries to acquire token interactively with WebView2 embedded browser, WebView2 will throw exceptions. These errors can be `System.BadImageFormatException: An attempt was made to load a program with an incorrect format.` or `System.DllNotFoundException: 'Unable to load DLL 'WebView2Loader.dll': The specified module could not be found.` (As of MSAL.NET 4.32.0 these exceptions are wrapped into `MsalClientException`.)

This occurs because of an existing bug in the WebView2 SDK on .NET Classic platform because it can't figure out the target platform of the dependencies. One possible workaround is to add a `<PlatformTarget>` with values `AnyCPU`, `x86`, or `x64` to your app's project file. (`x86` or `x64` have to match the target framework of the WebView2 installed on the machine.) Another workaround is to add `<PlatformTarget>AnyCPU</PlatformTarget>` in your app's project file and also directly reference WebView2 NuGet. For details, see issues [#2482](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/2482), [#730](https://github.com/MicrosoftEdge/WebView2Feedback/issues/730#issuecomment-803132248).
