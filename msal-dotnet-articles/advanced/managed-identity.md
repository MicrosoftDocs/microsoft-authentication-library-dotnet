# Managed identity with MSAL.NET

>[!NOTE]
>This is an experimental feature for 1st party use only.

## Managed Identity for Confidential Client

A common challenge for developers is the management of secrets, credentials, certificates, and keys used to secure communication between services. Managed identities eliminate the need for developers to manage these credentials.

## How to use managed identity in MSAL

```csharp
IConfidentialClientApplication cca = ConfidentialClientApplicationBuilder.Create(clientId)
    .WithExperimentalFeatures()
    .Build();

AuthenticationResult result = await cca.AcquireTokenForClient(scopes)
    .WithManagedIdentity(userAssignedClientOrResourceId) // userAssignedClientIdOrResourceId is optional. To be provided only in case of user assigned managed identity.
    .ExecuteAsync()
    .ConfigureAwait(false);
```

* The scopes array should contain a single scope as managed identity acquires token for a resource.
* The parameter userAssignedClientIdOrResourceId can either contain the client id of the user assigned managed identity or the resource id in case the client id is not yet available.
* For system assigned managed identity the parameter userAssignedClientIdOrResourceId need not be passed.
* For failed requests, the error response contains a correlation id that can be used for further investigation. The MSAL's correlation id generated in MSAL or passed in to MSAL is different than the one returned in server error response as MSAL cannot pass the correlation id to managed identity token acquisition endpoints.

## Supported by MSAL

MSAL supports the following sources for managed identity

Supported:

* App services
* IMDS (VMs)
* Azure Arc

In Progress:

* Cloud Shell
* Service Fabric

## Common Exceptions

### MsalServiceException Error Code: managed_identity_failed_response Error Message: An unexpected error occurred while fetching the AAD token

This exception might mean that the scope added is either not supported or is in wrong format. An example of expected scope is `https://management.azure.com/.default`