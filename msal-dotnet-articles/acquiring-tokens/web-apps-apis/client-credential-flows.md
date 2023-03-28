---
title: Client credential flows in MSAL.NET
description: "MSAL is a multi-framework library. Confidential Client flows are not available on mobile platforms (UWP, Xamarin.iOS, and Xamarin.Android) since there is no secure way of deploying a secret there."
---

# Client credential flows in MSAL.NET

## Availability by platform

MSAL is a multi-framework library. Confidential Client flows are not available on mobile platforms (UWP, Xamarin.iOS, and Xamarin.Android) since there is no secure way of deploying a secret there.

## Credentials

MSAL.NET supports two types of client credentials, which must be registered in the Azure AD app registration portal

- Application secrets (not recommended for production scenarios)
- Certificates

For advanced scenarios, 2 more types of credentials can be used. See details at [Confidential client assertions](/azure/active-directory/develop/msal-net-client-assertions).

- Signed client assertions
- Certificate + additional claims to be sent

### Code snippet

```csharp

// this object will cache tokens in-memory - keep it as a singleton
var singletonApp = ConfidentialClientApplicationBuilder.Create(config.ClientId)
           // don't specify authority here, we'll do it on the request 
           .WithCertificate(certificate) // or .WithSecret(secret)
           .Build();

// If instead you need to re-create the ConfidentialClientApplication on each request, you MUST customize 
// the cache serialization (see below)

// when making the request, specify the tenanted authority
var authResult = await app.AcquireTokenForClient(scopes: new [] {  "some_app_id_uri/.default"})        // uses the token cache automatically, which is optimized for multi-tenant access
                   .WithAuthority(AzureCloudInstance.AzurePublic, "{tenantID}")  // do not use "common" or "organizations"!
                   .ExecuteAsync();

```

**Important: do not use `common` or `organizations` authority for client credential flows.**

For more information, see [AuthenticationConfig.cs](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/blob/5199032b352a912e7cc0fce143f81664ba1a8c26/daemon-console/AuthenticationConfig.cs#L67-L87)

Note: Token cache performance was significantly improved in MSAL 4.30.0.

## Custom Cache Serialization

If your service is multi-tenant (i.e. it needs tokens for a resource that is in different tenants), see [MSAL for client credential flow in multi-tenant services](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Multi-tenant-client_credential-use).

You can serialize the token cache to a location of your choice for example in-memory or in distributed location like Redis. You would do this to:

- share the token cache between several instances of `ConfidentialClientApplication` OR
- persist the token cache to Redis to share it between different machines

Please see [distributed cache implementations](https://github.com/AzureAD/microsoft-identity-web/tree/master/src/Microsoft.Identity.Web.TokenCache/Distributed) and [binding the token cache](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization#token-cache-for-a-daemon-app).

This [sample](https://github.com/Azure-Samples/active-directory-dotnet-v1-to-v2/blob/b48c10180665260a1aec78a9acf7d1b1ff97e5ba/ConfidentialClientTokenCache/Program.cs) shows token cache serialization.

## High Availability

**Problem:**
My service is running out of memory.

**Solution:**
See [MSAL for client credential flow in multi-tenant services](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Multi-tenant-client_credential-use).
Provision enough RAM on the machines running your service or use a distributed cache.
A single token is a only a few KB in size, but there is 1 token for each tenant! A multi-tenant service sometimes needs tokens for 0.5M tenants.

**Problem:** How can I avoid requesting new tokens on each machine of my distributed service?
**Solution:** Use a distributed cache like Redis.

**Problem:** I customized my cache. How can I monitor the hit rate?
**Solution:** The result object will tell you if the token comes from the cache or not:

```csharp
authResult.AuthenticationResultMetadata.TokenSource == TokenSource.Cache
```

**Problem:** I am getting "loop detected" errors
**Solution:** You are calling Azure AD for a token to often and Azure ADis throttling you. You need to use a cache - either the in-memory one (as per the sample above) or a persisted one.

**Problem:** `AcquireTokenClient` latency is too high
**Possible Solutions:** Please ensure you have a high token cache hit rate.

The in-memory cache is optimized for searching through tokens that come from different client_id or different tenant_id. It is not optimized for storing tokens with different scopes. You need to use a different cache key that includes the scope. See [Performance testing](../../advanced/performance-testing.md).

## Registration of application secret or certificate with Azure AD

You can register your application secrets either through the interactive experience in the [Azure portal](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredAppsPreview), or using command-line tools (like PowerShell)

### Registering client secrets using the application registration portal

The management of client credentials happens in the **certificates & secrets** page for an application:

![image](../../media/azure-ad-certificates.png)

### Registering client secrets using PowerShell

The [active-directory-dotnetcore-daemon-v2](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2) sample shows how to register an application secret or a certificate with an Azure AD application:

- For details on how to register an application secret, see [AppCreationScripts/Configure.ps1](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/blob/5199032b352a912e7cc0fce143f81664ba1a8c26/AppCreationScripts/Configure.ps1#L190)
- For details on how to register a certificate with the application, see [AppCreationScripts-withCert/Configure.ps1](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/blob/5199032b352a912e7cc0fce143f81664ba1a8c26/AppCreationScripts-withCert/Configure.ps1#L162-L178)

## Construction of ConfidentialClientApplication with client credentials

In MSAL.NET client credentials are passed as a parameter at the application construction

Then, once the confidential client application is constructed, acquiring the token is a question of calling overrides of ``AcquireTokenForClient``, passing the scope, and forcing or not a refresh of the token.

## Client assertions

Instead of a client secret or a certificate, the confidential client application can also prove its identity using client assertions. This advanced scenario is detailed in [Confidential client assertions](/azure/active-directory/develop/msal-net-client-assertions).

## Remarks

### AcquireTokenForClient uses the application token cache

`AcquireTokenForClient` uses the **application token cache** (not the user token cache)
Don't call `AcquireTokenSilent` before calling `AcquireTokenForClient` as `AcquireTokenSilent` uses the **user** token cache. `AcquireTokenForClient` checks the **application** token cache itself and updates it.

### Scopes to request

The scope to request for a client credential flow is the name of the resource followed by `/.default`. This notation tells Azure AD to use the **application level permissions** declared statically during the application registration. Also these API permissions must be granted by a tenant administrator

```csharp
ResourceId = "someAppIDURI";
var scopes = new [] {  ResourceId+"/.default"};

var result = app.AcquireTokenForClient(scopes);
```

### No need to pass a Reply URL at app construction if your app is only a daemon

In the case where your confidential client application uses **only** client credentials flow, you don't need to pass a reply URL passed in the constructor.

## Samples illustrating acquiring tokens interactively with MSAL.NET

Sample | Platform | Description
------ | -------- | -----------
[active-directory-dotnetcore-daemon-v2](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2) | .NET Core 2.1 Console | <p>A simple .NET Core application that displays the users of a tenant querying the Microsoft Graph using the identity of the application, instead of on behalf of a user.</p> ![Daemon app topology](../../media/daemon-app-topology.png) <p>The sample also illustrates the variation with certificates.</p> ![Daemon certificate-based auth topology](../../media/daemon-certificate-topology.png)
[active-directory-dotnet-daemon-v2](https://github.com/Azure-Samples/active-directory-dotnet-daemon-v2) | ASP.NET MVC | <p>A web application that sync's data from the Microsoft Graph using the identity of the application, instead of on behalf of a user.</p>![UserSync app topology](../../media/user-sync-app-topology.png)

## More info

You can find more information in:

- The protocol documentation: [Azure Active Directory v2.0 and the OAuth 2.0 client credentials flow](/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)

See [Client credentials in MSAL.NET](./client-credential-flows.md).
