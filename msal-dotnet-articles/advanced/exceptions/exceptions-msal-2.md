> This page is for MSAL 2.x
> 
> If you are interested in MSAL 3.x, please see [exceptions](exceptions)

### Exceptions in MSAL.NET

Exceptions in MSAL.NET are intended for app developers to troubleshoot and not for displaying to end-users. Exception messages are not localized. 

When showing exceptions to the user, you can use the exception type and the ErrorCode to distinguish between exceptions. You will find most error codes are constants of CoreErrorMessages or MsalErrorMessage. Click [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/reference-aadsts-error-codes) for a list of known service exceptions.

#### MsalUiRequiredException

The "Ui Required" is proposed as a specialization of ``MsalServiceException`` named ``MsalUiRequiredException``. This means you have attempted to use a non-interactive method of acquiring a token (e.g. AcquireTokenSilent), but MSAL could not do it silently. 

### Exception types

![image](https://user-images.githubusercontent.com/13203188/37082863-968a8892-21e5-11e8-981a-2b1000f45da4.png)

### Handling Claim challenge exceptions in MSAL.NET

In some cases, when the Azure AD tenant admin has enabled conditional access policies, your application will need to handle claim challenge exceptions. This will appear as an `MsalServiceException` which `Claims` property won't be empty. For instance if the conditional access policy is to have a managed device (Intune) the error will be something like `AADSTS53000: Your device is required to be managed to access this resource` or something similar.

To handle the claim challenge, you will need you need to use one of the overrides of acquire token on the client, that accepts extra query parameters and encode the claims in this extra query parameters:
- The Claims are already surfaced in the `MsalServiceException` (let's assume here that this exception was caught in the `msalServiceException` variable) 
- Almost all the `AcquireTokenAsync` overrides in MSAL.NAET have an `extraQueryParameters` argument. 
- The way to go today is to add `"&claims={msalServiceException.Claims}”`  to the current `extraQueryParameters`.
 

