---
title: MSAL.NET code examples
description: Comprehensive code examples for common MSAL.NET scenarios with complete, working implementations.
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 01/15/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: reference
ms.custom: devx-track-csharp, aaddev
#Customer intent: As a developer, I want complete, working code examples for common MSAL.NET scenarios that I can copy and adapt.
---

# MSAL.NET code examples

This guide provides complete, working code examples for common MSAL.NET scenarios that you can copy and adapt for your applications.

## Desktop application examples

### Basic desktop authentication

```csharp
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DesktopAuthExample
{
    private static readonly string ClientId = "your-client-id";
    private static readonly string TenantId = "your-tenant-id";
    private static readonly string[] Scopes = { "User.Read" };
    
    private static IPublicClientApplication _app;
    
    public static async Task Main(string[] args)
    {
        // Initialize the MSAL client
        _app = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
            .WithDefaultRedirectUri()
            .WithLogging((level, message, containsPii) =>
            {
                Console.WriteLine($"[{level}] {message}");
            }, LogLevel.Info, enablePiiLogging: false)
            .Build();

        try
        {
            // Try to acquire token silently first
            var result = await AcquireTokenSilentAsync();
            
            if (result == null)
            {
                // Fall back to interactive authentication
                result = await AcquireTokenInteractiveAsync();
            }

            Console.WriteLine($"Welcome, {result.Account.Username}!");
            Console.WriteLine($"Token expires: {result.ExpiresOn}");
            
            // Use the token to call an API
            await CallGraphAPI(result.AccessToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task<AuthenticationResult> AcquireTokenSilentAsync()
    {
        try
        {
            var accounts = await _app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();
            
            if (firstAccount != null)
            {
                return await _app.AcquireTokenSilent(Scopes, firstAccount)
                    .ExecuteAsync();
            }
        }
        catch (MsalUiRequiredException)
        {
            // Expected when no token is in cache
        }
        
        return null;
    }

    private static async Task<AuthenticationResult> AcquireTokenInteractiveAsync()
    {
        return await _app.AcquireTokenInteractive(Scopes)
            .WithPrompt(Prompt.SelectAccount)
            .ExecuteAsync();
    }

    private static async Task CallGraphAPI(string accessToken)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine("User profile:");
            Console.WriteLine(content);
        }
    }
}
```

### Desktop app with Windows Authentication Manager (WAM)

```csharp
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Desktop;
using System;
using System.Threading.Tasks;

public class DesktopWamExample
{
    private static readonly string ClientId = "your-client-id";
    private static readonly string[] Scopes = { "User.Read" };
    
    public static async Task Main(string[] args)
    {
        var app = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority("https://login.microsoftonline.com/organizations")
            .WithBroker(true) // Enable WAM
            .WithLogging((level, message, containsPii) =>
            {
                Console.WriteLine($"[{level}] {message}");
            }, LogLevel.Info)
            .Build();

        try
        {
            // Try silent authentication first
            var accounts = await app.GetAccountsAsync();
            AuthenticationResult result;
            
            if (accounts.Any())
            {
                result = await app.AcquireTokenSilent(Scopes, accounts.First())
                    .ExecuteAsync();
            }
            else
            {
                result = await app.AcquireTokenInteractive(Scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();
            }

            Console.WriteLine($"Authentication successful! Welcome, {result.Account.Username}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication failed: {ex.Message}");
        }
    }
}
```

## Web application examples

### ASP.NET Core web app with Microsoft.Identity.Web

```csharp
// Program.cs
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    .AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
```

```csharp
// Controllers/HomeController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;

[Authorize]
public class HomeController : Controller
{
    private readonly GraphServiceClient _graphServiceClient;

    public HomeController(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    [AuthorizeForScopes(Scopes = new[] { "User.Read" })]
    public async Task<IActionResult> Profile()
    {
        try
        {
            var user = await _graphServiceClient.Me.Request().GetAsync();
            return View(user);
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View();
        }
    }
}
```

```json
// appsettings.json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "CallbackPath": "/signin-oidc"
  },
  "MicrosoftGraph": {
    "BaseUrl": "https://graph.microsoft.com/v1.0",
    "Scopes": "User.Read"
  }
}
```

### Web API with downstream API calls

```csharp
// Program.cs
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    .AddInMemoryTokenCaches();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

```csharp
// Controllers/UserController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly GraphServiceClient _graphServiceClient;

    public UserController(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    [HttpGet("profile")]
    [AuthorizeForScopes(Scopes = new[] { "User.Read" })]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var user = await _graphServiceClient.Me.Request().GetAsync();
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("emails")]
    [AuthorizeForScopes(Scopes = new[] { "Mail.Read" })]
    public async Task<IActionResult> GetEmails()
    {
        try
        {
            var emails = await _graphServiceClient.Me.Messages
                .Request()
                .Top(10)
                .GetAsync();
            return Ok(emails);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

## Service/Daemon application examples

### Console daemon app with client secret

```csharp
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

public class DaemonExample
{
    private static readonly string ClientId = "your-client-id";
    private static readonly string ClientSecret = "your-client-secret";
    private static readonly string TenantId = "your-tenant-id";
    private static readonly string[] Scopes = { "https://graph.microsoft.com/.default" };
    
    public static async Task Main(string[] args)
    {
        var app = ConfidentialClientApplicationBuilder
            .Create(ClientId)
            .WithClientSecret(ClientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
            .WithLogging((level, message, containsPii) =>
            {
                Console.WriteLine($"[{level}] {message}");
            }, LogLevel.Info)
            .Build();

        try
        {
            var result = await app.AcquireTokenForClient(Scopes)
                .ExecuteAsync();

            Console.WriteLine($"Token acquired successfully!");
            Console.WriteLine($"Token expires: {result.ExpiresOn}");
            
            // Use the token to call Microsoft Graph
            await CallGraphAPI(result.AccessToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task CallGraphAPI(string accessToken)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await client.GetAsync("https://graph.microsoft.com/v1.0/users?$top=10");
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine("Users from Microsoft Graph:");
            Console.WriteLine(content);
        }
    }
}
```

### Daemon app with client certificate

```csharp
using Microsoft.Identity.Client;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

public class DaemonCertificateExample
{
    private static readonly string ClientId = "your-client-id";
    private static readonly string TenantId = "your-tenant-id";
    private static readonly string CertificateThumbprint = "your-certificate-thumbprint";
    private static readonly string[] Scopes = { "https://graph.microsoft.com/.default" };
    
    public static async Task Main(string[] args)
    {
        // Load certificate from certificate store
        var certificate = GetCertificateFromStore(CertificateThumbprint);
        
        var app = ConfidentialClientApplicationBuilder
            .Create(ClientId)
            .WithCertificate(certificate)
            .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
            .Build();

        try
        {
            var result = await app.AcquireTokenForClient(Scopes)
                .ExecuteAsync();

            Console.WriteLine("Token acquired with certificate!");
            await CallGraphAPI(result.AccessToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static X509Certificate2 GetCertificateFromStore(string thumbprint)
    {
        using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
        {
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            
            if (certs.Count == 0)
            {
                throw new InvalidOperationException($"Certificate with thumbprint {thumbprint} not found");
            }
            
            return certs[0];
        }
    }

    private static async Task CallGraphAPI(string accessToken)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await client.GetAsync("https://graph.microsoft.com/v1.0/users?$top=10");
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine("Users from Microsoft Graph:");
            Console.WriteLine(content);
        }
    }
}
```

## Mobile application examples

### Xamarin.Forms example (deprecated but still functional)

```csharp
// MainPage.xaml.cs
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

public partial class MainPage : ContentPage
{
    private static readonly string ClientId = "your-client-id";
    private static readonly string[] Scopes = { "User.Read" };
    
    private IPublicClientApplication _app;
    
    public MainPage()
    {
        InitializeComponent();
        InitializeApp();
    }

    private void InitializeApp()
    {
        _app = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority("https://login.microsoftonline.com/organizations")
            .WithRedirectUri($"msal{ClientId}://auth")
            .WithBroker(true)
            .Build();
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await _app.AcquireTokenInteractive(Scopes)
                .WithParentActivityOrWindow(App.ParentWindow)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();

            await DisplayAlert("Success", $"Welcome, {result.Account.Username}!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        try
        {
            var accounts = await _app.GetAccountsAsync();
            if (accounts.Any())
            {
                await _app.RemoveAsync(accounts.First());
                await DisplayAlert("Success", "Signed out successfully", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
```

## Error handling examples

### Comprehensive error handling

```csharp
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

public class ErrorHandlingExample
{
    private static IPublicClientApplication _app;
    private static readonly string[] Scopes = { "User.Read" };
    
    public static async Task<AuthenticationResult> AcquireTokenWithErrorHandling()
    {
        try
        {
            // Try silent authentication first
            var accounts = await _app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();
            
            if (firstAccount != null)
            {
                return await _app.AcquireTokenSilent(Scopes, firstAccount)
                    .ExecuteAsync();
            }
            
            // Fall back to interactive authentication
            return await _app.AcquireTokenInteractive(Scopes)
                .ExecuteAsync();
        }
        catch (MsalUiRequiredException ex)
        {
            Console.WriteLine($"UI Required: {ex.ErrorCode}");
            return await HandleUiRequiredException(ex);
        }
        catch (MsalServiceException ex)
        {
            Console.WriteLine($"Service Error: {ex.ErrorCode} - {ex.Message}");
            return await HandleServiceException(ex);
        }
        catch (MsalClientException ex)
        {
            Console.WriteLine($"Client Error: {ex.ErrorCode} - {ex.Message}");
            throw; // Most client errors are not recoverable
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }

    private static async Task<AuthenticationResult> HandleUiRequiredException(MsalUiRequiredException ex)
    {
        // Handle different UI required scenarios
        return await _app.AcquireTokenInteractive(Scopes)
            .WithClaims(ex.Claims) // Include any claims from the exception
            .WithPrompt(Prompt.SelectAccount)
            .ExecuteAsync();
    }

    private static async Task<AuthenticationResult> HandleServiceException(MsalServiceException ex)
    {
        switch (ex.ErrorCode)
        {
            case "AADSTS50079": // MFA required
                return await _app.AcquireTokenInteractive(Scopes)
                    .WithPrompt(Prompt.ForceLogin)
                    .ExecuteAsync();
                    
            case "AADSTS65001": // User consent required
                return await _app.AcquireTokenInteractive(Scopes)
                    .WithPrompt(Prompt.Consent)
                    .ExecuteAsync();
                    
            default:
                throw; // Re-throw if we can't handle it
        }
    }
}
```

## Token cache examples

### File-based token cache for desktop apps

```csharp
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.IO;
using System.Threading.Tasks;

public class TokenCacheExample
{
    private static readonly string ClientId = "your-client-id";
    private static readonly string[] Scopes = { "User.Read" };
    
    public static async Task Main(string[] args)
    {
        var app = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority("https://login.microsoftonline.com/organizations")
            .WithDefaultRedirectUri()
            .Build();

        // Configure token cache
        await ConfigureTokenCache(app);

        // Use the app as normal
        var result = await app.AcquireTokenInteractive(Scopes)
            .ExecuteAsync();

        Console.WriteLine($"Token acquired and cached! Welcome, {result.Account.Username}");
    }

    private static async Task ConfigureTokenCache(IPublicClientApplication app)
    {
        // Configure cache storage
        var storageProperties = new StorageCreationPropertiesBuilder("MyAppTokenCache", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
            .WithLinuxKeyring(
                schemaName: "com.mycompany.myapp",
                collection: "default",
                secretLabel: "My App Token Cache",
                attribute1: new KeyValuePair<string, string>("Version", "1"),
                attribute2: new KeyValuePair<string, string>("ProductGroup", "MyApps"))
            .WithMacKeyChain(
                serviceName: "MyAppTokenCache",
                accountName: "MyApp")
            .Build();

        // Create cache helper
        var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
        
        // Register cache with MSAL
        cacheHelper.RegisterCache(app.UserTokenCache);
    }
}
```

### Custom token cache implementation

```csharp
using Microsoft.Identity.Client;
using System;
using System.IO;
using System.Threading.Tasks;

public class CustomTokenCacheExample
{
    private static readonly string CacheFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MyApp", "token_cache.dat");

    public static void RegisterTokenCache(IPublicClientApplication app)
    {
        app.UserTokenCache.SetBeforeAccess(BeforeAccessNotification);
        app.UserTokenCache.SetAfterAccess(AfterAccessNotification);
    }

    private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        try
        {
            if (File.Exists(CacheFilePath))
            {
                var tokenCacheBytes = File.ReadAllBytes(CacheFilePath);
                args.TokenCache.DeserializeMsalV3(tokenCacheBytes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading token cache: {ex.Message}");
        }
    }

    private static void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        try
        {
            if (args.HasStateChanged)
            {
                var tokenCacheBytes = args.TokenCache.SerializeMsalV3();
                
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(CacheFilePath));
                
                File.WriteAllBytes(CacheFilePath, tokenCacheBytes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing token cache: {ex.Message}");
        }
    }
}
```

## Advanced scenarios

### Device code flow example

```csharp
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

public class DeviceCodeFlowExample
{
    private static readonly string ClientId = "your-client-id";
    private static readonly string TenantId = "your-tenant-id";
    private static readonly string[] Scopes = { "User.Read" };
    
    public static async Task Main(string[] args)
    {
        var app = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
            .Build();

        try
        {
            var result = await app.AcquireTokenWithDeviceCode(Scopes, deviceCodeResult =>
            {
                // Display the device code to the user
                Console.WriteLine(deviceCodeResult.Message);
                return Task.FromResult(0);
            }).ExecuteAsync();

            Console.WriteLine($"Authentication successful! Welcome, {result.Account.Username}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication failed: {ex.Message}");
        }
    }
}
```

### Username/password flow example (not recommended for production)

```csharp
using Microsoft.Identity.Client;
using System;
using System.Security;
using System.Threading.Tasks;

public class UsernamePasswordExample
{
    private static readonly string ClientId = "your-client-id";
    private static readonly string TenantId = "your-tenant-id";
    private static readonly string[] Scopes = { "User.Read" };
    
    public static async Task Main(string[] args)
    {
        var app = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
            .Build();

        Console.Write("Username: ");
        var username = Console.ReadLine();
        
        Console.Write("Password: ");
        var password = ReadPassword();

        try
        {
            var result = await app.AcquireTokenByUsernamePassword(Scopes, username, password)
                .ExecuteAsync();

            Console.WriteLine($"Authentication successful! Welcome, {result.Account.Username}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication failed: {ex.Message}");
        }
    }

    private static SecureString ReadPassword()
    {
        var password = new SecureString();
        ConsoleKeyInfo keyInfo;
        
        do
        {
            keyInfo = Console.ReadKey(true);
            if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Backspace)
            {
                password.AppendChar(keyInfo.KeyChar);
                Console.Write("*");
            }
            else if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.RemoveAt(password.Length - 1);
                Console.Write("\b \b");
            }
        } while (keyInfo.Key != ConsoleKey.Enter);
        
        Console.WriteLine();
        return password;
    }
}
```

## Configuration examples

### Configuration class for dependency injection

```csharp
public class AuthenticationConfig
{
    public string Instance { get; set; } = "https://login.microsoftonline.com/";
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string[] Scopes { get; set; }
    public bool EnableLogging { get; set; } = true;
    public bool EnablePiiLogging { get; set; } = false;
    
    public string Authority => $"{Instance}{TenantId}";
}

// Usage in dependency injection
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<AuthenticationConfig>(Configuration.GetSection("Authentication"));
    
    services.AddSingleton<IPublicClientApplication>(provider =>
    {
        var config = provider.GetRequiredService<IOptions<AuthenticationConfig>>().Value;
        
        return PublicClientApplicationBuilder
            .Create(config.ClientId)
            .WithAuthority(config.Authority)
            .WithDefaultRedirectUri()
            .WithLogging((level, message, containsPii) =>
            {
                if (config.EnableLogging)
                {
                    Console.WriteLine($"[{level}] {message}");
                }
            }, LogLevel.Info, config.EnablePiiLogging)
            .Build();
    });
}
```

## Testing examples

### Unit test with mocked authentication

```csharp
using Microsoft.Identity.Client;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task AcquireToken_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var mockApp = new Mock<IPublicClientApplication>();
        var expectedResult = new AuthenticationResult(
            accessToken: "test-token",
            isExtendedLifeTimeToken: false,
            uniqueId: "test-user-id",
            expiresOn: DateTimeOffset.UtcNow.AddHours(1),
            extendedExpiresOn: DateTimeOffset.UtcNow.AddHours(2),
            tenantId: "test-tenant",
            account: Mock.Of<IAccount>(),
            idToken: "test-id-token",
            scopes: new[] { "User.Read" },
            correlationId: Guid.NewGuid(),
            tokenType: "Bearer",
            authenticationResultMetadata: Mock.Of<AuthenticationResultMetadata>()
        );

        mockApp.Setup(x => x.AcquireTokenInteractive(It.IsAny<string[]>()))
            .Returns(Mock.Of<AcquireTokenInteractiveParameterBuilder>());

        // Act & Assert
        // Your test logic here
    }
}
```

These examples provide a comprehensive foundation for implementing authentication in your .NET applications using MSAL.NET. Copy and adapt them based on your specific requirements, and don't forget to replace placeholder values with your actual configuration.

## Next steps

- [Choose the right authentication approach](authentication-decision-tree.md)
- [Follow best practices](best-practices.md)
- [Handle errors properly](../advanced/exceptions/practical-error-handling.md)
- [Browse more code samples](/azure/active-directory/develop/sample-v2-code)
