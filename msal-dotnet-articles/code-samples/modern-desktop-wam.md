# Modern Desktop Application with WAM (Windows Authentication Manager)

This sample demonstrates the latest MSAL.NET best practices for desktop applications using the Windows Authentication Manager (WAM) for enhanced security and user experience.

## Features Demonstrated

- **Windows Authentication Manager (WAM)**: Native Windows authentication experience
- **Modern logging**: Structured logging with proper categorization
- **Secure token caching**: Memory-based token cache for desktop apps
- **Regional endpoint optimization**: Automatic region discovery for improved performance
- **Comprehensive error handling**: Specific handling for common MSAL scenarios
- **Modern async patterns**: Proper async/await usage throughout

## Prerequisites

- Windows 10 version 1903 or later (for WAM support)
- .NET 6.0 or later
- Visual Studio 2022 or later

## NuGet Packages Required

```xml
<PackageReference Include="Microsoft.Identity.Client" Version="4.61.3" />
<PackageReference Include="Microsoft.Identity.Client.Broker" Version="4.61.3" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
```

## Complete Code Sample

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using Microsoft.Identity.Client.Extensibility;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace ModernDesktopMSALSample
{
    /// <summary>
    /// Modern MSAL.NET desktop application demonstrating WAM integration
    /// and latest best practices for authentication
    /// </summary>
    public class ModernDesktopAuthenticationService
    {
        private readonly IPublicClientApplication _app;
        private readonly ILogger<ModernDesktopAuthenticationService> _logger;
        private readonly string[] _scopes = { "User.Read", "Files.Read" };

        public ModernDesktopAuthenticationService(ILogger<ModernDesktopAuthenticationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _app = CreatePublicClientApplication();
        }

        /// <summary>
        /// Creates a PublicClientApplication with modern configuration
        /// </summary>
        private IPublicClientApplication CreatePublicClientApplication()
        {
            var builder = PublicClientApplicationBuilder
                .Create("your-client-id-here") // Replace with your actual client ID
                .WithAuthority("https://login.microsoftonline.com/common") // Use common for multi-tenant
                .WithRedirectUri("http://localhost") // Modern redirect URI for desktop apps
                .WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows)
                {
                    // Enable WAM for enhanced security and user experience
                    Title = "Modern MSAL Desktop Sample",
                    // Use system webview for better integration
                    MsaPassthrough = true
                })
                .WithLogging(LogCallback, LogLevel.Info, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                .WithClientName("ModernDesktopMSALSample")
                .WithClientVersion("1.0.0");

            // Enable regional endpoint discovery for improved performance
            builder.WithInstanceDiscovery(false) // Disable instance discovery for better performance
                   .WithAzureRegion(ConfidentialClientApplication.AttemptRegionDiscovery);

            return builder.Build();
        }

        /// <summary>
        /// Modern logging callback with structured logging
        /// </summary>
        private void LogCallback(LogLevel level, string message, bool containsPii)
        {
            var logLevel = level switch
            {
                LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
                LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
                LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
                LogLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Debug,
                _ => Microsoft.Extensions.Logging.LogLevel.Information
            };

            _logger.Log(logLevel, "MSAL: {Message}", message);
        }

        /// <summary>
        /// Acquire token silently with modern error handling
        /// </summary>
        public async Task<AuthenticationResult> AcquireTokenSilentlyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to acquire token silently...");

                var accounts = await _app.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();

                if (firstAccount == null)
                {
                    _logger.LogWarning("No cached accounts found. Interactive authentication required.");
                    throw new InvalidOperationException("No cached accounts available. Please sign in interactively first.");
                }

                var result = await _app.AcquireTokenSilent(_scopes, firstAccount)
                    .ExecuteAsync(cancellationToken);

                _logger.LogInformation("Token acquired silently successfully for user: {Username}", result.Account.Username);
                return result;
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogInformation("UI required for token acquisition: {Error}", ex.ErrorCode);
                throw; // Re-throw to handle in calling code
            }
            catch (MsalServiceException ex)
            {
                _logger.LogError(ex, "Service error during silent token acquisition: {ErrorCode}", ex.ErrorCode);
                throw;
            }
            catch (MsalClientException ex)
            {
                _logger.LogError(ex, "Client error during silent token acquisition: {ErrorCode}", ex.ErrorCode);
                throw;
            }
        }

        /// <summary>
        /// Acquire token interactively with WAM integration
        /// </summary>
        public async Task<AuthenticationResult> AcquireTokenInteractivelyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting interactive token acquisition with WAM...");

                var result = await _app.AcquireTokenInteractive(_scopes)
                    .WithPrompt(Prompt.SelectAccount) // Allow user to select account
                    .WithUseEmbeddedWebView(false) // Use system browser for better UX
                    .WithParentActivityOrWindow(GetParentWindow()) // Set parent window for proper modal behavior
                    .ExecuteAsync(cancellationToken);

                _logger.LogInformation("Interactive token acquisition successful for user: {Username}", result.Account.Username);
                return result;
            }
            catch (MsalUserCanceledException)
            {
                _logger.LogInformation("User cancelled the authentication flow");
                throw;
            }
            catch (MsalServiceException ex)
            {
                _logger.LogError(ex, "Service error during interactive token acquisition: {ErrorCode}", ex.ErrorCode);
                throw;
            }
            catch (MsalClientException ex)
            {
                _logger.LogError(ex, "Client error during interactive token acquisition: {ErrorCode}", ex.ErrorCode);
                throw;
            }
        }

        /// <summary>
        /// Complete authentication flow with fallback handling
        /// </summary>
        public async Task<AuthenticationResult> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // First, try to acquire token silently
                return await AcquireTokenSilentlyAsync(cancellationToken);
            }
            catch (MsalUiRequiredException)
            {
                // If silent acquisition fails, fall back to interactive
                _logger.LogInformation("Silent token acquisition failed, falling back to interactive authentication");
                return await AcquireTokenInteractivelyAsync(cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // No cached accounts, go straight to interactive
                _logger.LogInformation("No cached accounts found, starting interactive authentication");
                return await AcquireTokenInteractivelyAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        public async Task SignOutAsync()
        {
            try
            {
                _logger.LogInformation("Signing out user...");
                var accounts = await _app.GetAccountsAsync();
                
                foreach (var account in accounts)
                {
                    await _app.RemoveAsync(account);
                    _logger.LogInformation("Removed account: {Username}", account.Username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign out");
                throw;
            }
        }

        /// <summary>
        /// Get parent window handle for proper modal behavior
        /// </summary>
        private IntPtr GetParentWindow()
        {
            // In a real application, you would get the handle of your main window
            // For console apps, you can use the console window handle
            return System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        }
    }

    /// <summary>
    /// Example usage of the modern authentication service
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Set up logging
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information));
            
            var logger = loggerFactory.CreateLogger<ModernDesktopAuthenticationService>();
            
            // Create authentication service
            var authService = new ModernDesktopAuthenticationService(logger);

            try
            {
                // Authenticate user
                var result = await authService.AuthenticateAsync();
                
                Console.WriteLine($"Authentication successful!");
                Console.WriteLine($"User: {result.Account.Username}");
                Console.WriteLine($"Token expires: {result.ExpiresOn}");
                
                // Use the access token to call Microsoft Graph or other APIs
                Console.WriteLine($"Access token: {result.AccessToken[..10]}...");
                
                // Example: Sign out when done
                Console.WriteLine("Press any key to sign out...");
                Console.ReadKey();
                
                await authService.SignOutAsync();
                Console.WriteLine("Signed out successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication failed: {ex.Message}");
            }
        }
    }
}
```

## Key Features Explained

### 1. **WAM Integration**
```csharp
.WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows)
{
    Title = "Modern MSAL Desktop Sample",
    MsaPassthrough = true
})
```
- Enables Windows Authentication Manager for native Windows authentication experience
- Provides single sign-on capabilities across Windows applications
- Enhanced security through hardware-backed authentication

### 2. **Modern Logging**
```csharp
.WithLogging(LogCallback, LogLevel.Info, enablePiiLogging: false, enableDefaultPlatformLogging: true)
```
- Structured logging with Microsoft.Extensions.Logging
- PII logging disabled for production security
- Platform logging enabled for better diagnostics

### 3. **Regional Optimization**
```csharp
.WithAzureRegion(ConfidentialClientApplication.AttemptRegionDiscovery)
```
- Automatic region discovery for improved performance
- Reduces latency by using geographically closer endpoints

### 4. **Comprehensive Error Handling**
- Specific handling for `MsalUiRequiredException`, `MsalServiceException`, and `MsalClientException`
- Graceful fallback from silent to interactive authentication
- Proper logging for debugging and monitoring

### 5. **Modern Async Patterns**
- Full async/await usage throughout
- CancellationToken support for proper cancellation handling
- Non-blocking UI operations

## Security Considerations

1. **Client ID**: Replace `"your-client-id-here"` with your actual Azure AD application client ID
2. **Redirect URI**: Configure matching redirect URI in Azure AD app registration
3. **Scopes**: Only request necessary scopes following principle of least privilege
4. **Token Storage**: Tokens are automatically cached securely by MSAL.NET
5. **PII Logging**: Disabled in production to prevent sensitive data exposure

## Performance Optimizations

1. **WAM**: Faster authentication through native Windows integration
2. **Regional Endpoints**: Reduced latency through geographic optimization
3. **Silent Authentication**: Minimize user interruption through cached tokens
4. **Connection Pooling**: MSAL.NET automatically pools HTTP connections

## References

- [MSAL.NET Documentation](https://learn.microsoft.com/azure/active-directory/develop/msal-net-overview)
- [Windows Authentication Manager (WAM)](https://learn.microsoft.com/azure/active-directory/develop/msal-net-wam)
- [Regional Endpoints](https://learn.microsoft.com/azure/active-directory/develop/msal-net-regional-endpoints)
