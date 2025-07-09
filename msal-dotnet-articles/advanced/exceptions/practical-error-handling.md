---
title: Handle MSAL.NET errors with practical examples
description: Learn how to handle common MSAL.NET errors with practical examples and troubleshooting steps.
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 01/15/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: how-to
ms.custom: devx-track-csharp, aaddev
#Customer intent: As a developer, I want practical examples of how to handle common MSAL.NET errors in my application.
---

# Handle MSAL.NET errors with practical examples

This guide provides practical examples and solutions for handling common MSAL.NET errors in your applications.

## Quick error reference

| Error Code | Common Cause | Quick Fix |
|------------|--------------|-----------|
| `AADSTS50011` | Redirect URI mismatch | Update Azure portal or code |
| `AADSTS65001` | User consent required | Call `AcquireTokenInteractive` |
| `AADSTS50079` | MFA required | Fall back to interactive flow |
| `AADSTS90010` | Wrong authority endpoint | Use tenant-specific endpoint |
| `AADSTS70002` | Missing client secret | Check app registration type |

## Use comprehensive error handling pattern

Here's a robust error handling pattern you can use in your applications:

```csharp
public async Task<AuthenticationResult> AcquireTokenWithErrorHandling(
    IPublicClientApplication app, 
    string[] scopes, 
    IAccount account = null)
{
    try
    {
        // Try silent authentication first
        if (account != null)
        {
            return await app.AcquireTokenSilent(scopes, account)
                .ExecuteAsync();
        }
        
        // Fall back to interactive authentication
        return await app.AcquireTokenInteractive(scopes)
            .ExecuteAsync();
    }
    catch (MsalUiRequiredException ex)
    {
        return await HandleUiRequiredException(app, scopes, ex);
    }
    catch (MsalServiceException ex)
    {
        return await HandleServiceException(app, scopes, ex);
    }
    catch (MsalClientException ex)
    {
        return await HandleClientException(app, scopes, ex);
    }
    catch (Exception ex)
    {
        // Log unexpected errors
        Console.WriteLine($"Unexpected error: {ex.Message}");
        throw;
    }
}
```

## Handle MsalUiRequiredException

This is the most common exception you'll encounter. It indicates that user interaction is required.

```csharp
private async Task<AuthenticationResult> HandleUiRequiredException(
    IPublicClientApplication app, 
    string[] scopes, 
    MsalUiRequiredException ex)
{
    Console.WriteLine($"UI Required: {ex.ErrorCode}");
    
    switch (ex.ErrorCode)
    {
        case MsalError.InvalidGrantError:
            // Token expired or invalid, try interactive auth
            return await app.AcquireTokenInteractive(scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();
                
        case MsalError.NoTokensFoundError:
            // No tokens in cache, require interactive auth
            return await app.AcquireTokenInteractive(scopes)
                .ExecuteAsync();
                
        case MsalError.UserNullError:
            // User account is null, require interactive auth
            return await app.AcquireTokenInteractive(scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();
                
        default:
            // For other UI required errors, try interactive auth
            return await app.AcquireTokenInteractive(scopes)
                .WithClaims(ex.Claims) // Include any claims from the exception
                .ExecuteAsync();
    }
}
```

## Handle MsalServiceException

Service exceptions come from Azure AD and usually indicate server-side issues.

```csharp
private async Task<AuthenticationResult> HandleServiceException(
    IPublicClientApplication app, 
    string[] scopes, 
    MsalServiceException ex)
{
    Console.WriteLine($"Service Error: {ex.ErrorCode} - {ex.Message}");
    
    switch (ex.ErrorCode)
    {
        case "AADSTS50011": // Redirect URI mismatch
            throw new InvalidOperationException(
                "The redirect URI in your application registration doesn't match the one in your code. " +
                "Please check your Azure portal app registration.");
                
        case "AADSTS65001": // User consent required
            return await app.AcquireTokenInteractive(scopes)
                .WithPrompt(Prompt.Consent)
                .ExecuteAsync();
                
        case "AADSTS50079": // MFA required
            return await app.AcquireTokenInteractive(scopes)
                .WithPrompt(Prompt.ForceLogin)
                .ExecuteAsync();
                
        case "AADSTS90010": // Wrong endpoint
            throw new InvalidOperationException(
                "The authority endpoint is incorrect. Use a tenant-specific endpoint or /organizations instead of /common.");
                
        case "AADSTS70002": // Missing client secret
            throw new InvalidOperationException(
                "Your app registration needs a client secret or certificate. Check your Azure portal configuration.");
                
        default:
            // For other service errors, check if it's retryable
            if (IsRetryableError(ex))
            {
                await Task.Delay(TimeSpan.FromSeconds(2)); // Simple retry delay
                return await app.AcquireTokenInteractive(scopes)
                    .ExecuteAsync();
            }
            
            throw; // Re-throw if not retryable
    }
}
```

## Handle MsalClientException

Client exceptions indicate issues with the MSAL library configuration or environment.

```csharp
private async Task<AuthenticationResult> HandleClientException(
    IPublicClientApplication app, 
    string[] scopes, 
    MsalClientException ex)
{
    Console.WriteLine($"Client Error: {ex.ErrorCode} - {ex.Message}");
    
    switch (ex.ErrorCode)
    {
        case MsalError.UnknownUser:
            // Current user can't be identified
            return await app.AcquireTokenInteractive(scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();
                
        case MsalError.AuthenticationCanceledError:
            // User canceled authentication
            throw new OperationCanceledException("Authentication was canceled by the user.");
            
        case MsalError.AccessDenied:
            // User denied permission
            throw new UnauthorizedAccessException("User denied access to the requested resources.");
            
        case MsalError.BrowserNotAvailable:
            // No browser available for interactive auth
            throw new PlatformNotSupportedException("No web browser is available for interactive authentication.");
            
        default:
            // For other client errors, usually configuration issues
            throw new InvalidOperationException($"Client configuration error: {ex.Message}");
    }
}
```

## Implement retry logic

```csharp
private bool IsRetryableError(MsalServiceException ex)
{
    // Define which errors are worth retrying
    var retryableErrors = new[]
    {
        "AADSTS50020", // Temporary service error
        "AADSTS90019", // Temporary service error
        "AADSTS50140", // Temporary service error
        "AADSTS50197", // Temporary service error
    };
    
    return retryableErrors.Contains(ex.ErrorCode);
}

public async Task<AuthenticationResult> AcquireTokenWithRetry(
    IPublicClientApplication app, 
    string[] scopes, 
    int maxRetries = 3)
{
    for (int retry = 0; retry < maxRetries; retry++)
    {
        try
        {
            return await AcquireTokenWithErrorHandling(app, scopes);
        }
        catch (MsalServiceException ex) when (IsRetryableError(ex) && retry < maxRetries - 1)
        {
            // Exponential backoff
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry)));
            continue;
        }
        catch
        {
            // Don't retry for other exception types
            throw;
        }
    }
    
    throw new InvalidOperationException("Max retries exceeded");
}
```

## Configure logging and diagnostics

Enable comprehensive logging to help with troubleshooting:

```csharp
public IPublicClientApplication CreateAppWithLogging()
{
    return PublicClientApplicationBuilder
        .Create(ClientId)
        .WithAuthority(Authority)
        .WithLogging((level, message, containsPii) =>
        {
            // Log to your preferred logging framework
            Console.WriteLine($"[{level}] {message}");
            
            // In production, consider structured logging
            // logger.LogInformation("MSAL Log: {Level} - {Message}", level, message);
        }, 
        LogLevel.Info, 
        enablePiiLogging: false, // Set to true only for debugging
        enableDefaultPlatformLogging: true)
        .Build();
}
```

## Monitor authentication errors

Track authentication errors in your application:

```csharp
public class AuthenticationMetrics
{
    public void TrackAuthenticationError(string errorCode, string errorMessage)
    {
        // Track with your metrics provider (Application Insights, etc.)
        // telemetryClient.TrackException(new Exception($"{errorCode}: {errorMessage}"));
        
        // Log for immediate debugging
        Console.WriteLine($"Auth Error: {errorCode} - {errorMessage}");
    }
    
    public void TrackAuthenticationSuccess(string accountId)
    {
        // Track successful authentications
        Console.WriteLine($"Auth Success: {accountId}");
    }
}
```

## Follow troubleshooting steps

### Check your app registration

- Verify client ID and tenant ID
- Ensure redirect URIs match exactly
- Check API permissions are granted

### Review authority configuration

```csharp
// ✅ Good - Tenant-specific
.WithAuthority("https://login.microsoftonline.com/your-tenant-id")

// ❌ Avoid for client credentials
.WithAuthority("https://login.microsoftonline.com/common")
```

### Test with different accounts

- Try with different user accounts
- Test with admin accounts
- Check conditional access policies

### Handle network and proxy issues

```csharp
// Configure HTTP client for proxy environments
var app = PublicClientApplicationBuilder
    .Create(ClientId)
    .WithHttpClientFactory(new HttpClientFactory()) // Custom factory
    .Build();
```

## Best practices

1. **Always handle MsalUiRequiredException** - It's the most common exception
2. **Implement retry logic** for transient service errors
3. **Enable logging** in development and production
4. **Monitor authentication metrics** to identify patterns
5. **Use structured error handling** with specific actions for each error type
6. **Test error scenarios** during development
7. **Keep error messages user-friendly** while logging technical details

## Next steps

- [Complete error code reference](/azure/active-directory/develop/reference-error-codes)
- [MSAL.NET logging documentation](msal-logging.md)
- [Understanding MsalUiRequiredException](understanding-msaluirequiredexception.md)
- [Retry policy configuration](retry-policy.md)
