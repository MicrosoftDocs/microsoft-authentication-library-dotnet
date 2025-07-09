using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using Microsoft.Identity.Client.Extensibility;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModernDesktopWAM
{
    /// <summary>
    /// Configuration model for Azure AD settings
    /// </summary>
    public class AzureAdConfiguration
    {
        public string Instance { get; set; } = "https://login.microsoftonline.com/";
        public string TenantId { get; set; } = "common";
        public string ClientId { get; set; } = string.Empty;
        public string Authority => $"{Instance.TrimEnd('/')}/{TenantId}";
    }

    /// <summary>
    /// Modern MSAL.NET desktop application demonstrating WAM integration
    /// and latest best practices for authentication
    /// </summary>
    public class ModernDesktopAuthenticationService
    {
        private readonly IPublicClientApplication _app;
        private readonly ILogger<ModernDesktopAuthenticationService> _logger;
        private readonly string[] _scopes = { "User.Read", "Files.Read" };

        public ModernDesktopAuthenticationService(
            IPublicClientApplication app,
            ILogger<ModernDesktopAuthenticationService> logger)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    /// Factory for creating MSAL public client applications
    /// </summary>
    public class PublicClientFactory
    {
        private readonly ILogger<PublicClientFactory> _logger;

        public PublicClientFactory(ILogger<PublicClientFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a PublicClientApplication with modern configuration
        /// </summary>
        public IPublicClientApplication CreatePublicClientApplication(AzureAdConfiguration config)
        {
            var builder = PublicClientApplicationBuilder
                .Create(config.ClientId)
                .WithAuthority(config.Authority)
                .WithRedirectUri("http://localhost") // Modern redirect URI for desktop apps
                .WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows)
                {
                    // Enable WAM for enhanced security and user experience
                    Title = "Modern MSAL Desktop Sample",
                    // Use system webview for better integration
                    MsaPassthrough = true
                })
                .WithLogging(LogCallback, LogLevel.Info, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                .WithClientName("ModernDesktopWAMSample")
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
    }

    /// <summary>
    /// Service collection extensions for dependency injection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add modern MSAL public client services to DI container
        /// </summary>
        public static IServiceCollection AddModernMsalPublicClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register configuration
            var azureAdConfig = configuration.GetSection("AzureAd").Get<AzureAdConfiguration>()
                ?? throw new InvalidOperationException("AzureAd configuration section is missing");
            
            services.AddSingleton(azureAdConfig);

            // Register MSAL factory
            services.AddSingleton<PublicClientFactory>();

            // Register MSAL public client application
            services.AddSingleton<IPublicClientApplication>(provider =>
            {
                var factory = provider.GetRequiredService<PublicClientFactory>();
                var config = provider.GetRequiredService<AzureAdConfiguration>();
                return factory.CreatePublicClientApplication(config);
            });

            // Register the main service
            services.AddScoped<ModernDesktopAuthenticationService>();

            return services;
        }
    }

    /// <summary>
    /// Example usage of the modern authentication service
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Set up host with dependency injection
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddModernMsalPublicClient(configuration);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            try
            {
                // Get authentication service from DI container
                var authService = host.Services.GetRequiredService<ModernDesktopAuthenticationService>();
                var logger = host.Services.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("Starting Modern Desktop WAM Authentication Sample");

                // Authenticate user
                var result = await authService.AuthenticateAsync();
                
                Console.WriteLine($"Authentication successful!");
                Console.WriteLine($"User: {result.Account.Username}");
                Console.WriteLine($"Token expires: {result.ExpiresOn}");
                
                // Use the access token to call Microsoft Graph or other APIs
                Console.WriteLine($"Access token: {result.AccessToken[..10]}...");
                
                // Example: Sign out when done
                Console.WriteLine("\nPress any key to sign out...");
                Console.ReadKey();
                
                await authService.SignOutAsync();
                Console.WriteLine("Signed out successfully!");
            }
            catch (Exception ex)
            {
                var logger = host.Services.GetService<ILogger<Program>>();
                logger?.LogError(ex, "Authentication failed: {Message}", ex.Message);
                Console.WriteLine($"Authentication failed: {ex.Message}");
            }
            finally
            {
                await host.StopAsync();
            }
        }
    }
}
