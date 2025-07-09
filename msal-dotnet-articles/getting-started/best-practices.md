---
title: Best practices for MSAL.NET
description: Learn the best practices when using MSAL.NET in your application development scenario.
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
# Customer intent: As an application developer, I want to learn the best practices for using MSAL.NET in my development scenario
---


# Best practices for MSAL.NET

## Never parse an access token

While you can have a look at the contents of an access token (for instance, using https://jwt.ms), for education, or debugging purposes, you should never parse an access token as part of your client code. The access token is only meant for the Web API or the resource it was acquired for. In most cases, web APIs use a middleware layer (for instance [Identity model extension for .NET](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki) in .NET), as this is complex code, about the protection of your web apps and Web APIs, and you don't want to introduce security vulnerabilities by forgetting some important paths.

<a name='dont-acquire-tokens-from-azure-ad-too-often'></a>

## Don't acquire tokens from Microsoft Entra ID too often

The standard pattern of acquiring tokens is: (i) acquire a token from the cache silently and (ii) if it doesn't work, acquire a new token from Microsoft Entra ID. If you skip the first step, your app may be acquiring tokens from Microsoft Entra too often. This provides a bad user experience, because it is slow and error prone as the identity provider might throttle you.

## Don't handle token expiration on your own

Even if `AuthenticationResult` returns the expiry of the token, you should not handle the expiration and the refresh of the access tokens on your own. MSAL.NET does this for you. For flows retrieving tokens for a user account, you'd want to use the recommended pattern as these write tokens to the user token cache, and tokens are retrieved and refreshed (if needed) silently by `AcquireTokenSilent`

```csharp
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

If you use `AcquireTokenForClient` in the client credentials flow, you don't need to worry about the cache as this method not only stores tokens to the application cache, but also looks them up and refreshes them if needed. This is the only method interacting with the application token cache, the cache for tokens for the application itself.

## Use a single instance of the client application

Create a single instance of `IPublicClientApplication` or `IConfidentialClientApplication` and reuse it throughout your application's lifetime. This ensures optimal performance and proper token caching.

```csharp
// ✅ Good - Single instance (using dependency injection)
public class AuthenticationService
{
    private readonly IPublicClientApplication _app;
    
    public AuthenticationService(IPublicClientApplication app)
    {
        _app = app;
    }
    
    public async Task<AuthenticationResult> SignInAsync(string[] scopes)
    {
        return await _app.AcquireTokenInteractive(scopes).ExecuteAsync();
    }
}

// ❌ Bad - Creating new instances
public async Task<AuthenticationResult> SignInAsync(string[] scopes)
{
    var app = PublicClientApplicationBuilder.Create(clientId).Build();
    return await app.AcquireTokenInteractive(scopes).ExecuteAsync();
}
```

## Enable comprehensive logging

Always enable MSAL logging to help with troubleshooting authentication issues. Use modern structured logging patterns:

```csharp
// Modern structured logging approach
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithLogging((level, message, containsPii) =>
    {
        // Use structured logging with your preferred framework
        var logLevel = level switch
        {
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Debug,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };
        
        logger.Log(logLevel, "MSAL: {Message}", message);
    }, 
    LogLevel.Info, 
    enablePiiLogging: false, // Set to true only for debugging in development
    enableDefaultPlatformLogging: true)
    .Build();
```

For production applications, integrate with Microsoft.Extensions.Logging:

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddApplicationInsights(); // For Azure monitoring
});
```

## Implement proper error handling

Always handle the three main MSAL exception types:

```csharp
try
{
    var result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();
}
catch (MsalUiRequiredException)
{
    // User interaction required - call AcquireTokenInteractive
    var result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
}
catch (MsalServiceException ex)
{
    // Service error - check error code and handle appropriately
    Console.WriteLine($"Service error: {ex.ErrorCode}");
}
catch (MsalClientException ex)
{
    // Client error - usually configuration issues
    Console.WriteLine($"Client error: {ex.ErrorCode}");
}
```

## Use appropriate authority endpoints

Choose the correct authority endpoint based on your application's requirements:

```csharp
// ✅ Good - Tenant-specific for single-tenant apps
.WithAuthority("https://login.microsoftonline.com/your-tenant-id")

// ✅ Good - Organizations endpoint for multi-tenant apps
.WithAuthority("https://login.microsoftonline.com/organizations")

// ⚠️ Use with caution - Common endpoint allows personal accounts
.WithAuthority("https://login.microsoftonline.com/common")

// ❌ Avoid for client credentials flow
.WithAuthority("https://login.microsoftonline.com/common") // in confidential client apps
```

## Implement token cache serialization for web apps

For web applications, implement proper token cache serialization:

```csharp
// Configure token cache serialization in Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches(); // Or AddDistributedTokenCaches for production
}
```

## Follow the principle of least privilege

Request only the minimum scopes your application needs:

```csharp
// ✅ Good - Specific scopes
var scopes = new[] { "User.Read", "Mail.Read" };

// ❌ Avoid - Too broad permissions
var scopes = new[] { "https://graph.microsoft.com/.default" };
```

## Handle conditional access and claims challenges

Be prepared to handle additional authentication requirements:

```csharp
try
{
    var result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();
}
catch (MsalUiRequiredException ex)
{
    // Check if this is a claims challenge
    if (!string.IsNullOrEmpty(ex.Claims))
    {
        // Handle claims challenge
        var result = await app.AcquireTokenInteractive(scopes)
            .WithClaims(ex.Claims)
            .ExecuteAsync();
    }
}
```

## Optimize performance in production

### Use regional endpoints for better performance

```csharp
var app = ConfidentialClientApplicationBuilder
    .Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority("https://login.microsoftonline.com/tenant-id")
    .WithAzureRegion(ConfidentialClientApplication.AttemptRegionDiscovery)
    .Build();
```

### Implement proper retry policies

```csharp
var retryPolicy = Policy
    .Handle<MsalServiceException>(ex => IsRetryableError(ex))
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
    );

await retryPolicy.ExecuteAsync(async () =>
{
    return await app.AcquireTokenForClient(scopes).ExecuteAsync();
});
```

## Security considerations

### Validate redirect URIs

Always validate that redirect URIs in your app registration match exactly. Use specific redirect URIs instead of generic ones:

```csharp
// ✅ Good - Specific redirect URI for desktop apps
.WithRedirectUri("http://localhost")  // Recommended for desktop apps

// ✅ Good - Specific redirect URI for web apps
.WithRedirectUri("https://localhost:5001/signin-oidc")

// ⚠️ Avoid generic patterns in production
.WithRedirectUri("https://localhost")  // Too generic for production use
```

> **Important**: The `WithDefaultRedirectUri()` method is being phased out in favor of explicit redirect URI configuration. Always specify your redirect URI explicitly.

### Secure token storage

- Never store tokens in plain text
- Use secure storage mechanisms (KeyChain on iOS, Credential Manager on Windows)
- Implement proper token encryption for cross-platform scenarios

### Use HTTPS in production

```csharp
// ✅ Good - HTTPS redirect URI
.WithRedirectUri("https://myapp.com/auth/callback")

// ❌ Bad - HTTP in production
.WithRedirectUri("http://myapp.com/auth/callback")
```

## Testing best practices

### Test different user scenarios

- Test with users who have MFA enabled
- Test with users who need to consent to permissions
- Test with guest users from other tenants
- Test with users who have conditional access policies

### Use integration tests

```csharp
[Test]
public async Task AcquireToken_WithValidCredentials_ReturnsToken()
{
    // Arrange
    var app = CreateTestApp();
    var scopes = new[] { "User.Read" };
    
    // Act
    var result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
    
    // Assert
    Assert.IsNotNull(result.AccessToken);
    Assert.IsTrue(result.ExpiresOn > DateTimeOffset.UtcNow);
}
```

## Monitoring and diagnostics

### Track authentication metrics

```csharp
public class AuthenticationMetrics
{
    public void TrackSuccessfulAuthentication(string userId)
    {
        // Track with Application Insights or similar
        telemetryClient.TrackEvent("Authentication.Success", new Dictionary<string, string>
        {
            ["UserId"] = userId,
            ["Timestamp"] = DateTime.UtcNow.ToString()
        });
    }
    
    public void TrackAuthenticationFailure(string errorCode, string errorMessage)
    {
        telemetryClient.TrackException(new Exception($"{errorCode}: {errorMessage}"));
    }
}
```

### Monitor token cache performance

```csharp
var stopwatch = Stopwatch.StartNew();
var result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();
stopwatch.Stop();

telemetryClient.TrackMetric("TokenAcquisition.Duration", stopwatch.ElapsedMilliseconds);
```

## Configuration management

### Use configuration files

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

### Environment-specific configurations

```csharp
public class AuthConfig
{
    public string Instance { get; set; }
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    
    public string Authority => $"{Instance}{TenantId}";
}
```

## Platform-specific considerations

### Desktop applications

- Use explicit redirect URIs: `WithRedirectUri("http://localhost")` instead of `WithDefaultRedirectUri()`
- **Enable WAM (Windows Authentication Manager)** for better user experience on Windows:
  ```csharp
  .WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows)
  {
      Title = "Your App Name",
      MsaPassthrough = true
  })
  ```
- Implement proper token cache serialization for cross-session persistence
- Consider using system browsers instead of embedded web views

### Web applications

- **Use Microsoft.Identity.Web** for ASP.NET Core applications instead of direct MSAL.NET
- Implement distributed token caching for scalability (`AddDistributedTokenCaches()`)
- Handle sign-out scenarios properly with proper session management
- Use Azure Key Vault for credential storage in production

### Mobile applications

- Use system browsers instead of embedded web views when possible
- Implement proper keychain/keystore integration for secure token storage
- Handle network connectivity issues gracefully
- Consider using broker authentication where available

### Cloud/Service applications

- **Use Managed Identity** for Azure-hosted applications to eliminate credential management
- Implement proper retry policies with exponential backoff
- Use regional endpoints for better performance
- Consider using certificate-based authentication over client secrets

## Common pitfalls to avoid

1. **Creating multiple client application instances** - Always use a single instance
2. **Not handling MsalUiRequiredException** - This is the most common exception
3. **Ignoring token expiration** - Let MSAL handle token refresh
4. **Using overly broad scopes** - Request only what you need
5. **Not implementing proper error handling** - Always handle the three main exception types
6. **Not enabling logging** - Essential for troubleshooting
7. **Hardcoding configuration values** - Use configuration files or environment variables
8. **Not testing error scenarios** - Test with various user conditions
9. **Ignoring security best practices** - Always use HTTPS, validate redirect URIs
10. **Not monitoring authentication metrics** - Track success/failure rates and performance

## Avoid deprecated patterns

### Username/Password flows are discouraged

While `AcquireTokenByUsernamePassword` and `AcquireTokenByIntegratedWindowsAuth` are still available, they should be avoided in favor of more secure alternatives:

```csharp
// ❌ Avoid - Username/Password flow (ROPC)
var result = await app.AcquireTokenByUsernamePassword(scopes, username, password).ExecuteAsync();

// ✅ Prefer - Interactive authentication with broker
var result = await app.AcquireTokenInteractive(scopes)
    .WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows))
    .ExecuteAsync();
```

### SecureString is obsolete

Microsoft no longer recommends using `SecureString`. Use regular strings for passwords:

```csharp
// ❌ Avoid - SecureString is obsolete
var securePassword = new SecureString();
// ... populate securePassword
var result = await app.AcquireTokenByUsernamePassword(scopes, username, securePassword).ExecuteAsync();

// ✅ Use regular string (if username/password flow is absolutely necessary)
var result = await app.AcquireTokenByUsernamePassword(scopes, username, password).ExecuteAsync();
```

### Modern broker configuration

Use modern broker options instead of simple boolean flags:

```csharp
// ❌ Old pattern - Simple boolean
.WithBroker(true)

// ✅ Modern pattern - Explicit broker options
.WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows)
{
    Title = "Your App Name",
    MsaPassthrough = true  // Enable MSA passthrough if needed
})
```

## Leverage Microsoft.Identity.Web for web applications

For ASP.NET Core applications, use Microsoft.Identity.Web instead of directly using MSAL.NET:

```csharp
// ✅ Modern approach for web apps
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddMicrosoftGraph(Configuration.GetSection("GraphAPI"))
        .AddInMemoryTokenCaches(); // Or AddDistributedTokenCaches for production
}
```

This approach provides:
- Automatic token cache management
- Built-in token acquisition services
- Integration with ASP.NET Core's dependency injection
- Support for Microsoft Graph SDK
- Automatic token refresh handling
