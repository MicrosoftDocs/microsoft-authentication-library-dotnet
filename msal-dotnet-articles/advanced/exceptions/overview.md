## Exceptions in MSAL.NET

Exceptions in MSAL.NET are intended for app developers to troubleshoot and not for displaying to end-users. Exception messages are not localized. 

## The different types of exceptions

![image](https://user-images.githubusercontent.com/12273384/189374111-e1e788dd-89c3-4ff3-a808-fc7924f9c4a0.png)

| Exception               | Description                                                                                                                                                                                                    |
|-------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| MsalException           | Base class for MSAL exceptions.                                                                                                                                                                                |
| MsalClientException     | Errors which occur in the library itself, for example an incomplete configuration.                                                                                                                             |
| MsalServiceException    | Represents errors transmitted by the token provider (AAD). See [AAD errors](/azure/active-directory/develop/reference-aadsts-error-codes#handling-error-codes-in-your-application). Servince unavialble errors (e.g. HTTP 500), indicating a problem with the service, have the error code `service_not_available` |
| MsalUiRequiredException | Special AAD error which indicates that the user must interactively login.                                                                                                                                      |

No other exception is caught by MSAL. Any network issues, cancellations etc. are bubbled up to the application.

MSAL throws `MsalClientException` for things that go wrong inside the library (e.g. bad configuration) and `MsalServiceException` for things that go wrong service side or in the broker (e.g. a secret expired).

### Common exceptions 

1. User cancelled authentication (public client only)

When calling `AcquireTokenInteractive`, a browser or the broker is invoked to handle user interaction. If the user closes this process or if they hit the browser back button, MSAL generates an `MsalClientException` with the error code `authentication_canceled` (`MsalError.AuthenticationCanceledError`).

On Android, this exception can also occur if a [browser with tabs](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Android-system-browser#known-issues) is not available. 

2. HTTP Exceptions

Developers are expected to implement their own retry policies when calling MSAL. MSAL makes HTTP calls to the AAD service, and occasional failures can occur, for example the network can go down or the server is overloaded. HTTP 5xx status code responses are retried once. 

See also [Simple retry for errors with HTTP error codes 500-600](retry-after#simple-retry-for-errors-with-http-error-codes-500-600) and [Http 429 (Retry After)](retry-after#http-429-retry-after)

### Exception types

When processing exceptions, you can use the exception type itself and the `ErrorCode` member to distinguish between exceptions. The values of `ErrorCode` are constants of [MsalError](/dotnet/api/microsoft.identity.client.msalerror?view=azure-dotnet#fields)

You can also have a look at the fields of [MsalClientException](/dotnet/api/microsoft.identity.client.msalexception?view=azure-dotnet#fields), [MsalServiceException](/dotnet/api/microsoft.identity.client.msalserviceexception?view=azure-dotnet#fields), [MsalUIRequiredException](/dotnet/api/microsoft.identity.client.msaluirequiredexception?view=azure-dotnet#fields)

In the case of `MsalServiceException`, the error code might contain a code which you can find in [Authentication and authorization error codes](/azure/active-directory/develop/reference-aadsts-error-codes)

#### MsalUiRequiredException

The "Ui Required" is proposed as a specialization of ``MsalServiceException`` named ``MsalUiRequiredException``. This means you have attempted to use a non-interactive method of acquiring a token (e.g. AcquireTokenSilent), but MSAL could not do it silently. this can be because:
- you need to sign-in
- you need to consent
- you need to go through a multi-factor authentication experience.

To remediate, call an AcquireToken* method that prompts the user, for example `AcquireTokenInteractive` in public clients, redirect the user to login in websites or respond with a 401 in a web api.

### Continous Access Evaluation

See https://learn.microsoft.com/azure/active-directory/develop/app-resilience-continuous-access-evaluation?tabs=dotnet

### Handling Claim challenge exceptions in MSAL.NET

In some cases, when the Azure AD tenant admin has enabled conditional access policies, your application will need to handle claim challenge exceptions. This will appear as an `MsalServiceException` which `Claims` property won't be empty. For instance if the conditional access policy is to have a managed device (Intune) the error will be something like `AADSTS53000: Your device is required to be managed to access this resource` or something similar.

To handle the claim challenge, you will need to use the `.WithClaims(claims)` method.

### Retry policies

See [Retry-Policy](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Retry-Policy)



