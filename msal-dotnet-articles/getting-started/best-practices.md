# Do not!

This page gathers recommendations about does and don't

### Never parse an access token

Even if you can have a look at what is contained in an Access token (for instance using https://jwt.ms), for education, or debugging purposes, you should never parse an access token part of your client code. The access token is only meant for the Web API (resource) it's acquired for, and this Web API is the only one should crack open it. Most of the time the Web apis will use a middleware layer (for instance [Identity model extension for .NET](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki) in .NET), as this is complex code, about the protection of your web apps and Web apis, and you don't want to introduce security vulnerabilities by forgetting some important paths.

### Don't acquire tokens from AAD too often

The standard pattern of acquiring tokens is:

```
acquire token silent (from the cache)
if that doesn't work, acquire a token from the AAD
```

If you skip step 1, your app may be asking for tokens from AAD very often. This provides a bad user experience, because it is slow. And it is error prone, because AAD might throttle you.

### Don't handle token expiration on your own

Even if `AuthenticationResult` returns the Expiry of the token, you should not handle the expiration and the refresh of the access tokens on your own. MSAL.NET does this for you.

For flows retrieving tokens for a user account, you'd want to use the recommended pattern as these write tokens to the user token cache, and tokens are retrieved and refreshed (if needed) silently by `AcquireTokenSilent`

```CSharp
AuthenticationResult result;
try
{
 // will handle expired Access Tokens by fetching new ones using the Refresh Token
 result = await AcquireTokenSilent(scopes).ExecuteAsync();
}
catch(MsalUiRequiredException ex)
{
 result = AcquireTokenXXXX(scopes, ..).WithXXX(…).ExecuteAsync();
}
```

Finally if you use `AcquireTokenForClient` (client credentials) you don't need it to bother of the cache as this method not only stores tokens to the application cache, but it also looks them up and refreshes them if needed (this is the only method interacting with the application token cache: the cache for tokens for the application itself)
MSAL.NET 

