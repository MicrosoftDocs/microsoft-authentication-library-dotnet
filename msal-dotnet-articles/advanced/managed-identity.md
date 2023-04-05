---
title: Managed identity with MSAL.NET
description: "How to use Azure managed identities in MSAL.NET applications."
---

# Managed identity with MSAL.NET

>[!NOTE]
>This feature is experimental and available from [MSAL.NET](https://www.nuget.org/packages/Microsoft.Identity.Client/) version 4.51.0.

A common challenge for developers is the management of secrets, credentials, certificates, and keys used to secure communication between services. [Managed identities](/azure/active-directory/managed-identities-azure-resources/overview) in Azure eliminate the need for developers to manage these credentials manually. MSAL.NET supports acquiring tokens through the managed identity capability when used with applications running inside Azure infrastructure, such as:

* [Azure App Service](https://azure.microsoft.com/products/app-service/) (API version `2019-08-01` and above)
* [Azure VMs](https://azure.microsoft.com/free/virtual-machines/)
* [Azure Arc](/azure/azure-arc/overview)
* [Azure Cloud Shell](/azure/cloud-shell/overview)
* [Azure Service Fabric](/azure/service-fabric/service-fabric-overview)

For a complete list, refer to [Azure services that can use managed identities to access other services](/azure/active-directory/managed-identities-azure-resources/managed-identities-status).

## How to use managed identities

There are two types of managed identities available to developers - **system-assigned** and **user-assigned**. You can learn more about the differences in the [Managed identity types](/azure/active-directory/managed-identities-azure-resources/overview#managed-identity-types) article. MSAL.NET supports acquiring tokens with both. [MSAL.NET logging](/azure/active-directory/develop/msal-logging-dotnet) allows to keep track of requests and related metadata.

Prior to using managed identities from MSAL.NET, developers must enable them for the resources they want to use through Azure CLI or the Azure Portal.

## Examples

For both user-assigned and system-assigned identities, developers can use the <xref:Microsoft.Identity.Client.ManagedIdentityApplicationBuilder> class. Because the feature is experimental, using <xref:Microsoft.Identity.Client.BaseAbstractApplicationBuilder%601.WithExperimentalFeatures(System.Boolean)> is required.

### System-assigned managed identities

For system-assigned managed identities, the developer does not need to pass any additional information when creating an instance of <xref:Microsoft.Identity.Client.IManagedIdentityApplication>, as it will automatically infer the relevant metadata about the assigned identity.

<xref:Microsoft.Identity.Client.IManagedIdentityApplication.AcquireTokenForManagedIdentity(System.String)> is called with the resource to acquire a token for, such as `https://management.azure.com`.

```csharp
IManagedIdentityApplication mi = ManagedIdentityApplicationBuilder.Create()
    .WithExperimentalFeatures()
    .Build();

AuthenticationResult result = await mi.AcquireTokenForManagedIdentity(resource)
    .ExecuteAsync()
    .ConfigureAwait(false);
```

### User-assigned managed identities

For user-assigned managed identities, the developer needs to pass the managed identity client ID or the full resource identifier string if client ID is not available when creating <xref:Microsoft.Identity.Client.IManagedIdentityApplication>.

Like in the case for system-assigned managed identities, <xref:Microsoft.Identity.Client.IManagedIdentityApplication.AcquireTokenForManagedIdentity(System.String)> is called with the resource to acquire a token for, such as `https://management.azure.com`.

```csharp
IManagedIdentityApplication mi = ManagedIdentityApplicationBuilder.Create(userAssignedId)
    .WithExperimentalFeatures()
    .Build();

AuthenticationResult result = await mi.AcquireTokenForManagedIdentity(resource)
    .ExecuteAsync()
    .ConfigureAwait(false);
```

## Caching

By default, MSAL.NET supports in-memory caching. To explore additional caching options or implement a custom cache, see [Token cache serialization in MSAL.NET](/azure/active-directory/develop/msal-net-token-cache-serialization). We do not recommend sharing a cache between two or more Azure sources with managed identities enabled as this can result in the same token being shared across the resources.

## Troubleshooting

For failed requests the error response contains a correlation ID that can be used for further diagnostics and log analysis. Keep in mind that the correlation IDs generated in MSAL.NET or passed into MSAL are different than the one returned in server error responses, as MSAL.NET cannot pass the correlation ID to managed identity token acquisition endpoints.

### Potential errors

#### `MsalServiceException` Error Code: `managed_identity_failed_response` Error Message: An unexpected error occurred while fetching the AAD token

This exception might mean that the resource you are trying to acquire a token for is either not supported or is provided using the wrong resource ID format. Examples of correct resource ID formats include `https://management.azure.com/.default`, `https://management.azure.com`, and `https://graph.microsoft.com`.

#### `System.Net.Http.HttpRequestException`: A socket operation was attempted to an unreachable network

This exception might mean that you are likely using a resource where MSAL.NET does not support acquiring token for managed identity or you are running the sample code from a development machine where the endpoint to acquire the token for managed identities is unreachable.
