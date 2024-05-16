---
title: MSAL.NET telemetry overview
description: Explore MSAL.NET's telemetry capabilities for Microsoft Entra token endpoint requests. Learn about client-side state, error tracking, and SDK API usage metadata.
---

# MSAL.NET telemetry overview

MSAL.NET sends basic telemetry about the client side state on requests to the Microsoft Entra token endpoint. Telemetry data will be logged by Microsoft Entra ID. This telemetry will give us visibility into both 1st and 3rd party app health without introducing an additional telemetry pipeline dependency into the open source SDK. MSAL.NET collects this telemetry to proactively detect server side failures or library regressions, in order to provide a better service.

Basic telemetry includes:

* Client side state at the time of the request - shows the reason for the request execution, e.g. client app requested prompt, no cached tokens, expired access token etc
* Errors for preceding requests that failed
* SDK API usage metadata - e.g. which API/parameters were used for the request

## Data

MSAL requests to the token endpoint will have 2 additional headers:

* Current request header: "x-client-current-telemetry"
  * Current request will contain information about the current public API request.
* Last request header: "x-client-last-telemetry"
  * Last request contains information about failures for any previous requests. 

Current request and last request are appended to calls to the token endpoint.  

### Current request example

Current requests are used in telemetry to help proactively detect server side issues or library regressions with as little impact to the customer as possible. An example of the current request header format is found [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/3d9cb46d824820a580b7f826a71ecd5beb8131a8/src/client/Microsoft.Identity.Client/TelemetryCore/Http/HttpTelemetryManager.cs#L108).

### Last request example

Failed requests are used in telemetry to help proactively detect server side issues or library regressions with as little impact to the customer as possible. An example of the last request header format is found [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/3d9cb46d824820a580b7f826a71ecd5beb8131a8/src/client/Microsoft.Identity.Client/TelemetryCore/Http/HttpTelemetryManager.cs#L51).
