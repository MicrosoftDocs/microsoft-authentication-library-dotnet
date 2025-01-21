---
title: MSAL.NET telemetry overview
description: Explore MSAL.NET's telemetry capabilities for Microsoft Entra token endpoint requests. Learn about client-side state, error tracking, and SDK API usage metadata.
---

# MSAL.NET telemetry overview

MSAL.NET relies on 2 strategies for telemetry: 

1. It uses Open Telemetry to emit metrics. These need to be collected by apps explicitly.
2. It sends some datapoints with every request to the token endpoint. This is for Microsoft Entra consumption only and it happens automatically.

## Open Telemetry 

The SDK uses the metric name `MicrosoftIdentityClient_Common_Meter`.  As of version 4.67.0, it emits: 

- A counter named `MsalSuccess` which contains: the SDK version, an identifier of the SDK which is different on .NET Framework and .NET, an ID of the main API used, the token source (cache or identity provider), the reason for the cache refresh in case of cache miss, details about the cache in case of cache hit and the token type. See [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/main//src/client/Microsoft.Identity.Client/Platforms/Features/OpenTelemetry/OtelInstrumentation.cs#L20) for more details.
- A counter named `MsalFailure` which contains: the SDK version, the SDK identifier, the error code, the API used, the cache refresh reason and the token type.
- Histograms that measure latency for requests through MSAL. For example, time spent performing HTTP requests, cache calls and time spent forming complex tokens like PoP tokens.

## Programatic Access

You can also rely on `AuthenticationResult.AuthenticationResultMetadata` property bag to access these datapoint programatically, without OpenTelemetry.

## Automatically collected data

MSAL.NET sends basic telemetry about the client side state on requests to the Microsoft Entra token endpoint. Telemetry data is collected by Microsoft Entra ID, alongside the usual Entra ID telemetry associated with each request.

The datapoints include:

* the AcquireToken* API used
* the reason why a token request is made (e.g. the cached token has expired)
* details about the Azure region
* the type of token requested (e.g. Bearer, PoP)

>[!IMPORTANT]
>For details on how personally identifiable information (PII) or organizational identifiable information (OII) is handled, refer to [Handling of personally-identifiable information in MSAL.NET](handling-pii.md).
