---
title: MSAL.NET extensibility points
description: Explore advanced extensibility points in MSAL.NET for scalable apps. Adapt HttpClient factories, modify token requests, inject query parameters, and more.
author: Dickson-Mwendia
manager: 
ms.author: dmwendia
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: article
ms.custom: 
#Customer intent: 
---

# MSAL.NET extensibility points

MSAL adopts the strategy of "make simple scenarios simple, make complex scenarios possible".

## Use your own HttpClient

Allows apps to adapt highly scalable HttpClient factories such as ASP.NET Core's [IHttpClientFactory](/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0).
Helps desktop and mobile apps which have to deal with complex proxy configurations.
Allows apps to fully control the HTTP messages.

Details in <xref:Microsoft.Identity.Client.IMsalHttpClientFactory>.

## Modify the /token request

Allows applications to make changes to the `/token` request, by providing access to the list of parameters and headers and to the URI where it is performed. Useful for trying out new flows which MSAL doesn't yet support.

```csharp
public string GetTokenAsync()
{
   var result = await app.AcquireTokenForClient(scope)
         .OnBeforeTokenRequest(ModifyRequestAsync)
         .ExecuteAsync();
 
    // log result.AuthenticationResultMetadata.DurationTotalInMs and other metrics

    return result.Token;
}

private static Task ModifyRequestAsync(OnBeforeTokenRequestData requestData)
{
    requestData.BodyParameters.Add("param1", "val1");
    requestData.BodyParameters.Add("param2", "val2");

    requestData.Headers.Add("header1", "hval1");
    requestData.Headers.Add("header2", "hval2");

    return Task.CompletedTask;
}
   
```

## Inject extra query parameters

Allows apps to add query (GET) parameters to applications, customizing the experience. This mainly controls the UX login experience exposed by the `/authorize` endpoint, but the parameters are sent to the `/token` endpoint request as well.

Useful to target Microsoft Entra service slices where new features or bug fixes are deployed first and to customize the UX experience with features not exposed by MSAL. Note that MSAL doesn't perform the `/authorize` request in ASP.NET / ASP.NET Core scenarios, so those calls are not affected!

Details [here](/dotnet/api/microsoft.identity.client.abstractacquiretokenparameterbuilder-1.withextraqueryparameters?view=azure-dotnet#microsoft-identity-client-abstractacquiretokenparameterbuilder-1-withextraqueryparameters(system-string))

## Desktop / Mobile Apps - ICustomWebUi

> [!CAUTION]
> **ICustomWebUi is not recommended for production use due to security risks and current service limitations, and is on a deprecation path.**
>
> This pattern introduces security risks and is not supported by Entra ID cloud services. Using native client redirect URIs (like `https://login.microsoftonline.com/common/oauth2/nativeclient`) with custom web UI implementations typically requires users to manually copy the authorization code from the URL—an anti-pattern most commonly seen with the `nativeclient` URI. This pattern will not work in most configurations and poses security risks.
>
> - **Recommended Alternatives**: 
>   - **Use [Broker authentication (WAM)](../acquiring-tokens/desktop-mobile/wam.md)** for Windows 10+ applications - provides the best security and user experience
>   - **Use embedded browser flow** as described in [Using web browsers](../acquiring-tokens/using-web-browsers.md)

While ICustomWebUi allows desktop and mobile apps to use their own browser instead of the embedded / system browsers provided by MSAL, it should only be used for testing or legacy scenarios where migration is not yet possible.

Details [here](/dotnet/api/microsoft.identity.client.extensibility.icustomwebui?view=azure-dotnet)
