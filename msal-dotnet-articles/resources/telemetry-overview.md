---
title: MSAL.NET telemetry overview
description: Explore MSAL.NET's telemetry capabilities for Microsoft Entra token endpoint requests. Learn about client-side state, error tracking, and SDK API usage metadata.
author: Dickson-Mwendia
manager: 
ms.author: dmwendia
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: reference
ms.custom:
#Customer intent: 
---

# MSAL.NET telemetry overview

MSAL.NET sends basic telemetry about the client side state on requests to the Microsoft Entra token endpoint. Telemetry data will be logged by Microsoft Entra ID. This telemetry will give us visibility into both first and third party app health without introducing an additional telemetry pipeline dependency into the open source SDK.

MSAL.NET collects this telemetry to proactively detect server side failures or library regressions in order to provide a better service.

Basic library telemetry includes:

* Client side state at the time of the request. It shows the reason for the request execution, for example client app requested prompt, no cached tokens, expired access, or others.
* Errors for preceding requests that failed.
* SDK API usage metadata, such as which API and parameters were used for the request.

>[!IMPORTANT]
>For details on how personally identifiable information (PII) or organizational identifiable information (OII) is handled, refer to [Handling of personally-identifiable information in MSAL.NET](handling-pii.md).

## Data

MSAL requests to the token endpoint will have 2 additional headers:

* Current request header: `x-client-current-telemetry`
  * Current request will contain information about the current public API request.
* Last request header: `x-client-last-telemetry`
  * Last request contains information about failures for any previous requests.

Current request and last request are appended to calls to the token endpoint.

### Current request example

Current requests are used in telemetry to help proactively detect server side issues or library regressions with as little impact to the customer as possible. An example of the current request header format is found [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/3d9cb46d824820a580b7f826a71ecd5beb8131a8/src/client/Microsoft.Identity.Client/TelemetryCore/Http/HttpTelemetryManager.cs#L108).

### Last request example

Failed requests are used in telemetry to help proactively detect server side issues or library regressions with as little impact to the customer as possible. An example of the last request header format is found [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/3d9cb46d824820a580b7f826a71ecd5beb8131a8/src/client/Microsoft.Identity.Client/TelemetryCore/Http/HttpTelemetryManager.cs#L51).
