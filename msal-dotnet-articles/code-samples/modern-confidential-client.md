# Modern Confidential Client Application with Azure Key Vault

This sample demonstrates the latest MSAL.NET best practices for confidential client applications (web apps, web APIs, daemon apps) using Azure Key Vault for secure credential management and modern configuration patterns.

## Features Demonstrated

- **Azure Key Vault Integration**: Secure credential storage and retrieval
- **Managed Identity Authentication**: Passwordless authentication to Azure services
- **Modern dependency injection**: Integration with .NET's built-in DI container
- **Comprehensive error handling**: Specific handling for confidential client scenarios
- **Regional endpoint optimization**: Automatic region discovery for improved performance
- **Structured logging**: Integration with Microsoft.Extensions.Logging
- **Client credentials flow**: Secure daemon application authentication
- **On-behalf-of flow**: Secure API-to-API authentication

## Prerequisites

- Azure subscription with Key Vault access
- .NET 6.0 or later
- Azure CLI or PowerShell for setup
- Managed Identity or Service Principal for Key Vault access

## NuGet Packages Required

```xml
<PackageReference Include="Microsoft.Identity.Client" Version="4.61.3" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.AzureKeyVault" Version="8.0.0" />
<PackageReference Include="Azure.Identity" Version="1.10.4" />
<PackageReference Include="Azure.Security.KeyVault.Secrets" Version="4.5.0" />
<PackageReference Include="Azure.Extensions.AspNetCore.Configuration.Secrets" Version="1.2.2" />
```

## Configuration (appsettings.json)

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "KeyVaultUrl": "https://your-keyvault.vault.azure.net/",
    "ClientSecretName": "AppClientSecret",
    "EnableRegionalEndpoints": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Identity": "Information"
    }
  }
}
```

## Complete Code Sample

```csharp
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ModernConfidentialClientSample
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
        private readonly SecretClient _secretClient;
        private readonly ILogger<ConfidentialClientFactory> _logger;

        public ConfidentialClientFactory(SecretClient secretClient, ILogger<ConfidentialClientFactory> logger)
        {
            _secretClient = secretClient ?? throw new ArgumentNullException(nameof(secretClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create confidential client application with client secret from Key Vault
        /// </summary>
        public async Task<IConfidentialClientApplication> CreateWithClientSecretAsync(AzureAdConfiguration config)
        {
            try
            {
                _logger.LogInformation("Creating confidential client application with client secret from Key Vault");

                // Retrieve client secret from Key Vault
                var secretResponse = await _secretClient.GetSecretAsync(config.ClientSecretName);
                var clientSecret = secretResponse.Value.Value;

                var builder = ConfidentialClientApplicationBuilder
                    .Create(config.ClientId)
                    .WithClientSecret(clientSecret)
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

                // Use distributed token cache in production
                // app.AddDistributedTokenCache(services => { ... });

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
                _logger.LogInformation("Creating confidential client application with certificate from Key Vault");

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

            // Register Azure Key Vault client with Managed Identity
            services.AddSingleton<SecretClient>(provider =>
            {
                var credential = new DefaultAzureCredential();
                return new SecretClient(new Uri(azureAdConfig.KeyVaultUrl), credential);
            });

            // Register MSAL factory
            services.AddSingleton<ConfidentialClientFactory>();

            // Register MSAL confidential client application
            services.AddSingleton<IConfidentialClientApplication>(async provider =>
            {
                var factory = provider.GetRequiredService<ConfidentialClientFactory>();
                var config = provider.GetRequiredService<AzureAdConfiguration>();
                return await factory.CreateWithClientSecretAsync(config);
            });

            // Register the main service
            services.AddScoped<ModernConfidentialClientService>();

            return services;
        }
    }

    /// <summary>
    /// Example daemon application using modern confidential client
    /// </summary>
    public class DaemonApplication : BackgroundService
    {
        private readonly ModernConfidentialClientService _authService;
        private readonly ILogger<DaemonApplication> _logger;
        private readonly string[] _scopes = { "https://graph.microsoft.com/.default" };

        public DaemonApplication(
            ModernConfidentialClientService authService,
            ILogger<DaemonApplication> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Daemon application cycle starting...");

                    // Acquire token for Microsoft Graph
                    var result = await _authService.AcquireTokenForClientAsync(_scopes, stoppingToken);

                    // Use the token to call Microsoft Graph or other APIs
                    await CallMicrosoftGraphAsync(result.AccessToken, stoppingToken);

                    _logger.LogInformation("Daemon application cycle completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in daemon application cycle");
                }

                // Wait before next cycle
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task CallMicrosoftGraphAsync(string accessToken, CancellationToken cancellationToken)
        {
            // Example: Call Microsoft Graph API
            // In a real application, you would use the Microsoft Graph SDK
            _logger.LogInformation("Calling Microsoft Graph API with access token");
            
            // Placeholder for actual Graph API call
            await Task.Delay(100, cancellationToken);
            
            _logger.LogInformation("Microsoft Graph API call completed");
        }
    }

    /// <summary>
    /// Program entry point demonstrating modern configuration
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Add Azure Key Vault configuration provider
                    var builtConfig = config.Build();
                    var azureAdConfig = builtConfig.GetSection("AzureAd").Get<AzureAdConfiguration>();
                    
                    if (azureAdConfig?.KeyVaultUrl != null)
                    {
                        config.AddAzureKeyVault(
                            new Uri(azureAdConfig.KeyVaultUrl),
                            new DefaultAzureCredential());
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    // Add modern MSAL confidential client services
                    services.AddModernMsalConfidentialClient(context.Configuration);
                    
                    // Add the daemon application
                    services.AddHostedService<DaemonApplication>();
                });

            var host = builder.Build();

            try
            {
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                var logger = host.Services.GetService<ILogger<Program>>();
                logger?.LogError(ex, "Application terminated unexpectedly");
                throw;
            }
        }
    }
}
```

## Key Features Explained

### 1. **Azure Key Vault Integration**
```csharp
services.AddSingleton<SecretClient>(provider =>
{
    var credential = new DefaultAzureCredential();
    return new SecretClient(new Uri(azureAdConfig.KeyVaultUrl), credential);
});
```
- Secure credential storage in Azure Key Vault
- Managed Identity authentication for passwordless access
- Automatic credential rotation support

### 2. **Modern Dependency Injection**
```csharp
public static IServiceCollection AddModernMsalConfidentialClient(
    this IServiceCollection services,
    IConfiguration configuration)
```
- Full integration with .NET's built-in DI container
- Proper service lifetime management
- Configuration-based setup

### 3. **Regional Optimization**
```csharp
builder.WithAzureRegion(ConfidentialClientApplication.AttemptRegionDiscovery);
```
- Automatic region discovery for improved performance
- Reduced latency through geographic optimization

### 4. **Comprehensive Error Handling**
- Specific handling for different MSAL exception types
- Structured logging for debugging and monitoring
- Graceful degradation and retry patterns

### 5. **Multiple Authentication Flows**
- **Client Credentials**: For daemon applications
- **On-Behalf-Of**: For web APIs calling downstream services
- **Certificate-based**: Enhanced security with certificates

## Setup Instructions

### 1. Azure Key Vault Setup
```bash
# Create Key Vault
az keyvault create --name "your-keyvault" --resource-group "your-rg" --location "East US"

# Store client secret
az keyvault secret set --vault-name "your-keyvault" --name "AppClientSecret" --value "your-client-secret"

# Grant access to Managed Identity
az keyvault set-policy --name "your-keyvault" --object-id "your-managed-identity-id" --secret-permissions get
```

### 2. Azure AD App Registration
```bash
# Create app registration
az ad app create --display-name "ModernConfidentialClientSample" --sign-in-audience "AzureADMyOrg"

# Add required permissions
az ad app permission add --id "your-app-id" --api "00000003-0000-0000-c000-000000000000" --api-permissions "e1fe6dd8-ba31-4d61-89e7-88639da4683d=Role"
```

## Security Considerations

1. **Key Vault Access**: Use Managed Identity or Service Principal with minimal permissions
2. **Client Secret Rotation**: Implement automatic rotation using Key Vault
3. **Certificate Authentication**: Prefer certificates over client secrets for enhanced security
4. **Scope Management**: Request only necessary permissions following principle of least privilege
5. **Token Caching**: Use distributed cache in production for scalability

## Performance Optimizations

1. **Regional Endpoints**: Automatic region discovery for reduced latency
2. **Token Caching**: Efficient token reuse across requests
3. **Connection Pooling**: MSAL.NET automatically pools HTTP connections
4. **Async Patterns**: Non-blocking operations throughout

## Production Considerations

1. **Distributed Token Cache**: Implement Redis or SQL Server token cache for multi-instance scenarios
2. **Health Checks**: Add health checks for authentication service
3. **Monitoring**: Implement comprehensive logging and monitoring
4. **Resilience**: Add circuit breakers and retry policies
5. **Configuration**: Use Azure App Configuration for centralized settings

## References

- [MSAL.NET Confidential Client Documentation](https://learn.microsoft.com/azure/active-directory/develop/msal-net-confidential-client)
- [Azure Key Vault Integration](https://learn.microsoft.com/azure/key-vault/general/overview)
- [Managed Identity](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [Client Credentials Flow](https://learn.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)
- [On-Behalf-Of Flow](https://learn.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow)
