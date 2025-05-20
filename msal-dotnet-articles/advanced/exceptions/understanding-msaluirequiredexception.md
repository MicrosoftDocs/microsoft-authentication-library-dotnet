---
title: Understanding MsalUiRequiredException
ms.service: msal
ms.subservice: msal-dotnet
ms.date: 05/20/2025
---

# Understanding `MsalUiRequiredException`

One of common status codes returned from MSAL.NET when calling `AcquireTokenSilent()` is `MsalError.InvalidGrantError`. This status code means that the application should call the authentication library again, but in interactive mode (AcquireTokenInteractive or AcquireTokenByDeviceCodeFlow for public client applications, and do a challenge in Web apps). This is because additional user interaction is required before authentication token can be issued.

Most of the time when `AcquireTokenSilent` fails, it is because the token cache does not have tokens matching your request. Access tokens expire in 1h, and `AcquireTokenSilent` will try to fetch a new one based on a refresh token (in OAuth2 terms, this is the "Refresh Token' flow). This flow can also fail for various reasons, for example if a tenant admin configures more stringent login policies. 

The interaction aims at having the user do an action. Some of those conditions are easy for users to resolve (e.g. accept Terms of Use with a single click), and some cannot be resolved with the current configuration (e.g. the machine in question needs to connect to a specific corporate network). Some help the user setting-up Multi-factor authentication, or install Microsoft Authenticator on their device.

## `MsalUiRequiredException` Classification

MSAL exposes a `Classification` field, which you can read to provide a better user experience, for example to tell the user that his password expired or that he will need to provide consent to use some resources. The supported values are part of the `UiRequiredExceptionClassification` enum:

| Classification    | Meaning           | Recommended handling |
|-------------------|-------------------|----------------------|
| BasicAction | Condition can be resolved by user interaction during the interactive authentication flow. | Call AcquireTokenInteractively(). |
| AdditionalAction | Condition can be resolved by additional remedial interaction with the system, outside of the interactive authentication flow. | Call AcquireTokenInteractively() to show a message that explains the remedial action. Calling application may choose to hide flows that require additional_action if the user is unlikely to complete the remedial action. |
| MessageOnly      | Condition cannot be resolved at this time. Launching interactive authentication flow will show a message explaining the condition. | Call AcquireTokenInteractively() to show a message that explains the condition. AcquireTokenInteractively() will return UserCanceled error after the user reads the message and closes the window. Calling application may choose to hide flows that result in message_only if the user is unlikely to benefit from the message.|
| ConsentRequired  | User consent is missing, or has been revoked. | Call AcquireTokenInteractively() for user to give consent. |
| UserPasswordExpired | User's password has expired. | Call AcquireTokenInteractively() so that user can reset their password. |
| PromptNeverFailed| Interactive Authentication was called with the parameter prompt=never, forcing MSAL to rely on browser cookies and not to display the browser. This has failed. | Call AcquireTokenInteractively() without Prompt.None |
| AcquireTokenSilentFailed | MSAL SDK does not have enough information to fetch a token from the cache. This can be because no tokens are in the cache or an account was not found. The error message has more details.  | Call AcquireTokenInteractively(). |
| None    | No further details are provided. Condition may be resolved by user interaction during the interactive authentication flow. | Call AcquireTokenInteractively(). |

## Code Example

```csharp
AuthenticationResult res;
try
{
 res = await application.AcquireTokenSilent(scopes, account)
        .ExecuteAsync();
}
catch (MsalUiRequiredException ex) when (ex.ErrorCode == MsalError.InvalidGrantError)
{
 switch (ex.Classification)
 {
  case UiRequiredExceptionClassification.None:
   break;
  case UiRequiredExceptionClassification.MessageOnly:
  // You might want to call AcquireTokenInteractive(). Azure AD will show a message
  // that explains the condition. AcquireTokenInteractively() will return UserCanceled error
  // after the user reads the message and closes the window. The calling application may choose
  // to hide features or data that result in message_only if the user is unlikely to benefit 
  // from the message
  try
  {
   res = await application.AcquireTokenInteractive(scopes)
                          .ExecuteAsync();
  }
  catch (MsalClientException ex2) when (ex2.ErrorCode == MsalError.AuthenticationCanceledError)
  {
   // Do nothing. The user has seen the message
  }
  break;

  case UiRequiredExceptionClassification.BasicAction:
  // Call AcquireTokenInteractive() so that the user can, for instance accept terms
  // and conditions

  case UiRequiredExceptionClassification.AdditionalAction:
  // You might want to call AcquireTokenInteractive() to show a message that explains the remedial action. 
  // The calling application may choose to hide flows that require additional_action if the user 
  // is unlikely to complete the remedial action (even if this means a degraded experience)

  case UiRequiredExceptionClassification.ConsentRequired:
  // Call AcquireTokenInteractive() for user to give consent.
  
  case UiRequiredExceptionClassification.UserPasswordExpired:
  // Call AcquireTokenInteractive() so that user can reset their password
  
  case UiRequiredExceptionClassification.PromptNeverFailed:
  // You used WithPrompt(Prompt.Never) and this failed
  
  case UiRequiredExceptionClassification.AcquireTokenSilentFailed:
  default:
  // May be resolved by user interaction during the interactive authentication flow.
  res = await application.AcquireTokenInteractive(scopes)
                         .ExecuteAsync(); break;
 }
}
```

For full details see Issue [#1148](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1148).
