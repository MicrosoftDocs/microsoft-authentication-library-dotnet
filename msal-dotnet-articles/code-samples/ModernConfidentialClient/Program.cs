using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ModernConfidentialClient
{
    /// <summary>
    /// Configuration model for Azure AD settings
    /// </summary>
    public class AzureAdConfiguration
    {
        public string Instance { get; set; } = "https://login.microsoftonline.com/";
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string KeyVaultUrl { get; set; } = string.Empty;
        public string ClientSecretName { get; set; } = "AppClientSecret";
        public bool EnableRegionalEndpoints { get; set; } = true;
        public string Authority => $"{Instance.TrimEnd('/')}/{TenantId}";
    }

    /// <summary>
    /// Modern confidential client authentication service with Key Vault integration
    /// </summary>
    public class ModernConfidentialClientService
    {
        private readonly IConfidentialClientApplication _app;
        private readonly ILogger<ModernConfidentialClientService> _logger;
        private readonly AzureAdConfiguration _config;

        public ModernConfidentialClientService(
            IConfidentialClientApplication app,
            ILogger<ModernConfidentialClientService> logger,
            AzureAdConfiguration config)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Acquire token using client credentials flow (daemon applications)
        /// </summary>
        public async Task<AuthenticationResult> AcquireTokenForClientAsync(
            string[] scopes,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Acquiring token using client credentials flow for scopes: {Scopes}", 
                    string.Join(", ", scopes));

                var result = await _app.AcquireTokenForClient(scopes)
                    .ExecuteAsync(cancellationToken);

                _logger.LogInformation("Client credentials token acquired successfully. Expires: {ExpiresOn}", 
                    result.ExpiresOn);

                return result;
            }
            catch (MsalServiceException ex)
            {
                _logger.LogError(ex, "Service error during client credentials token acquisition: {ErrorCode} - {Message}", 
                    ex.ErrorCode, ex.Message);
                throw;
            }
            catch (MsalClientException ex)
            {
                _logger.LogError(ex, "Client error during client credentials token acquisition: {ErrorCode} - {Message}", 
                    ex.ErrorCode, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Acquire token using on-behalf-of flow (web APIs calling downstream APIs)
        /// </summary>
        public async Task<AuthenticationResult> AcquireTokenOnBehalfOfAsync(
            string[] scopes,
            UserAssertion userAssertion,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Acquiring token using on-behalf-of flow for scopes: {Scopes}", 
                    string.Join(", ", scopes));

                var result = await _app.AcquireTokenOnBehalfOf(scopes, userAssertion)
                    .ExecuteAsync(cancellationToken);

                _logger.LogInformation("On-behalf-of token acquired successfully for user: {Username}", 
                    result.Account?.Username ?? "Unknown");

                return result;
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogWarning("UI required for on-behalf-of token acquisition: {ErrorCode}", ex.ErrorCode);
                throw;
            }
            catch (MsalServiceException ex)
            {
                _logger.LogError(ex, "Service error during on-behalf-of token acquisition: {ErrorCode} - {Message}", 
                    ex.ErrorCode, ex.Message);
                throw;
            }
            catch (MsalClientException ex)
            {
                _logger.LogError(ex, "Client error during on-behalf-of token acquisition: {ErrorCode} - {Message}", 
                    ex.ErrorCode, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get application information for diagnostics
        /// </summary>
        public async Task<IEnumerable<IAccount>> GetCachedAccountsAsync()
        {
            try
            {
                var accounts = await _app.GetAccountsAsync();
                _logger.LogInformation("Retrieved {Count} cached accounts", accounts.Count());
                return accounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cached accounts");
                throw;
            }
        }

        /// <summary>
        /// Clear token cache for specific account
        /// </summary>
        public async Task ClearCacheAsync(IAccount account)
        {
            try
            {
                await _app.RemoveAsync(account);
                _logger.LogInformation("Cache cleared for account: {Username}", account.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache for account: {Username}", account.Username);
                throw;
            }
        }
    }

    /// <summary>
    /// Factory for creating and configuring MSAL confidential client applications
    /// </summary>
    public class ConfidentialClientFactory
    {
        private readonly SecretClient? _secretClient;
        private readonly ILogger<ConfidentialClientFactory> _logger;
        private readonly bool _useKeyVault;

        public ConfidentialClientFactory(
            ILogger<ConfidentialClientFactory> logger,
            SecretClient? secretClient = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _secretClient = secretClient;
            _useKeyVault = secretClient != null;
        }

        /// <summary>
        /// Create confidential client application with client secret
        /// </summary>
        public async Task<IConfidentialClientApplication> CreateWithClientSecretAsync(
            AzureAdConfiguration config, 
            string? clientSecret = null)
        {
            try
            {
                _logger.LogInformation("Creating confidential client application with client secret");

                string secret;
                if (_useKeyVault && _secretClient != null)
                {
                    // Retrieve client secret from Key Vault
                    var secretResponse = await _secretClient.GetSecretAsync(config.ClientSecretName);
                    secret = secretResponse.Value.Value;
                    _logger.LogInformation("Retrieved client secret from Key Vault");
                }
                else
                {
                    // Use provided client secret (for demo purposes)
                    secret = clientSecret ?? throw new ArgumentException("Client secret must be provided when Key Vault is not configured");
                    _logger.LogInformation("Using provided client secret");
                }

                var builder = ConfidentialClientApplicationBuilder
                    .Create(config.ClientId)
                    .WithClientSecret(secret)
                    .WithAuthority(config.Authority)
                    .WithLogging(LogCallback, LogLevel.Info, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                    .WithClientName("ModernConfidentialClientSample")
                    .WithClientVersion("1.0.0");

                // Enable regional endpoints for better performance
                if (config.EnableRegionalEndpoints)
                {
                    builder.WithAzureRegion(ConfidentialClientApplication.AttemptRegionDiscovery);
                    _logger.LogInformation("Regional endpoint discovery enabled");
                }

                var app = builder.Build();

                _logger.LogInformation("Confidential client application created successfully");
                return app;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create confidential client application");
                throw;
            }
        }

        /// <summary>
        /// Create confidential client application with certificate from Key Vault
        /// </summary>
        public async Task<IConfidentialClientApplication> CreateWithCertificateAsync(
            AzureAdConfiguration config, 
            string certificateName)
        {
            try
            {
                _logger.LogInformation("Creating confidential client application with certificate");

                // In production, you would retrieve the certificate from Key Vault
                // For this example, we'll show the pattern for certificate-based auth
                var certificate = await GetCertificateFromKeyVaultAsync(certificateName);

                var builder = ConfidentialClientApplicationBuilder
                    .Create(config.ClientId)
                    .WithCertificate(certificate)
                    .WithAuthority(config.Authority)
                    .WithLogging(LogCallback, LogLevel.Info, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                    .WithClientName("ModernConfidentialClientSample")
                    .WithClientVersion("1.0.0");

                // Enable regional endpoints for better performance
                if (config.EnableRegionalEndpoints)
                {
                    builder.WithAzureRegion(ConfidentialClientApplication.AttemptRegionDiscovery);
                }

                var app = builder.Build();
                _logger.LogInformation("Confidential client application created successfully with certificate");
                return app;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create confidential client application with certificate");
                throw;
            }
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
        /// Retrieve certificate from Key Vault (example implementation)
        /// </summary>
        private async Task<X509Certificate2> GetCertificateFromKeyVaultAsync(string certificateName)
        {
            // In a real implementation, you would retrieve the certificate from Key Vault
            // This is a placeholder to show the pattern
            await Task.Delay(1); // Simulate async operation
            throw new NotImplementedException("Certificate retrieval from Key Vault not implemented in this sample");
        }
    }

    /// <summary>
    /// Service collection extensions for dependency injection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add modern MSAL confidential client services to DI container
        /// </summary>
        public static IServiceCollection AddModernMsalConfidentialClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register configuration
            var azureAdConfig = configuration.GetSection("AzureAd").Get<AzureAdConfiguration>()
                ?? throw new InvalidOperationException("AzureAd configuration section is missing");
            
            services.AddSingleton(azureAdConfig);

            // Register Azure Key Vault client with Managed Identity (optional)
            if (!string.IsNullOrEmpty(azureAdConfig.KeyVaultUrl))
            {
                services.AddSingleton<SecretClient>(provider =>
                {
                    var credential = new DefaultAzureCredential();
                    return new SecretClient(new Uri(azureAdConfig.KeyVaultUrl), credential);
                });
            }

            // Register MSAL factory
            services.AddSingleton<ConfidentialClientFactory>();

            // Register the main service
            services.AddScoped<ModernConfidentialClientService>();

            return services;
        }
    }

    /// <summary>
    /// Microsoft Graph service for making API calls
    /// </summary>
    public class GraphService
    {
        private readonly ModernConfidentialClientService _authService;
        private readonly ILogger<GraphService> _logger;

        public GraphService(
            ModernConfidentialClientService authService,
            ILogger<GraphService> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get users from Microsoft Graph using application permissions
        /// </summary>
        public async Task<int> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var scopes = new[] { "https://graph.microsoft.com/.default" };
                var result = await _authService.AcquireTokenForClientAsync(scopes, cancellationToken);

                // Use the Microsoft Graph SDK or make direct HTTP calls
                _logger.LogInformation("Successfully acquired token for Microsoft Graph");
                _logger.LogInformation("Token expires at: {ExpiresOn}", result.ExpiresOn);

                // Simulate Graph API call
                await Task.Delay(500, cancellationToken);
                
                var userCount = new Random().Next(1, 100);
                _logger.LogInformation("Retrieved {UserCount} users from Microsoft Graph", userCount);
                
                return userCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Microsoft Graph API");
                throw;
            }
        }
    }

    /// <summary>
    /// Example daemon application using modern confidential client
    /// </summary>
    public class DaemonApplication : BackgroundService
    {
        private readonly GraphService _graphService;
        private readonly ILogger<DaemonApplication> _logger;

        public DaemonApplication(
            GraphService graphService,
            ILogger<DaemonApplication> logger)
        {
            _graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daemon application starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Daemon application cycle starting...");

                    // Call Microsoft Graph
                    var userCount = await _graphService.GetUsersAsync(stoppingToken);
                    
                    _logger.LogInformation("Daemon application cycle completed successfully. Found {UserCount} users.", userCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in daemon application cycle");
                }

                // Wait before next cycle
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Program entry point demonstrating modern configuration
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddConfiguration(configuration);
                    
                    // Add Azure Key Vault configuration provider if configured
                    var azureAdConfig = configuration.GetSection("AzureAd").Get<AzureAdConfiguration>();
                    
                    if (azureAdConfig?.KeyVaultUrl != null)
                    {
                        try
                        {
                            config.AddAzureKeyVault(
                                new Uri(azureAdConfig.KeyVaultUrl),
                                new DefaultAzureCredential());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Could not connect to Key Vault: {ex.Message}");
                        }
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    // Add modern MSAL confidential client services
                    services.AddModernMsalConfidentialClient(context.Configuration);
                    
                    // Add Graph service
                    services.AddScoped<GraphService>();
                    
                    // Add the daemon application
                    services.AddHostedService<DaemonApplication>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });

            var host = builder.Build();

            try
            {
                // For demo purposes, we'll run a single operation instead of the daemon
                if (args.Length > 0 && args[0] == "--demo")
                {
                    await RunDemoAsync(host);
                }
                else
                {
                    await host.RunAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = host.Services.GetService<ILogger<Program>>();
                logger?.LogError(ex, "Application terminated unexpectedly");
                throw;
            }
        }

        private static async Task RunDemoAsync(IHost host)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            
            try
            {
                // Get required services
                var config = host.Services.GetRequiredService<AzureAdConfiguration>();
                var factory = host.Services.GetRequiredService<ConfidentialClientFactory>();
                
                logger.LogInformation("Starting Modern Confidential Client Demo");
                
                // For demo purposes, we'll use a placeholder client secret
                // In production, this would come from Key Vault
                const string demoClientSecret = "demo-client-secret";
                
                // Create confidential client application
                var app = await factory.CreateWithClientSecretAsync(config, demoClientSecret);
                
                // Create service manually for demo
                var authService = new ModernConfidentialClientService(
                    app, 
                    host.Services.GetRequiredService<ILogger<ModernConfidentialClientService>>(),
                    config);
                
                // Try to acquire token
                var scopes = new[] { "https://graph.microsoft.com/.default" };
                
                logger.LogInformation("Attempting to acquire token for Microsoft Graph...");
                
                var result = await authService.AcquireTokenForClientAsync(scopes);
                
                logger.LogInformation("Token acquired successfully!");
                logger.LogInformation("Token expires: {ExpiresOn}", result.ExpiresOn);
                logger.LogInformation("Token type: {TokenType}", result.TokenType);
                
                // Show first 10 characters of token for verification
                logger.LogInformation("Token preview: {TokenPreview}...", result.AccessToken[..10]);
                
                logger.LogInformation("Demo completed successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Demo failed: {Message}", ex.Message);
            }
        }
    }
}
