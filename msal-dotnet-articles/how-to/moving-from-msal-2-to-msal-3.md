This page explains how to change the code to move from the MSAL 2.x to MSAL 3.x

```CSharp
IEnumerable<string> scopes = new string[]{"user.read"};
IAccount account;
string authority;
bool forceRefresh = false;
```

## ClientApplicationBase

### AcquireTokenSilent

Used to acquire an access token from the user cache, and refresh it if needed

<table>
<tr><td>Instead of</td><td>use</td></tr>
<tr><td>

```CSharp
app.AcquireTokenSilentAsync(scopes,
                            account)
```
</td><td>

```CSharp
app.AcquireTokenSilent(scopes,
                       account)   
   .ExecuteAsync()
   .ConfigureAwait(false);
```
</td></tr>

<tr><td>

```CSharp
app.AcquireTokenSilentAsync(scopes,
                            account, 
                            authority,
                            forceRefresh)
```
</td><td>

```CSharp
app.AcquireTokenSilent(scopes, account)
   .WithAuthority(authority)
   .WithForceRefresh(forceRefresh)    
   .ExecuteAsync()
   .ConfigureAwait(false);
```
</td></tr>
</table>

## PublicClientApplication

### Constructors of PublicClientApplication

Instead of calling the constructor of PublicClientApplication directly, use the `PublicClientApplicationBuilder.Create()` or the `PublicClientApplicationBuilder.CreateWithOptions()` methods. The reference documentation page for [PublicClientApplicationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.identity.client.appconfig.publicclientapplicationbuilder?view=azure-dotnet-preview) shows all the options that you can use.

```CSharp
string clientId;
PublicClientApplicationOptions options;
```

<table>
<tr><td>Instead of</td><td>use</td></tr>
<tr><td>

```CSharp
app=new PublicClientApplication(clientId);
```
</td><td>

```CSharp
app=PublicClientApplicationBuilder
    .Create(clientId)
    .Build();
```
</td></tr>

<tr><td>

```CSharp
app=new PublicClientApplication(clientId,
                                authority);
```
</td><td>

```CSharp
app=PublicClientApplicationBuilder
   .Create(clientId)
   .WithAuthority(authority)
   .Build();
```

or
```CSharp
options = new PublicClientApplicationOptions()
{
 ClientId = client,
 Authority = authority
};
app=PublicClientApplicationBuilder
   .CreateWithOptions(options )
   .Build();
```

</td></tr>
</table>

### Acquire Token interactive

MSAL.NET 2.x had twelve overrides of `AcquireTokenAsync`


<table>
<tr><td>Instead of</td><td>use</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes, null)
    .ExecuteAsync().
    .ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes, loginHint)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes, null)
    .WithLoginHint(loginHint)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes, account)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes, null)
    .WithAccount(account)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes,
                      loginHint,
                      uiBehavior,
                      extraQueryParameters)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes, null)
    .WithLoginHint(account)
    .WithPrompt(prompt)
    .WithExtraQueryParameters(extraQueryParameters)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes,
                      loginHint,
                      uiBehavior,
                      extraQueryParameters,
                      extraScopesToConsent,
                      authority)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes, null)
    .WithLoginHint(loginHint)
    .WithPrompt(prompt)
    .WithExtraQueryParameters(extraQueryParameters)
    .WithExtraSCopesToConsent(extraScopesToConsent)
    .WithAuthority(authority)
    .ExecuteAsync()
    .ConfigureAwait(false);
```

but of course you only need to specify the parameters that you need
</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes,
                      account,
                      uiBehavior,
                      extraQueryParameters,
                      extraScopesToConsent,
                      authority)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes, null)
    .WithAccount(account)
    .WithPrompt(prompt)
    .WithExtraQueryParameters(extraQueryParameters)
    .WithExtraSCopesToConsent(extraScopesToConsent)
    .WithAuthority(authority)
    .ExecuteAsync()
    .ConfigureAwait(false);
```

</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes,
                      uiParent)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes,
                            parentObject)
    .WithUseEmbeddedWebView(useEmbeddedWebView)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes, 
                      loginHint,
                      uiParent)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes,
                            parentObject)
    .WithUseEmbeddedWebView(useEmbeddedWebView)
    .WithLoginHint(loginHint)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes,
                      account,
                      uiParent)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes,
                            parentObject)
    .WithUseEmbeddedWebView(useEmbeddedWebView)
    .WithAccount(account)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes,
                      loginHint,
                      uiBehavior,
                      extraQueryParameters,
                      uiParent)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes,
                            parentObject)
    .WithUseEmbeddedWebView(useEmbeddedWebView)
    .WithLoginHint(account)
    .WithPrompt(prompt)
    .WithExtraQueryParameters(extraQueryParameters)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes,
                      loginHint,
                      uiBehavior,
                      extraQueryParameters,
                      extraScopesToConsent,
                      authority,
                      uiParent)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes,
                            parentObject)
    .WithUseEmbeddedWebView(useEmbeddedWebView)
    .WithLoginHint(loginHint)
    .WithPrompt(prompt)
    .WithExtraQueryParameters(extraQueryParameters)
    .WithExtraSCopesToConsent(extraScopesToConsent)
    .WithAuthority(authority)
    .ExecuteAsync()
    .ConfigureAwait(false);
```

</td></tr>
<tr><td>

```CSharp
app.AcquireTokenAsync(scopes,
                      account,
                      uiBehavior,
                      extraQueryParameters,
                      extraScopesToConsent,
                      authority,
                      uiParent)
```
</td><td>

```CSharp
app=AcquireTokenInteractive(scopes,
                            parentObject)
    .WithUseEmbeddedWebView(useEmbeddedWebView)
    .WithAccount(account)
    .WithPrompt(prompt)
    .WithExtraQueryParameters(extraQueryParameters)
    .WithExtraSCopesToConsent(extraScopesToConsent)
    .WithAuthority(authority)
    .ExecuteAsync()
    .ConfigureAwait(false);
```

</td></tr>
</table>

For the list of all the .With operations applicable on AcquireTokenInteractive see [AcquireTokenInteractiveParameterBuilder](https://docs.microsoft.com/dotnet/api/microsoft.identity.client.apiconfig.acquiretokeninteractiveparameterbuilder?view=azure-dotnet-preview)

### Acquire Token by username password

<table>
<tr><td>Instead of</td><td>use</td></tr>
<tr><td>

```CSharp
app.AcquireTokenByUsernamePasswordAsync(scopes,
                                        username,
                                        securePassword)
```
</td><td>

```CSharp
app.AcquireTokenByUsernamePassword(scopes,
                                   username,
                                   password)   
   .ExecuteAsync()
   .ConfigureAwait(false);
```
</td></tr>
</table>

For the list of all the .With parameters on `AcquireTokenByUsernamePassword`, see [AcquireTokenByUsernamePasswordParameterBuilder](https://docs.microsoft.com/dotnet/api/microsoft.identity.client.apiconfig.acquiretokenbyusernamepasswordparameterbuilder?view=azure-dotnet-preview)

### Acquire token with device code flow


<table>
<tr><td>Instead of</td><td>use</td></tr>
<tr><td>

```CSharp
app
.AcquireTokenWithDeviceCodeAsync(scopes,
                                 deviceCodeResultCallback)
```
</td><td>

```CSharp
app
.AcquireTokenWithDeviceCode(scopes,
                            deviceCodeResultCallback)
.ExecuteAsync()
.ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app
.AcquireTokenWithDeviceCodeAsync(scopes,
                                 extraQueryParameters
                                 deviceCodeResultCallback)
```
</td><td>

```CSharp
app
.AcquireTokenWithDeviceCode(scopes,
                            deviceCodeResultCallback)
.WithExtraQueryParameters(extraQueryParameters)
.ExecuteAsync()
.ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app
.AcquireTokenWithDeviceCodeAsync(scopes,
                                 extraQueryParameters
                                 deviceCodeResultCallback,
                                 CancellationToken)
```
</td><td>

```CSharp
app
.AcquireTokenWithDeviceCode(scopes,
                            deviceCodeResultCallback)
.WithExtraQueryParameters(extraQueryParameters)
.ExecuteAsync(CancellationToken)
.ConfigureAwait(false);
```
</td></tr>
</table>

For the list of all the .With parameters on `AcquireTokenWithDeviceCode`, see [AcquireTokenWithDeviceCodeParameterBuilder](https://docs.microsoft.com/dotnet/api/microsoft.identity.client.apiconfig.acquiretokenwithdevicecodeparameterbuilder?view=azure-dotnet-preview)

### Acquire Token by refresh token

<table>
<tr><td>Instead of</td><td>use</td></tr>
<tr><td>

```CSharp
app
.AcquireTokenByRefreshTokenAsync(scopes,
                                 refreshToken)
```
</td><td>

```CSharp
IByRefreshToken brt = app as IByRefreshToken;
brt
.AcquireTokenByRefreshToken(scopes,
                            refreshToken)
.ExecuteAsync()
.ConfigureAwait(false);
```
</td></tr>

</table>

## ConfidentialClientApplication

### Constructors of ConfidentialClientApplication

Similar to the PublicClientApplication, use the `ConfidentialClientApplicationBuilder.Create()` or the `ConfidentialClientApplicationBuilder.CreateWithOptions()` methods to construct the ConfidentialClientApplication. The reference documentation page for [ConfidentialClientApplicationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.identity.client.appconfig.publicclientapplicationbuilder?view=azure-dotnet-preview) shows all the options that you can use.

```CSharp
string clientId;
ConfidentialClientApplicationOptions options;
```

<table>
<tr><td>Instead of</td><td>use</td></tr>
<tr><td>

```CSharp
app=new ConfidentialClientApplication(clientId);
```
</td><td>

```CSharp
app=ConfidentialClientApplicationBuilder
    .Create(clientId)
    .Build();
```
</td></tr>

<tr><td>

```CSharp
app=new ConfidentialClientApplication(clientId,
                                authority);
```
</td><td>

```CSharp
app=ConfidentialClientApplicationBuilder
   .Create(clientId)
   .WithAuthority(authority)
   .Build();
```

or
```CSharp
options = new ConfidentialClientApplicationOptions()
{
 ClientId = client,
 Authority = authority
};
app=ConfidentialClientApplicationBuilder
   .CreateWithOptions(options )
   .Build();
```

</td></tr>
</table>

### Acquire Token For Client

MSAL.NET 2.x had twelve overrides of `AcquireTokenForClientAsync`

### Acquire Token For Client
<table>
<tr><td>Instead of</td><td>use</td></tr>
<tr><td>

```CSharp
app.AcquireTokenForClientAsync(scopes)
```
</td><td>

```CSharp
app=AcquireTokenForClientAsync(scopes, null)
    .ExecuteAsync().
    .ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app.AcquireTokenForClientWithCertificateAsync(scopes)
```
</td><td>

```CSharp
app=AcquireTokenForClientAsync(scopes)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>
</table>

### AcquireTokenByAuthorizationCodeAsync

<table>
<tr><td>Instead of</td><td>use</td></tr>
<tr><td>

```CSharp
app.AcquireTokenByAuthorizationCodeAsync(authorizationCode, scopes)
```
</td><td>

```CSharp
app=AcquireTokenByAuthorizationCodeAsync(scopes, null)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>

<tr><td>

```CSharp
app.AcquireTokenByAuthorizationCodeAsync(authorizationCode, scopes, authority)
```
</td><td>

```CSharp
app=AcquireTokenByAuthorizationCodeAsync(scopes, null)
    .WithAuthority(authority)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>
<tr><td>
</table>

### Acquire Token On Behalf Of

<table>
<tr><td>Instead of</td><td>use</td></tr>
<tr><td>

```CSharp
app.AcquireTokenOnBehalfOfAsync(scopes, userAssertion)
```
</td><td>

```CSharp
app=AcquireTokenOnBehalfOfAsync(scopes, userAssertion)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>

<tr><td>

```CSharp
app.AcquireTokenOnBehalfOfAsync(scopes, userAssertion, authority)
```
</td><td>

```CSharp
app=AcquireTokenOnBehalfOfAsync(scopes, userAssertion)
    .WithAuthority(authority)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>
<tr><td>

```CSharp
app.AcquireTokenOnBehalfOfWithCertificateAsync(scopes, userAssertion)
```
</td><td>

```CSharp
app=AcquireTokenOnBehalfOfAsync(scopes, userAssertion)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>

<tr><td>

```CSharp
app.AcquireTokenOnBehalfOfWithCertificateAsync(scopes, userAssertion, authority)
```
</td><td>

```CSharp
app=AcquireTokenOnBehalfOfAsync(scopes, userAssertion)
    .ExecuteAsync()
    .ConfigureAwait(false);
```
</td></tr>
</table>