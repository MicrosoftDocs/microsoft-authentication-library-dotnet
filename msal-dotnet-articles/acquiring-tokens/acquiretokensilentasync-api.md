---
title: Understanding the AcquireTokenAsync API
description: Learn how to acquire  tokens silently in public and confidential client applications using MSAL.NET
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 03/17/2023
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: reference
ms.custom: devx-track-csharp, aaddev
#Customer intent: 
# Customer intent: As an application developer, I want to learn how to acquire tokens silently in public and confidential client applications using MSAL.NET
---
# Understanding the `AcquireTokenAsync` API

## Tokens are cached

### Public client application

Once MSAL.NET has acquired a user token to call a Web API, it caches it. If you are building a public client application and want to acquire a token, first call `AcquireTokenSilent`, to verify if an acceptable token is in the cache, can be refreshed, or can get derived. If not, call the AcquireToken*ForFlow* method depending on the flow you are interested in.

### Confidential client application

If you are build an ASP.NET Core application, use [`Microsoft.Identity.Web`](https://github.com/AzureAD/microsoft-identity-web), which handles all these for you.

Otherwise, in confidential client applications, you should not call `AcquireTokenSilent` before:

- `AcquireTokenForClient` ([Client credentials flow](./web-apps-apis/client-credential-flows.md)), as it does not use the user token cache, but an application token cache. This method takes care of verifying this application token cache before sending a request to the STS
- `AcquireTokenByAuthorizationCode` in Web Apps, as it redeems a code that the application got by signing-in the user, and having them consent for more scopes. Since a code is passed as a parameter, and not an account, the method cannot look in the cache before redeeming the code, which requires, anyway, a call to the service.
- `AcquireTokenOnBehalfOf` in web APIs, as it uses the token coming to the web API and verifies and updates the token cache with this token as a key. `AcquireTokenSilent` doesn't have the necessary information (the incoming token).

## AcquireTokenXYZ don't get token from the cache

In public client applications, contrary to what happens in ADAL.NET, the design of MSAL.NET is such that `AcquireTokenInteractive` never looks at the cache. As an application developer, you need to call `AcquireTokenSilent` first. `AcquireTokenSilent` is capable, in many cases, of silently getting another token with more scopes, based on a token in the cache. It's also capable of refreshing a token when it's getting close to expiration (as the token cache also contains a refresh token)

### Recommended call pattern in public client applications

The recommended call pattern is to first try to call `AcquireTokenSilent`, and if it fails with a `MsalUiRequiredException`, call `AcquireTokenXYZ`.

### Recommended call pattern in public client applications with MSAL.NET 4.x

```csharp
AuthenticationResult result = null;
var accounts = await app.GetAccountsAsync();

try
{
 result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
        .ExecuteAsync();
}
catch (MsalUiRequiredException ex)
{
 // A MsalUiRequiredException happened on AcquireTokenSilent.
 // This indicates you need to call AcquireTokenInteractive to acquire a token
 System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

 try
 {
    result = await app.AcquireTokenInteractive(scopes)
          .ExecuteAsync();
 }
 catch (MsalException msalex)
 {
    ResultText.Text = $"Error Acquiring Token:{System.Environment.NewLine}{msalex}";
 }
}
catch (Exception ex)
{
 ResultText.Text = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
 return;
}

if (result != null)
{
 string accessToken = result.AccessToken;
 // Use the token
}
```

For the code in context, see the [active-directory-dotnet-desktop-msgraph-v2](https://github.com/Azure-Samples/active-directory-dotnet-desktop-msgraph-v2/blob/master/active-directory-wpf-msgraph-v2/MainWindow.xaml.cs#L45-L67) sample.

#### Recommended call pattern in public client applications with MSAL.NET 2.x

```csharp
AuthenticationResult result = null;
var accounts = await app.GetAccountsAsync();

try
{
 result = await app.AcquireTokenSilentAsync(scopes, accounts.FirstOrDefault());
}
catch (MsalUiRequiredException ex)
{
 // A MsalUiRequiredException happened on AcquireTokenSilentAsync.
 // This indicates you need to call AcquireTokenAsync to acquire a token
 System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

 try
 {
    result = await app.AcquireTokenAsync(scopes);
 }
 catch (MsalException msalex)
 {
    ResultText.Text = $"Error Acquiring Token:{System.Environment.NewLine}{msalex}";
 }
}
catch (Exception ex)
{
 ResultText.Text = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
 return;
}

if (result != null)
{
 string accessToken = result.AccessToken;
 // Use the token
}
```

### Recommended call pattern in public client applications with  MSAL.NET 1.x

Previous versions of MSAL.NET were using `IUser` instead of `IAccount`. The code was as follows:

```csharp
AuthenticationResult result = null;
try
{
    result = await app.AcquireTokenSilentAsync(scopes, app.Users.FirstOrDefault());
}
catch (MsalUiRequiredException ex)
{
    // A MsalUiRequiredException happened on AcquireTokenSilentAsync.
    // This indicates you need to call AcquireTokenAsync to acquire a token
    System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

    try
    {
        result = await app.AcquireTokenAsync(scopes);
    }
    catch (MsalException msalex)
    {
        ResultText.Text = $"Error Acquiring Token:{System.Environment.NewLine}{msalex}";
    }
}
catch (Exception ex)
{
    ResultText.Text = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
    return;
}

if (result != null)
{
    string accessToken = result.AccessToken;
    // Use the token
}

```

For the code in context, see the [active-directory-dotnet-desktop-msgraph-v2](https://github.com/Azure-Samples/active-directory-dotnet-desktop-msgraph-v2/blob/master/active-directory-wpf-msgraph-v2/MainWindow.xaml.cs#L45-L67) sample

### Recommended call pattern in Web Apps using the Authorization Code flow to authenticate the user

For Web applications that use OpenID Connect [Authorization Code](./web-apps-apis/authorization-codes.md) flow, the recommended pattern in the Controllers is to:

- instantiate a `ConfidentialClientApplication` with a token cache for which you would have customized the serialization see [Token cache serialization in MSAL.NET](/azure/active-directory/develop/msal-net-token-cache-serialization?tabs=aspnet)
- Call `AcquireTokenByAuthorizationCode`

Then in the web app, each time you want to get a token for an API, just call `AcquireTokenSilent`. If `AcquireTokenSilent` throws an `MsalUiRequiredException`, then the web API will need to challenge the user.
