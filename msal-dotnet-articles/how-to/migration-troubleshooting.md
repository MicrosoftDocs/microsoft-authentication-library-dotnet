---
title: Troubleshoot ADAL to MSAL.NET migration
description: A comprehensive troubleshooting guide for common issues when migrating from ADAL.NET to MSAL.NET.
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 01/15/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: troubleshooting
ms.custom: devx-track-csharp, aaddev
#Customer intent: As a developer migrating from ADAL.NET to MSAL.NET, I want to troubleshoot common issues and get solutions quickly.
---

# Troubleshoot ADAL to MSAL.NET migration

This guide helps you troubleshoot common issues when migrating from ADAL.NET to MSAL.NET.

## Migration checklist

Before troubleshooting, ensure you've completed these migration steps:

- [ ] Updated NuGet packages from `Microsoft.IdentityModel.Clients.ActiveDirectory` to `Microsoft.Identity.Client`
- [ ] Changed namespace from `Microsoft.IdentityModel.Clients.ActiveDirectory` to `Microsoft.Identity.Client`
- [ ] Replaced `AuthenticationContext` with `IPublicClientApplication` or `IConfidentialClientApplication`
- [ ] Updated authority URLs from v1.0 to v2.0 endpoints
- [ ] Changed from resources to scopes
- [ ] Updated token cache serialization code

## Common migration issues

### Issue: "The type or namespace name 'AuthenticationContext' could not be found"

**Cause**: Still using ADAL.NET namespace or incomplete package migration.

**Solution**:
1. Remove the old ADAL.NET package:
   ```bash
   dotnet remove package Microsoft.IdentityModel.Clients.ActiveDirectory
   ```

2. Add the new MSAL.NET package:
   ```bash
   dotnet add package Microsoft.Identity.Client
   ```

3. Update the namespace:
   ```csharp
   // OLD (ADAL.NET)
   using Microsoft.IdentityModel.Clients.ActiveDirectory;
   
   // NEW (MSAL.NET)
   using Microsoft.Identity.Client;
   ```

### Issue: "ResourceId not found" or "The name 'resourceId' does not exist"

**Cause**: MSAL.NET uses scopes instead of resource IDs.

**Solution**:
```csharp
// OLD (ADAL.NET)
string resourceId = "https://graph.microsoft.com/";
var result = await authContext.AcquireTokenAsync(resourceId, clientId, redirectUri, new PlatformParameters(PromptBehavior.Auto));

// NEW (MSAL.NET)
string[] scopes = { "https://graph.microsoft.com/.default" };
var result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
```

### Issue: "The redirect URI is not valid"

**Cause**: MSAL.NET has different redirect URI requirements.

**Solution**:
```csharp
// For public client applications
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithDefaultRedirectUri() // Use this for public clients
    .Build();

// For specific redirect URI
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithRedirectUri("http://localhost") // Must match Azure portal registration
    .Build();
```

### Issue: "Authority validation failed"

**Cause**: Different authority endpoint requirements between ADAL and MSAL.

**Solution**:
```csharp
// OLD (ADAL.NET) - v1.0 endpoint
string authority = "https://login.microsoftonline.com/common";

// NEW (MSAL.NET) - v2.0 endpoint (recommended)
string authority = "https://login.microsoftonline.com/organizations";
// OR tenant-specific
string authority = "https://login.microsoftonline.com/your-tenant-id";
```

### Issue: "Token cache serialization not working"

**Cause**: MSAL.NET uses different token cache serialization.

**Solution**:
```csharp
// OLD (ADAL.NET)
public class TokenCache
{
    public void EnableSerialization(byte[] tokenCacheBytes)
    {
        // ADAL serialization code
    }
}

// NEW (MSAL.NET)
public static class TokenCacheHelper
{
    public static void EnableSerialization(ITokenCache tokenCache)
    {
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
    }
    
    private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        // Load token cache from storage
        byte[] tokenCacheBytes = ReadFromStorage();
        args.TokenCache.DeserializeMsalV3(tokenCacheBytes);
    }
    
    private static void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        if (args.HasStateChanged)
        {
            // Save token cache to storage
            WriteToStorage(args.TokenCache.SerializeMsalV3());
        }
    }
}
```

### Issue: "Prompt behavior not working"

**Cause**: MSAL.NET uses different prompt options.

**Solution**:
```csharp
// OLD (ADAL.NET)
var result = await authContext.AcquireTokenAsync(
    resourceId, 
    clientId, 
    redirectUri, 
    new PlatformParameters(PromptBehavior.SelectAccount));

// NEW (MSAL.NET)
var result = await app.AcquireTokenInteractive(scopes)
    .WithPrompt(Prompt.SelectAccount)
    .ExecuteAsync();
```

### Issue: "Device code flow not working"

**Cause**: Different implementation in MSAL.NET.

**Solution**:
```csharp
// OLD (ADAL.NET)
var deviceCodeResult = await authContext.AcquireDeviceCodeAsync(resourceId, clientId);
var result = await authContext.AcquireTokenByDeviceCodeAsync(deviceCodeResult);

// NEW (MSAL.NET)
var result = await app.AcquireTokenWithDeviceCode(scopes, deviceCodeResult =>
{
    Console.WriteLine(deviceCodeResult.Message);
    return Task.FromResult(0);
}).ExecuteAsync();
```

### Issue: "Client credentials flow not working"

**Cause**: Different API in MSAL.NET.

**Solution**:
```csharp
// OLD (ADAL.NET)
var clientCredential = new ClientCredential(clientId, clientSecret);
var result = await authContext.AcquireTokenAsync(resourceId, clientCredential);

// NEW (MSAL.NET)
var app = ConfidentialClientApplicationBuilder
    .Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(authority)
    .Build();

var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
```

## Specific error codes and solutions

### AADSTS70011: The provided value for the input parameter 'scope' is not valid

**Cause**: Incorrect scope format.

**Solution**:
```csharp
// ❌ Wrong
var scopes = new[] { "https://graph.microsoft.com" };

// ✅ Correct
var scopes = new[] { "https://graph.microsoft.com/.default" };
```

### AADSTS50011: The reply URL specified in the request does not match

**Cause**: Redirect URI mismatch between code and Azure portal.

**Solution**:
1. Check your Azure portal app registration
2. Ensure exact match including trailing slashes
3. Use `WithDefaultRedirectUri()` for public clients

### AADSTS90014: The required field 'client_secret' is missing

**Cause**: Confidential client app missing client secret.

**Solution**:
```csharp
var app = ConfidentialClientApplicationBuilder
    .Create(clientId)
    .WithClientSecret(clientSecret) // Add this
    .WithAuthority(authority)
    .Build();
```

## Performance optimization after migration

### Cache single instance

```csharp
// ✅ Good - Single instance (singleton pattern)
public class AuthenticationService
{
    private static readonly Lazy<IPublicClientApplication> _app = 
        new Lazy<IPublicClientApplication>(() => 
            PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .Build());
    
    public static IPublicClientApplication App => _app.Value;
}

// ❌ Bad - Multiple instances
public async Task<AuthenticationResult> GetTokenAsync()
{
    var app = PublicClientApplicationBuilder.Create(clientId).Build();
    return await app.AcquireTokenInteractive(scopes).ExecuteAsync();
}
```

### Implement proper error handling

```csharp
public async Task<AuthenticationResult> GetTokenAsync()
{
    try
    {
        // Try silent authentication first
        var accounts = await app.GetAccountsAsync();
        var result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
            .ExecuteAsync();
        return result;
    }
    catch (MsalUiRequiredException)
    {
        // Fall back to interactive authentication
        return await app.AcquireTokenInteractive(scopes)
            .ExecuteAsync();
    }
}
```

## Testing your migration

### Create a test checklist

- [ ] Silent token acquisition works
- [ ] Interactive token acquisition works
- [ ] Token refresh works automatically
- [ ] Different user scenarios work (MFA, consent, etc.)
- [ ] Error handling works correctly
- [ ] Performance is acceptable
- [ ] Logging provides useful information

### Sample test code

```csharp
[Test]
public async Task Migration_SilentTokenAcquisition_Works()
{
    // Arrange
    var app = CreateTestApp();
    var scopes = new[] { "User.Read" };
    
    // First, get a token interactively to populate cache
    var interactiveResult = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
    
    // Act - Try silent acquisition
    var accounts = await app.GetAccountsAsync();
    var silentResult = await app.AcquireTokenSilent(scopes, accounts.First())
        .ExecuteAsync();
    
    // Assert
    Assert.IsNotNull(silentResult.AccessToken);
    Assert.AreEqual(interactiveResult.Account.HomeAccountId.Identifier, 
                   silentResult.Account.HomeAccountId.Identifier);
}
```

## Migration validation

### Before-and-after comparison

| Feature | ADAL.NET | MSAL.NET |
|---------|----------|----------|
| Package | Microsoft.IdentityModel.Clients.ActiveDirectory | Microsoft.Identity.Client |
| Namespace | Microsoft.IdentityModel.Clients.ActiveDirectory | Microsoft.Identity.Client |
| Main Class | AuthenticationContext | IPublicClientApplication / IConfidentialClientApplication |
| Authority | v1.0 endpoint | v2.0 endpoint |
| Scopes | resourceId (string) | scopes (string[]) |
| Token Cache | Extends TokenCache | Implements ITokenCache |

### Verify migration success

1. **Functionality**: All authentication flows work as expected
2. **Performance**: No significant performance degradation
3. **Error Handling**: Proper exception handling implemented
4. **Logging**: Comprehensive logging enabled
5. **Security**: All security best practices followed

## Get help

If you're still experiencing issues:

1. **Check the logs**: Enable MSAL logging and review the output
2. **Review the documentation**: [MSAL.NET migration guide](msal-net-migration.md)
3. **Search existing issues**: [GitHub Issues](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues)
4. **Get community support**: [Stack Overflow](https://stackoverflow.com/questions/tagged/msal)
5. **Report bugs**: [File a GitHub issue](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/new)

## Next steps

After successful migration:

- [Review MSAL.NET best practices](../getting-started/best-practices.md)
- [Implement proper error handling](../advanced/exceptions/practical-error-handling.md)
- [Configure monitoring and logging](../advanced/monitoring.md)
- [Optimize performance](../advanced/performance-testing.md)
