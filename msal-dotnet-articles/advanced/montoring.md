MSAL exposes token acquisition metrics as part of [AuthenticationResult.AuthenticationResultMetadata](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/src/client/Microsoft.Identity.Client/AuthenticationResultMetadata.cs#L9) object. 

## DurationTotalInMs

**Meaning**: Total time spent in MSAL to acquire a token, including network calls and cache operations.

**Suggestion**: Alarm on overall high latency (> 1 seconds). Note that the first ever token acquisition usually makes an extra HTTP call.

## DurationInCacheInMs

**Meaning**: Time spent loading or saving the token cache, which is customized by the app developer (for example, save to Redis).
**Suggestion**: Alarm on spikes.

Note: To understand how to customize token caching, see https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-net-token-cache-serialization?tabs=aspnet

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