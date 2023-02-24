# High availability considerations in MSAL.NET

For client credential (app 2 app) flow, please see https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-credential-flows which has a topic on High-Availablity first.

## Use the latest MSAL

Semantic versioning is followed to the letter. Use the latest MSAL to get the latest bug fixes.

You also want to check if you should use Microsoft Identity Web, a higher level library for web apps and web APIs, which does a lot of what is described below for your. See [Is MSAL right for me?](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Is-MSAL.NET-right-for-me%3F), which proposes a decision tree to choose the best solution depending on your platform and constraints.

## Use the token cache

**Default behaviour:** MSAL caches the tokens in memory. Each `ConfidentialClientApplication` instance has its own internal token cache. In-memory cache can be lost, for example, if the object instance is disposed or the whole application is stopped. 

**Recommendation:** All apps should persist their token caches. Web apps and Web APIs should use an L1 / L2 token cache where L2 is a distributed store like Redis to handle scale. Desktop apps should use [this token cache serialization strategy](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization#token-cache-for-a-public-client-application).

> Note: if you use Microsoft.Identity.Web, you don't need to worry about the cache, as it implements the right cache behavior. If you don't use Microsoft.Identity.Web but are building a web app or web API, you'd want to consider an [hybrid approach](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Is-MSAL.NET-right-for-me%3F#use-hybrid-model-msalnet-and-microsoft-identity-web)

**Default behaviour:** MSAL maintains a secondary ADAL token cache for migration scenarios between ADAL and MSAL. ADAL cache operations are very slow.
**Recommendation:** Disable ADAL cache if you are not interested in migrating from ADAL. This will make a **BIG** perf improvement - see perf measurements [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/pull/2309). 

Add `WithLegacyCacheCompatibility(false)` when constructing your app to disable ADAL caching.

## Add monitoring around MSAL operations

MSAL exposes important metrics as part of [AuthenticationResult.AuthenticationResultMetadata](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/src/client/Microsoft.Identity.Client/AuthenticationResultMetadata.cs#L9) object: 

| Metric       | Meaning     | When to trigger an alarm?    |
| :-------------: | :----------: | :-----------: |
|  `DurationTotalInMs`| Total time spent in MSAL, including network calls and cache   | Alarm on overall high latency (> 1 s). Value depends on token source. From the cache: one cache access. From AAD: two cache accesses + one HTTP call. First ever call (per-process) will take longer because of one extra HTTP call. |
|  `DurationInCacheInMs` | Time spent loading or saving the token cache, which is customized by the app developer (for example, save to Redis).| Alarm on spikes. |
|  `DurationInHttpInMs` | Time spent making HTTP calls to AAD.  | Alarm on spikes.|
|  `TokenSource` | Indicates the source of the token. Tokens are retrieved from the cache much faster (for example, ~100 ms versus ~700 ms). Can be used to monitor and alarm the cache hit ratio. | Use with `DurationTotalInMs`. |
|  `CacheRefreshReason` | Specifies the reason for fetching the access token from the identity provider. See [Possible Values](see https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/src/client/Microsoft.Identity.Client/Cache/CacheRefreshReason.cs) . | Use with `TokenSource`. |

## Logging

Listen to `Warning` and `Error` level messages coming from MSAL logs. These can be silent errors or strong recommendations to use a different config. 
It is not recommended to set `Verbose` logging in production, as it produces a lot of messages and it impacts perf.

Details about logging can be found [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging).

## Retry Policy

**Default behaviour**: MSAL will retry failed 5xx requests once.

**Recommendation**:

- See https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Retry-Policy for writing a retry policy with Polly

# One Confidential Client per session

In web app and web API scenarios, it is recommended to use a new `ConfidentialClientApplication` on each session and to serialize in the same way - one token cache per session. This scales well and also increases security. The [official samples](/azure/active-directory/develop/sample-v2-code) show how to do this.

> Note: Microsoft.Identity.Web does this.

## HttpClient 

**Default behaviour**: MSAL's creating HttpClient does not scale well for web sites / web API where we recommend to have a `ClientApplication` object for each user session.

**Recommendation**: Provide your own scalable HttpClientFactory. On .NET Core we recommend that you inject the [System.Net.Http.IHttpClientFactory](/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0). This is described in more detail [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/HttpClient) and in the [official docs](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient?view=net-7.0#net-framework--mono)

## Proactive Token renewal

### Goal

Increase application availability by issuing longer lived access tokens and ensure they are refreshed earlier than their expiration date.

### Status quo

By default, Azure AD issues access tokens with 1 hour expiration. If an Azure AD outage occurs when a token needs to be refreshed, MSAL will fail. The failure propagates to the calling application and impacts availability.

### Process

To improve availability MSAL tries to ensure than an app always has fresh unexpired tokens. AAD outages rarely take more than a few hours, so if MSAL can guarantee that a token always has at least a few hours of availability left, the application will not be impacted by the AAD outage. 

To get long lived tokens, you must configure your tenant (note: internal Microsoft tenants are already configured). For client_credentials (service 2 service), this is enough. For user credentials, you must also configure CAE - /azure/active-directory/conditional-access/concept-continuous-access-evaluation.

When Azure AD returns a long lived token, it includes a `refresh_in` field. It is generally set to half the expiration of the access token.
![image](https://user-images.githubusercontent.com/12273384/108714872-05acbd80-7512-11eb-855d-a42b6ff01b0c.png)

Note: From MSAL 4.37.0 and above, you can observe this value by inspecting the `AuthenticationResult.AuthenticationResultMetadata.RefreshOn`.

Additionally, you can configure a token lifetime of more than the default 1 hour, as described [here](/azure/active-directory/develop/active-directory-configurable-token-lifetimes.

Whenever you make **requests for the same token**, i.e. whenever MSAL is able to serve a token from its cache, then MSAL will automatically check the `refresh_in` value. If it has elapsed, MSAL will issue a token request to AAD in the background, but will return the existing, valid token to the application. In the unlikely event that the background refresh fails (e.g. AAD outage), the app is not affected.

## Certificate Rotation

Certs for the confidential client app must be rotated for security reasons (don't use secrets in prod!). There are several ways to handle cert rotation, listing is in ordered of most preferred to least preferred.

1. Use Microsoft.Identity.Web's certificate handling logic

In web app / web api scenarios, you should use Microsoft.Identity.Web, a higher-level API over MSAL. It handles certificate rotation for when the certificate is stored in KeyVault and handles Managed Identity for you as well.

https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#getting-certificates-from-key-vault

This is the preferred solution for non-Microsoft internal services using ASP.NET Core.

2. (internal only) Rely on Subject Name / Issuer certificates

This mechanism allows AAD to identify a cert based on SN/I instead of x5t. It is a stop-gap solution, there are no plans to make it available to 3p.
https://aka.ms/msal-net-sni

This is the preferred solution for Microsoft internal services.

3. Write your own simple cert reload logic

AAD will reject an expired certificate with an error, which MSAL will report as an `MsalServiceException`

```csharp
        private bool IsInvalidClientCertificateError(MsalServiceException exMsal)
        {
            return !_retryClientCertificate &&
                string.Equals(exMsal.ErrorCode, "invalid_client", StringComparison.OrdinalIgnoreCase) &&
                exMsal.Message.Contains("AADSTS700027", StringComparison.OrdinalIgnoreCase);
        }
```

At this stage your app should try to reload the KeyVault certificate.

4. Re-create the CCA object

Create a new ConfidentialClientApplication object on each request. But make sure to point it to the same token cache! /azure/active-directory/develop/msal-net-token-cache-serialization?tabs=aspnetcore
