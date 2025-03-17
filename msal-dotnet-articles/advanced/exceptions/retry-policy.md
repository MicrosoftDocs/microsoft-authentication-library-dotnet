---
title: Retry policies
description: Learn how to implement a custom retry policy for token acquisition operations in .NET with MSAL. Increase your service availability with our detailed guide.
ms.date: 03/17/2025
---

# Retry policies

This document explains how to implement a custom retry policy around token acquisition operations. For other tips on how to increase the availability of your service, see [High Availability](../high-availability.md).

## Better HTTP stack

The default HTTP stack in .NET is not great. See [HttpClient tips](../httpclient.md) for details.

## MSAL implements a simple "retry-once" for errors with HTTP error codes 5xx

MSAL.NET implements a simple retry-once mechanism for errors with HTTP error codes 500-600. For more control / availability, consider disabling the default retry policy and define your own.

Note that Microsoft Entra ID may return a Retry-After header indicating to clients to pause for a few seconds, which needs to be taken into account.

## Example Retry policy

```csharp
public async Task<AuthenticationResult> GetTokenAsync()
{
        var app = ConfidentialClientApplicationBuilder.Create(ClientId)
                                                    .WithClientSecret(ClientSecret)
                                                    .WithHttpClientFactory(
        // consider using a higly scalable HttpClient, the default one is not great.
        // See /dotnet/fundamentals/networking/http/httpclient-guidelines#recommended-use 
        httpClientFactory: null,

        // Disable MSAL's internal simple retry policy
        retryOnceOn5xx: false)
        .Build();
    
    // For caching see /azure/active-directory/develop/msal-net-token-cache-serialization?tabs=aspnet#in-memory-token-cache-1
    app.AddInMemoryCache(); 

    AsyncRetryPolicy retryPolicy = GetMsalRetryPolicy();

    AuthenticationResult result = await retryPolicy.ExecuteAsync(
             () => app.AcquireTokenForClient(TestConstants.s_scope.ToArray()).ExecuteAsync())
        .ConfigureAwait(false);

    return result;
}

private static AsyncRetryPolicy GetMsalRetryPolicy()
{
    // Retry policy at token request level
    TimeSpan retryAfter = TimeSpan.Zero;

    var retryPolicy = Policy.Handle<Exception>(ex =>
    {
        return IsMsalRetryableException(ex, out retryAfter);
    }).WaitAndRetryAsync(new[] // simple retry 0s, 3s, 5s + and "retry after" hint from the server
    {
            retryAfter, // could be 0 
            retryAfter + TimeSpan.FromSeconds(2),
            retryAfter + TimeSpan.FromSeconds(5),
        },
    onRetry: (ex, ts) =>
        // Do some logging
        Debug.WriteLine($"MSAL call failed. Trying again after {ts}. Exception was {ex}"));

    return retryPolicy;
}

/// <summary>
///  Retry any MsalException marked as retryable - see IsRetryable property and HttpRequestException
///  If Retry-After header is present, return the value.
/// </summary>
/// <remarks>
/// In MSAL 4.47.2 IsRetryable includes HTTP 408, 429 and 5xx Azure AD errors but may be expanded to transient Azure AD errors in the future. 
/// </remarks>
private static bool IsMsalRetryableException(Exception ex, out TimeSpan retryAfter)
{
    retryAfter = TimeSpan.Zero;

    if (ex is HttpRequestException)
        return true;

    if (ex is MsalException msalException && msalException.IsRetryable)
    {
        if (msalException is MsalServiceException msalServiceException)
        {
            retryAfter = GetRetryAfterValue(msalServiceException.Headers);
        }

        return true;
    }

    return false;
}

private static TimeSpan GetRetryAfterValue(HttpResponseHeaders headers)
{
    var date = headers?.RetryAfter?.Date;
    if (date.HasValue)
    {
        return date.Value - DateTimeOffset.Now;
    }

    var delta = headers?.RetryAfter?.Delta;
    if (delta.HasValue)
    {
        return delta.Value;
    }

    return TimeSpan.Zero;
}
```

## Retry Policy at HTTP level or at MSAL level?

The example above shows how to introduce a retry policy at MSAL level, since MSAL transforms HTTP errors 5xx into MSAL specific exceptions.
It is also possible, to use an HTTP level retry policy, which can be introduced directly via the HttpClient.

Both possibilities are valid, with a slight preference for library-level retry policy. The reason for this is that we are trying to classify more exceptions as retry-able, for example Microsoft Entra error AADSTS50087 (see [Microsoft Entra error codes](/azure/active-directory/develop/reference-aadsts-error-codes#aadsts-error-codes) for full list). Tracking [work item](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/3649).

## Fallback instead of retry

Internally, Microsoft Entra ID uses several fallback mechanisms which help applications retrieve tokens even when Microsoft Entra ID is struggling. Regional token issuers (ESTS-R) automatically fallback to the global issuer if they go down. An emergency token issuer, which does not depend on Azure infrastructure, takes over if Microsoft Entra goes down. 

Client applications do not need to do anything to benefit from these measures.

## What is the Retry-After header?

When the Security Token Service (STS) is too busy or having problems, it returns an HTTP error [429](https://developer.mozilla.org/docs/Web/HTTP/Status/429). It may also return other HTTP error codes, such as 503. Alongside the response it will add a [Retry-After header](https://developer.mozilla.org/docs/Web/HTTP/Headers/Retry-After), which indicates that the client should wait before calling again. The wait delay is in seconds, as per spec.

`MsalServiceException` surfaces `System.Net.Http.Headers.HttpResponseHeaders` as a property named `Headers`. You can therefore leverage additional information to the Error code to improve the reliability of your applications. In the case we just described, you can use the `RetryAfter` property (of type `RetryConditionHeaderValue`) and compute when to retry.
