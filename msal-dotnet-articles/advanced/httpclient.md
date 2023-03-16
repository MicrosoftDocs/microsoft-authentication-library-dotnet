---
title: Providing your own HttpClient, supporting HTTP proxies, and customization of user agent headers
---

# Providing your own HttpClient, supporting HTTP proxies, and customization of user agent headers

We understand that there are cases where you want fine grained control on the Http proxy for instance, which we had not been able to provide you at all (on .NET core), or in a limited way (.NET framework). Also, ASP.NET Core has some very efficient ways of pooling the `HttpClient` instance, and MSAL.NET clearly did not benefit from it (for details see [Use HttpClientFactory to implement resilient HTTP requests](/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests))

```csharp
IMsalHttpClientFactory httpClientFactory = new MyHttpClientFactory();

var pca = PublicClientApplicationBuilder.Create(MsalTestConstants.ClientId) 
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

## Interactive Auth with proxy

When you call `.AcquireTokenInteractive`, MSAL pops up a browser and instructs it to navigate to the authorization uri. MSAL does not call the /authorize endpoint on its own, so any HttpClient configuration you've made is not taken into account. However, for all the other required calls, MSAL uses HttpClient.

## Dispose guarantee

MSAL will **not** call Dispose() on the HttpClient. The thinking behind this based on https://stackoverflow.com/questions/15705092/do-httpclient-and-httpclienthandler-have-to-be-disposed

## .NET Core IHttpClientFactory

It is recommended to adapt [ASP.NET Core's IHttpClientFactory](/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0) to improve scalability in Web App / Web Api scenarios.

## Proxy Troubleshooting

**Problem**: I need to use a proxy different from the IE proxy

**Solution**: On .NET classic, by default, MSAL uses the `System.Windows.Forms.WebBrowser` control show UI. You can control the proxy for it by following the technique at:  https://blogs.msdn.microsoft.com/jpsanders/2011/04/26/how-to-set-the-proxy-for-the-webbrowser-control-in-net/
This cannot be achived on .NET Core, where only the system browser is available. MSAL has no control over the system browser.

**Problem**: My browser can connect to the proxy, but I get HTTP 407 errors from MSAL

**Solution**: HTTP 407 shows a proxy authentication issue. .NET framework uses the proxy settings from IE, which by default does not include the "useDefaultCredential" setting. Some users have reported fixing this issue by adding the following to their .config file: 

```xml
<system.net>
        <defaultProxy enabled="true" useDefaultCredentials="true" />  
</system.net>
```

## Additional Information

* Prior to the changes needed in order to make MSAL's httpClients thread safe (https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/pull/2046/files), the httpClient had the possibility to throw an exception stating "Properties can only be modified before sending the first request". MSAL's httpClient will no longer throw this exception after 4.19.0 (https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/releases/tag/4.19.0)