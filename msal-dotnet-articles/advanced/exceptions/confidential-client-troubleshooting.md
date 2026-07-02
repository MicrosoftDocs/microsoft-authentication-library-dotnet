---
title: Troubleshooting confidential client applications
description: Diagnose and resolve common errors in confidential client apps using MSAL.NET, including throttling, socket exhaustion, OBO failures, client credential errors, and token cache misses.
author: nebharg
manager: bgavril
ms.author: nebharg
ms.date: 05/18/2026
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: troubleshooting-general
ms.custom:
#Customer intent: As a developer building confidential client applications (web apps, web APIs, daemon services), I want to diagnose and fix common token acquisition failures so that my service remains reliable.
---

# Troubleshooting confidential client applications

This guide covers common issues specific to confidential client applications (web apps, web APIs, and daemon/service apps). For general exception handling, see [Handle errors and exceptions in MSAL.NET](msal-error-handling.md).

## Throttling (HTTP 429 and AADSTS50196)

### Symptoms

- `MsalServiceException` with HTTP status code 429
- Error code `AADSTS50196` — "The server terminated an operation because it encountered a loop"
- `MsalThrottledServiceException` (MSAL 4.47.0+)

### Common causes

1. **Missing or misconfigured token cache** — Every call goes to Microsoft Entra ID instead of serving tokens from the cache.
2. **Requesting tokens in a tight loop** — For example, calling `AcquireTokenForClient` on every incoming request without checking the cache first.
3. **Too many distinct scopes/resources** — Each unique scope produces a separate cached token.

### Resolution

1. **Always call `AcquireTokenSilent` first** (for delegated flows) or ensure the token cache is configured (for client credentials). MSAL's built-in cache handles deduplication automatically when properly configured.

2. **Verify your token cache is working.** Check `AuthenticationResult.AuthenticationResultMetadata.TokenSource` — if it always shows `IdentityProvider` instead of `Cache`, your cache isn't being used.

   ```csharp
   var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

   if (result.AuthenticationResultMetadata.TokenSource == TokenSource.IdentityProvider)
   {
       // This should NOT happen on every call - investigate cache configuration
       logger.LogWarning("Token was fetched from IdP, not cache. CacheRefreshReason: {Reason}",
           result.AuthenticationResultMetadata.CacheRefreshReason);
   }
   ```

3. **Respect `Retry-After` headers.** When you receive a 429, the `MsalServiceException.Headers` property includes a `RetryAfter` value:

   ```csharp
   catch (MsalServiceException ex) when (ex.StatusCode == 429)
   {
       TimeSpan delay = ex.Headers.RetryAfter?.Delta
           ?? TimeSpan.FromSeconds(ex.Headers.RetryAfter?.Date?.Subtract(DateTimeOffset.Now).TotalSeconds ?? 60);
       await Task.Delay(delay);
   }
   ```

4. **Use a single `ConfidentialClientApplication` instance per session** (not per request). See [High availability](../high-availability.md) for guidance.

> [!TIP]
> Starting with MSAL 4.47.0, throttled responses throw `MsalThrottledServiceException` (a subclass of `MsalServiceException`) which makes it easier to distinguish throttling from other service errors.

## Network instability and socket exceptions

### Symptoms

- `HttpRequestException` (often with an inner `SocketException`) when acquiring tokens.
- Intermittent failures during calls to the Microsoft Entra token endpoint.
- High latency in MSAL operations (`AuthenticationResult.AuthenticationResultMetadata.DurationTotalInMs`).

### Common causes

1. **Not caching tokens** — Without caching, every token request results in a network call to Microsoft Entra ID. This increases exposure to transient network failures and socket exhaustion.
2. **Service outage or local network issues** — The token endpoint may be temporarily unavailable, or the local network is unstable.
3. **Custom `HttpClient` overriding MSAL's default** — MSAL's built-in `HttpClient` is designed to be scalable. If you override it, connection management becomes your responsibility.
4. **Firewall or network rules** — Recent updates to network or firewall rules may be blocking outbound traffic to `login.microsoftonline.com`.

### Solution

Enable and verify token caching. Caching reduces the number of outbound network calls, shielding your app from transient network failures.

To verify that tokens are being served from cache, check the `TokenSource` property on the authentication result:

```csharp
var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

if (result.AuthenticationResultMetadata.TokenSource == TokenSource.IdentityProvider)
{
    // Token was fetched from the network - this should only happen on the first call or after expiry
    logger.LogWarning("Token not served from cache. CacheRefreshReason: {Reason}",
        result.AuthenticationResultMetadata.CacheRefreshReason);
}
```

If `TokenSource` consistently returns `IdentityProvider` instead of `Cache`, your token cache is not configured correctly.

For web apps and web APIs, use a distributed token cache (for example, Redis). See [High availability](../high-availability.md#use-the-token-cache) for configuration guidance.

If you provide a custom `HttpClient`, ensure it's long-lived and properly manages connection pooling. See [Providing your own HttpClient](../httpclient.md).

Verify if there are any recent updates to network or firewall rules that might have caused connectivity issues to `login.microsoftonline.com` and regional endpoints.

## On-Behalf-Of (OBO) failures

### AADSTS50013 — Assertion validation failed

#### Symptoms

`MsalServiceException` with error code `AADSTS50013: Assertion failed signature validation` or `invalid_grant`.

#### Common causes

1. The incoming token (user assertion) has expired.
2. The token was issued by a different authority than expected.
3. The audience (`aud`) of the token doesn't match the app's client ID or app ID URI.

#### Resolution

- Verify that the access token passed to `.WithUserAssertion()` is fresh and intended for your API.
- Ensure your API's app registration has the correct `accessTokenAcceptedVersion` (v2 tokens use `api://{clientId}` as audience).
- Check that the authority in your `ConfidentialClientApplication` matches the token issuer's tenant.

### AADSTS65001 — Consent not granted for OBO

#### Symptoms

`MsalServiceException` with error code `AADSTS65001` when calling `AcquireTokenOnBehalfOf`.

#### Common causes

The downstream API scopes haven't been consented to by the user or admin. OBO requires that the user (or a tenant admin) has granted consent for the downstream permissions.

#### Resolution

1. Ensure the required downstream API permissions are declared in your app registration under **API permissions**.
2. For multi-tenant apps, trigger admin consent using the admin consent URL:
   ```
   https://login.microsoftonline.com/{tenant}/adminconsent?client_id={clientId}
   ```
3. For single-tenant apps, have a tenant admin grant consent through the Microsoft Entra admin center.

### OBO token too large

#### Symptoms

HTTP 431 (Request Header Fields Too Large) from downstream APIs, or `MsalServiceException` indicating the token response is too large.

#### Common causes

Users with many group memberships produce large tokens. When OBO exchanges these, the resulting token can exceed HTTP header size limits.

#### Resolution

- Configure your app registration to use **groups claims with a filter** or switch to `hasgroups` / `_claim_names` claims (which return a Graph URL instead of embedding all groups).
- Use the [groups overage pattern](/azure/active-directory/develop/id-token-claims-reference#groups-overage-claim) to query Microsoft Graph for group membership at runtime.

## Client credential errors

### AADSTS7000215 — Invalid client secret

#### Symptoms

`MsalServiceException` with `AADSTS7000215: Invalid client secret provided`.

#### Resolution

1. Verify the secret value (not the secret ID) is used in your configuration.
2. Check that the secret hasn't expired in the Microsoft Entra admin center under **Certificates & secrets**.
3. Ensure there are no trailing whitespace or encoding issues when loading the secret from configuration/Key Vault.

### AADSTS700024 — Client assertion expired

#### Symptoms

`MsalServiceException` with `AADSTS700024: Client assertion is not within its valid time range`.

#### Common causes

The certificate used to sign client assertions has expired, or the system clock is significantly skewed.

#### Resolution

1. Check certificate expiration: ensure the certificate's `NotAfter` date hasn't passed.
2. Verify system clock synchronization (NTP).
3. If using Azure Key Vault for certificates, ensure your app is loading the latest version. See [Certificate rotation](../high-availability.md#certificate-rotation) for best practices.

### AADSTS700016 — Application not found

#### Symptoms

`MsalServiceException` with `AADSTS700016: Application with identifier '{clientId}' was not found in the directory '{tenant}'`.

#### Common causes

1. Wrong `ClientId` in configuration.
2. The app registration exists in a different tenant than the authority being used.
3. For multi-tenant apps, the app hasn't been consented to in the target tenant.

#### Resolution

- Double-check the `ClientId` and `TenantId` in your configuration.
- If using `/common` or `/organizations` authority, ensure the app supports multi-tenant access.

## Token cache miss diagnosis

### Symptoms

`AuthenticationResult.AuthenticationResultMetadata.TokenSource` consistently returns `IdentityProvider` instead of `Cache`, even for repeated calls with the same parameters.

### Diagnostic steps

1. **Check `CacheRefreshReason`** on the `AuthenticationResultMetadata`:

   | Value | Meaning |
   |-------|---------|
   | `NoCachedAccessToken` | No matching token in cache — first call or cache was cleared |
   | `Expired` | Cached token expired (normal for tokens > 1 hour old) |
   | `ProactivelyRefreshed` | Token is being refreshed before expiry (normal, improves availability) |
   | `ForceRefreshOrClaims` | App explicitly called `.WithForceRefresh(true)` or passed claims |

2. **Verify cache key alignment.** Tokens are cached by: authority + client ID + scopes + (for OBO) user assertion hash. If any of these differ between calls, you get a cache miss.

3. **Check for accidental `WithForceRefresh(true)`** in your code — this bypasses the cache entirely.

4. **For distributed caches (Redis, SQL):** Ensure the serialization callbacks (`SetBeforeAccessAsync`/`SetAfterAccessAsync`) are registered and not throwing silently.

   ```csharp
   // Verify cache callbacks are firing
   app.AppTokenCache.SetBeforeAccessAsync(async args =>
   {
       logger.LogDebug("Cache read for {SuggestedKey}", args.SuggestedCacheKey);
       // Load from distributed store
   });
   ```

## Managed Identity failures

### IMDS timeout or unavailable

#### Symptoms

- `MsalServiceException` with error message indicating IMDS (Instance Metadata Service) is unreachable.
- Long delays (2+ seconds) before token acquisition fails.
- `HttpRequestException` or `TaskCanceledException` during managed identity calls.

#### Common causes

1. The application is not running in an Azure environment that supports managed identity (e.g., running locally or in an unsupported hosting environment).
2. Network Security Group (NSG) rules block access to the IMDS endpoint (`169.254.169.254`).
3. A user-assigned managed identity ID is specified but doesn't exist or isn't assigned to the resource.

#### Resolution

1. **Verify the hosting environment** supports managed identity (App Service, Azure Functions, VMs, AKS, Container Apps, etc.).
2. **For local development**, use `DefaultAzureCredential` from `Azure.Identity` which falls through to developer credentials when MI is unavailable — or use environment variables to disable MI locally.
3. **Check NSG rules** — ensure outbound access to `169.254.169.254:80` is allowed.
4. **For user-assigned MI**, verify the `ManagedIdentityId` value matches the client ID, resource ID, or object ID of an identity assigned to your Azure resource:

   ```csharp
   var miApp = ManagedIdentityApplicationBuilder
       .Create(ManagedIdentityId.WithUserAssignedClientId("your-client-id"))
       .Build();
   ```

### AADSTS70021 — No matching federated identity record

#### Symptoms

`MsalServiceException` with `AADSTS70021` when using workload identity federation with managed identity.

#### Resolution

- Verify the federated identity credential is configured correctly on the target app registration.
- Check that the `subject`, `issuer`, and `audience` values in the federated credential match what the managed identity token contains.

## Next steps

- [High availability considerations](../high-availability.md)
- [Retry policy](retry-policy.md)
- [Logging in MSAL.NET](msal-logging.md)
- [Microsoft Entra authentication error codes](/azure/active-directory/develop/reference-error-codes)
