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
