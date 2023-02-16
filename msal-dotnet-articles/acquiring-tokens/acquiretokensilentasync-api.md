### Token are cached

Once MSAL.NET has acquired a user token to call  a Web API, it caches it. Next time the application wants a token, it should first call ``AcquireTokenSilentAsync`` first, to verify if an acceptable token is in the cache, or can get derived. If not, a call to AcquireTokenAsync (in public client applications), or AcquireTokenXXX in confidential client applications will be needed. The only exceptions are:

- `AcquireTokenForClientAsync` ([Client credentials flow](Client-credential-flows)), which does not use the user token cache, but an application token cache. This method takes care of verifying this application token cache before sending a request to the STS
- `AcquireTokenByAuthorizationCodeAsync` in Web Apps, as it redeems a code that the application got by signing-in the user, and having them consent for more scopes. Since a code is passed as a parameter, and not an account, the method cannot look in the cache before redeeming the code, which requires, anyway, a call to the service.

### AcquireTokenAsync don't get token from the cache

Contrary to what happens in ADAL.NET, the design of MSAL.NET is such that ``AcquireTokenAsync`` never looks at the cache. As an application developer, you need to call ``AcquireTokenSilentAsync`` first. ``AcquireTokenSilentAsync`` is capable, in many cases, of silently getting another token with more scopes, based on a token in the cache. It's also capable of refreshing a token when it's getting close to expiration (as the token cache also contains a refresh token)

### Recommended call pattern in public client applications

The recommended call pattern is to first try to call ``AcquireTokenSilentAsync``, and if it fails with a ``MsalUiRequiredException``, call ``AcquireTokenAsync``

#### Recommended call pattern in public client applications with MSAL.NET 2.x

```CSharp
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

#### Recommended call pattern in public client applications with  MSAL.NET 1.x

Previous versions of MSAL.NET were using `IUser` instead of `IAccount`. The code was as follows:

```CSharp
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

For Web applications that use OpenID Connect [Authorization Code](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps) flow, the recommended pattern in the Controllers is to:

- instantiate a `ConfidentialClientApplication` with a token cache for which you would have customized the serialization [See token cache serialization for Web apps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization#token-cache-for-a-web-app-confidential-client-application)
- Call `AcquireTokenByAuthorizationCodeAsync`