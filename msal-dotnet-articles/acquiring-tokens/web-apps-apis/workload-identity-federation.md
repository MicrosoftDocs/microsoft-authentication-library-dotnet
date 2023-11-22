---
title: Acquiring a token with federated workload identity
description: "How to acquire tokens with federated workload identity in MSAL.NET"
---

# Workload identity federation

[Workload identity federation](/entra/workload-id/workload-identity-federation) allows you to access Microsoft Entra protected resources without needing to manage client application secrets. First, set up the workload identity federation in the app registration. In the application code, create a function which will fetch the tokens from the external provider, then pass it into <xref:Microsoft.Identity.Client.ConfidentialClientApplicationBuilder.WithClientAssertion(System.Func{Microsoft.Identity.Client.AssertionRequestOptions,System.Threading.Tasks.Task{System.String}})>. For each token request, MSAL will call this function to get an external token with which to acquire the Microsoft Entra tokens. Make sure this function caches the token to avoid making too many calls to the external provider.

```csharp
using Microsoft.Identity.Client;

var app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientAssertion((AssertionRequestOptions options) => FetchExternalTokenAsync())
            .Build()

var result = await app
            .AcquireTokenForClient(scope).ExecuteAsync();

public async Task<string> FetchExternalTokenAsync() 
{
    // logic to get token from cache or other sources, like GitHub, Kubernetes, etc.
     return token;
}

```

[Microsoft.Identity.Web.Certificateless](https://www.nuget.org/packages/Microsoft.Identity.Web.Certificateless) package provides some helper methods to acquire federated tokens. Use <xref:Microsoft.Identity.Web.ManagedIdentityClientAssertion> for managed identity federation.

```csharp
using Microsoft.Identity.Web;

// Reuse this instance so that the assertion is cached and only refreshed once it expires.
ManagedIdentityClientAssertion managedIdentityClientAssertion = new ManagedIdentityClientAssertion(userAssignedId);

public async Task<string> FetchExternalTokenAsync() 
{
    return await managedIdentityClientAssertion.GetSignedAssertion(default);
}

```

To acquire a federated token in a Azure Kubernetes cluster, use <xref:Microsoft.Identity.Web.AzureIdentityForKubernetesClientAssertion>.

```csharp
using Microsoft.Identity.Web;

// Reuse this instance so that the assertion is cached and only refreshed once it expires.
AzureIdentityForKubernetesClientAssertion aksClientAssertion = new AzureIdentityForKubernetesClientAssertion();

public async Task<string> FetchExternalTokenAsync() 
{
    return await aksClientAssertion.GetSignedAssertion(default);
}

```
