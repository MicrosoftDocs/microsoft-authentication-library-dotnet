# Monitoring of applications using MSAL.NET

In order to ensure authentication services using MSAL.NET are running correctly, MSAL provides a number of ways to monitor its behavior so that issues can be identified and addressed before they occur in production. The incorrect use of MSAL (lifecycle and token cache) does not lead to immediate failures. But sometimes, they will bubble up under high traffic scenarios after the app is in production for a period of time. For example, if only one instance of confidential client application is used and MSAL is not configured serialize the token cache, the cache will grow forever. Another issue can arise when creating a new confidential client application and not utilizing the cache which will lead to various issues such as throttling from the identity provider. For recommendations on how to utilize MSAL appropriately, See [High Availability](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/High-availability#add-monitoring-around-msal-operations)

## Logging

One of the tools MSAL provides to combat these issues is logging errors when MSAL in not configured correctly. It is critical to enable logging whenever possible to not only monitor logs for these monitoring errors but also help in the diagnosis of issues that may occur. See [Logging](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging).

The following errors will be logged in MSAL:

- When using an authority ending in /common or /organizations for client credential authentication. `AcquireTokenForCleint` (This will be available in 4.37.0)

  - "The current authority is targeting the /common or /organizations endpoint which is not reccomended. See https://aka.ms/msal-net-client-credentials for more details"

- When the default internal token cache is used when using confidential client applications.

  - "The default token cache provided by MSAL is not designed to be performant when used in confidential client applications. Please use token cache serialization. See https://aka.ms/msal-net-cca-token-cache-serialization."

## Metrics

In addition to logging, MSAL exposes important metrics as part of [AuthenticationResult.AuthenticationResultMetadata](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/src/client/Microsoft.Identity.Client/AuthenticationResultMetadata.cs#L9). See [Add monitoring around MSAL operations](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/High-availability#add-monitoring-around-msal-operations) for more details.

## Additional Information

MSAL exposes token acquisition metrics as part of [AuthenticationResult.AuthenticationResultMetadata](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/src/client/Microsoft.Identity.Client/AuthenticationResultMetadata.cs#L9) object. 

## DurationTotalInMs

**Meaning**: Total time spent in MSAL to acquire a token, including network calls and cache operations.

**Suggestion**: Alarm on overall high latency (> 1 seconds). Note that the first ever token acquisition usually makes an extra HTTP call.

## DurationInCacheInMs

**Meaning**: Time spent loading or saving the token cache, which is customized by the app developer (for example, save to Redis).
**Suggestion**: Alarm on spikes.

Note: To understand how to customize token caching, see https://learn.microsoft.com/azure/active-directory/develop/msal-net-token-cache-serialization?tabs=aspnet

## DurationInHttpInMs

**Meaning**: Time spent making HTTP calls to the identity provider (AAD). 
**Suggestion**: Alarm on spikes.

## TokenSource

**Meaning**: Indicates the source of the token - typically cache or identity provider (AAD). Tokens are retrieved from the cache much faster (for example, ~100 ms versus ~700 ms). Can be used to monitor and alarm the cache hit ratio.

## CacheRefreshReason

**Meaning**: Specifies the reason for fetching the access token from the identity provider. See [Possible Values](see https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/src/client/Microsoft.Identity.Client/Cache/CacheRefreshReason.cs) . Use in conjunction with `TokenSource`.

## TokenEndpoint

**Meaning**: The actual token endpoint uri used to fetch the token. Useful to understand how MSAL resolves the tenant in silent calls and the region in regionalized calls. Note: regionalization is available only to 1P applications for now.

## RegionDetails

**Meaning**: Has details about the region used to make call, such as the region used and any auto-detection error.  Note: regionalization is available only to 1P applications for now.