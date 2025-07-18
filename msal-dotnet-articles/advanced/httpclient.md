---
title: Providing your own HttpClient, supporting HTTP proxies, and customization of user agent headers
description: "There are cases where developers want fine-grained control of the HttpClient instance, such as configuring a proxy or using ASP.NET Core's efficient ways of pooling the HttpClient."
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

# Providing your own HttpClient, supporting HTTP proxies, and customization of user agent headers

There are cases where developers want fine-grained control of the `HttpClient` instance, such as configuring a proxy or using ASP.NET Core's efficient ways of pooling the `HttpClient`. You can read more in the [HttpClientFactory to implement resilient HTTP requests](/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) document. To customize `HttpClient`, developers will need to implement `IMsalHttpClientFactory`, which MSAL will then use to get a `HttpClient` for each HTTP request.

## IMsalHttpClientFactory implementation guidelines

- See <xref:System.Net.Http.HttpClient?displayProperty=fullName> for examples of scalable .NET factories which can be adapted for this interface, such as [ASP.NET Core's `IHttpClientFactory`](/aspnet/core/fundamentals/http-requests).
- Implementations must be thread-safe.
- Do not create a new `HttpClient` in `GetHttpClient`, as this will lead to port exhaustion.
- MSAL will not call `Dispose()` on the `HttpClient`.
- If your app uses [Integrated Windows Authentication](../acquiring-tokens/desktop-mobile/integrated-windows-authentication.md), ensure <xref:System.Net.Http.HttpClientHandler.UseDefaultCredentials?displayProperty=fullName> is set to `true`.

## Example implementation

```csharp
IMsalHttpClientFactory httpClientFactory = new MyHttpClientFactory();

var pca = ConfidentialClientApplication.Create("client_id") 
                                        .WithHttpClientFactory(httpClientFactory)
                                        .Build();
```

A simple implementation of `IMsalHttpClientFactory`

```csharp
public class StaticClientWithProxyFactory : IMsalHttpClientFactory
{
    private static readonly HttpClient s_httpClient;

    static StaticClientWithProxyFactory()
    {
        var webProxy = new WebProxy(
            new Uri("http://my.proxy"),
            BypassOnLocal: false);

        webProxy.Credentials = new NetworkCredential("user", "pass");

        var proxyHttpClientHandler = new HttpClientHandler
        {
            Proxy = webProxy,
            UseProxy = true,
        };

        s_httpClient = new HttpClient(proxyHttpClientHandler);
        
    }

    public HttpClient GetHttpClient()
    {
        return s_httpClient;
    }
}
```


## HttpClient and Xamarin iOS

When using Xamarin iOS, it is recommended to create an `HttpClient` that explicitly uses the `NSURLSession`-based handler for iOS 7 and newer. MSAL.NET automatically creates an `HttpClient` that uses `NSURLSessionHandler` for iOS 7 and newer. For more information, read the [Xamarin iOS documentation for HttpClient](/xamarin/cross-platform/macios/http-stack).

## Troubleshooting

**Problem**: On a desktop application, the authorization experience do not use the HttpClient I defined

**Solution**:

On desktop and mobile apps, MSAL opens a browser and navigates to the authorization URL. It does not use HttpClient
When using the embedded browser, you can control the proxy for it by following the technique at:  https://blogs.msdn.microsoft.com/jpsanders/2011/04/26/how-to-set-the-proxy-for-the-webbrowser-control-in-net/
This cannot be achived on .NET Core, where only the system browser is available. MSAL has no control over the system browser.

**Problem**: My browser can connect to the proxy, but I get HTTP 407 errors from MSAL

**Solution**: HTTP 407 shows a proxy authentication issue. .NET framework uses the proxy settings from IE, which by default does not include the "UseDefaultCredential" setting. Some users have reported fixing this issue by adding the following to their .config file: 

```xml
<system.net>
        <defaultProxy enabled="true" useDefaultCredentials="true" />  
</system.net>
```
