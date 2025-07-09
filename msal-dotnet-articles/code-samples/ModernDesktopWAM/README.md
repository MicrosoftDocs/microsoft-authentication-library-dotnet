# Modern Desktop Application with WAM (Windows Authentication Manager)

This sample demonstrates the latest MSAL.NET best practices for desktop applications using the Windows Authentication Manager (WAM) for enhanced security and user experience.

## Features Demonstrated

- **Windows Authentication Manager (WAM)**: Native Windows authentication experience
- **Modern logging**: Structured logging with proper categorization
- **Secure token caching**: Memory-based token cache for desktop apps
- **Regional endpoint optimization**: Automatic region discovery for improved performance
- **Comprehensive error handling**: Specific handling for common MSAL scenarios
- **Modern async patterns**: Proper async/await usage throughout
- **Dependency injection**: Full integration with .NET's built-in DI container

## Prerequisites

- Windows 10 version 1903 or later (for WAM support)
- .NET 8.0 or later
- Visual Studio 2022 or later
- Azure AD application registration

## Setup Instructions

### 1. Azure AD App Registration

1. Go to the [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Fill in the following:
   - **Name**: `ModernDesktopWAMSample`
   - **Supported account types**: Choose based on your needs
   - **Redirect URI**: Select **Public client/native** and enter `http://localhost`
5. Click **Register**
6. Copy the **Application (client) ID** for use in configuration

### 2. Configure Application Permissions

1. In your app registration, go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph**
4. Choose **Delegated permissions**
5. Add the following permissions:
   - `User.Read`
   - `Files.Read` (or other permissions as needed)
6. Click **Add permissions**

### 3. Configure the Application

1. Open `appsettings.json`
2. Replace `"your-client-id-here"` with your actual Client ID
3. Optionally, change the `TenantId` from `"common"` to your specific tenant ID for single-tenant applications

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "common",
    "ClientId": "your-actual-client-id-here"
  }
}
```

## Running the Application

1. Open a command prompt in the project directory
2. Run the following commands:

```bash
dotnet restore
dotnet build
dotnet run
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

### 6. **Dependency Injection**
- Full integration with .NET's built-in DI container
- Proper service lifetime management
- Configuration-based setup

## Understanding the Code Structure

### Configuration Model
```csharp
public class AzureAdConfiguration
{
    public string Instance { get; set; } = "https://login.microsoftonline.com/";
    public string TenantId { get; set; } = "common";
    public string ClientId { get; set; } = string.Empty;
    public string Authority => $"{Instance.TrimEnd('/')}/{TenantId}";
}
```

### Main Authentication Service
The `ModernDesktopAuthenticationService` class provides:
- Silent token acquisition
- Interactive token acquisition with WAM
- Complete authentication flow with fallback
- User sign-out functionality

### Factory Pattern
The `PublicClientFactory` creates and configures MSAL applications with modern settings.

## Security Considerations

1. **Client ID**: Replace the placeholder with your actual Azure AD application client ID
2. **Redirect URI**: Configure matching redirect URI in Azure AD app registration
3. **Scopes**: Only request necessary scopes following principle of least privilege
4. **Token Storage**: Tokens are automatically cached securely by MSAL.NET
5. **PII Logging**: Disabled in production to prevent sensitive data exposure

## Performance Optimizations

1. **WAM**: Faster authentication through native Windows integration
2. **Regional Endpoints**: Reduced latency through geographic optimization
3. **Silent Authentication**: Minimize user interruption through cached tokens
4. **Connection Pooling**: MSAL.NET automatically pools HTTP connections

## Troubleshooting

### Common Issues

1. **WAM not available**: Ensure you're running on Windows 10 version 1903 or later
2. **Authentication fails**: Check that your client ID and redirect URI are correctly configured
3. **Token acquisition fails**: Verify the requested scopes are configured in your app registration
4. **Build errors**: Ensure you have the required NuGet packages installed

### Debug Logging

To enable verbose logging, change the log level in `appsettings.json`:

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
- Users who have MFA enabled
- Users who need to consent to permissions
- Users who have conditional access policies
- Different account types (work, school, personal)

## References

- [MSAL.NET Documentation](https://learn.microsoft.com/azure/active-directory/develop/msal-net-overview)
- [Windows Authentication Manager (WAM)](https://learn.microsoft.com/azure/active-directory/develop/msal-net-wam)
- [Regional Endpoints](https://learn.microsoft.com/azure/active-directory/develop/msal-net-regional-endpoints)
- [Public Client Applications](https://learn.microsoft.com/azure/active-directory/develop/msal-client-applications)
