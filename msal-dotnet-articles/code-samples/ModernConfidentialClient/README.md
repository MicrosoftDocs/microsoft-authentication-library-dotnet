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

- .NET 8.0 or later
- Visual Studio 2022 or later
- Azure subscription with access to:
  - Azure Active Directory
  - Azure Key Vault (optional, for production scenarios)
- Azure CLI or PowerShell for setup

## Setup Instructions

### 1. Azure AD App Registration

1. Go to the [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Fill in the following:
   - **Name**: `ModernConfidentialClientSample`
   - **Supported account types**: `Accounts in this organizational directory only`
   - **Redirect URI**: Leave blank for daemon applications
5. Click **Register**
6. Copy the **Application (client) ID** and **Directory (tenant) ID**

### 2. Create Client Secret

1. In your app registration, go to **Certificates & secrets**
2. Click **New client secret**
3. Add a description and choose an expiration period
4. Click **Add**
5. **Important**: Copy the secret value immediately (it won't be shown again)

### 3. Configure Application Permissions

1. In your app registration, go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph**
4. Choose **Application permissions** (not delegated)
5. Add the following permissions:
   - `User.Read.All` (for reading user information)
   - `Directory.Read.All` (for reading directory data)
6. Click **Add permissions**
7. **Important**: Click **Grant admin consent** for your organization

### 4. Configure the Application

#### Option A: Basic Configuration (for testing)

1. Open `appsettings.json`
2. Replace the placeholder values:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-actual-tenant-id",
    "ClientId": "your-actual-client-id",
    "KeyVaultUrl": "",
    "ClientSecretName": "AppClientSecret",
    "EnableRegionalEndpoints": true
  }
}
```

#### Option B: Production Configuration with Key Vault

1. **Create Azure Key Vault**:
   ```bash
   az keyvault create --name "your-keyvault" --resource-group "your-rg" --location "East US"
   ```

2. **Store client secret in Key Vault**:
   ```bash
   az keyvault secret set --vault-name "your-keyvault" --name "AppClientSecret" --value "your-client-secret"
   ```

3. **Configure Managed Identity access**:
   ```bash
   # For Azure VM or App Service
   az keyvault set-policy --name "your-keyvault" --object-id "your-managed-identity-id" --secret-permissions get
   
   # For local development (using your user account)
   az keyvault set-policy --name "your-keyvault" --upn "your-email@domain.com" --secret-permissions get
   ```

4. **Update appsettings.json**:
   ```json
   {
     "AzureAd": {
       "Instance": "https://login.microsoftonline.com/",
       "TenantId": "your-actual-tenant-id",
       "ClientId": "your-actual-client-id",
       "KeyVaultUrl": "https://your-keyvault.vault.azure.net/",
       "ClientSecretName": "AppClientSecret",
       "EnableRegionalEndpoints": true
     }
   }
   ```

## Running the Application

### Option 1: Run as Daemon Service
```bash
dotnet restore
dotnet build
dotnet run
```

### Option 2: Run Demo Mode
```bash
dotnet run -- --demo
```

## Authentication Methods

### Method 1: Azure Key Vault (Recommended for Production)

When Key Vault is configured, the application will:
1. Use `DefaultAzureCredential` to authenticate to Azure
2. Retrieve the client secret from Key Vault
3. Use the secret to authenticate the confidential client

### Method 2: Environment Variables (For Testing)

Set environment variables:
```bash
# Windows
set AZURE_CLIENT_SECRET=your-client-secret

# Linux/macOS
export AZURE_CLIENT_SECRET=your-client-secret
```

### Method 3: Direct Configuration (Development Only)

For development/testing, you can pass the client secret directly in the code (not recommended for production).

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
- **Certificate-based**: Enhanced security with certificates (framework included)

## Understanding the Code Structure

### Configuration Model
```csharp
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
```

### Main Authentication Service
The `ModernConfidentialClientService` class provides:
- Client credentials flow for daemon applications
- On-behalf-of flow for web APIs
- Token cache management
- Comprehensive error handling

### Factory Pattern
The `ConfidentialClientFactory` creates and configures MSAL applications with:
- Key Vault integration
- Certificate support
- Modern logging
- Regional optimization

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

## Troubleshooting

### Common Issues

1. **"AADSTS700016: Application with identifier 'xxx' was not found"**
   - Verify the Client ID is correct
   - Ensure the app registration exists in the correct tenant

2. **"AADSTS7000215: Invalid client secret is provided"**
   - Verify the client secret is correct and not expired
   - Check Key Vault permissions if using Key Vault

3. **"AADSTS65001: The user or administrator has not consented to use the application"**
   - Grant admin consent for the application permissions
   - Ensure the correct scopes are requested

4. **Key Vault access denied**
   - Verify Managed Identity has access to Key Vault
   - Check Key Vault access policies
   - Ensure you're authenticated with Azure CLI for local development

### Debug Logging

To enable verbose logging, update `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.Identity": "Verbose"
    }
  }
}
```

## Testing Scenarios

Test the application with:
- Different permission scopes
- Token expiration scenarios
- Network connectivity issues
- Key Vault authentication failures
- Various Azure regions

## Deployment Options

### Azure App Service
- Enable Managed Identity
- Configure Key Vault access
- Set application settings

### Azure Container Instances
- Use Managed Identity
- Mount Key Vault secrets as volumes
- Configure environment variables

### Azure Kubernetes Service
- Use Azure AD Pod Identity
- Configure Key Vault CSI driver
- Set up service accounts

## References

- [MSAL.NET Confidential Client Documentation](https://learn.microsoft.com/azure/active-directory/develop/msal-net-confidential-client)
- [Azure Key Vault Integration](https://learn.microsoft.com/azure/key-vault/general/overview)
- [Managed Identity](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [Client Credentials Flow](https://learn.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)
- [On-Behalf-Of Flow](https://learn.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow)
- [DefaultAzureCredential](https://learn.microsoft.com/dotnet/api/azure.identity.defaultazurecredential)
