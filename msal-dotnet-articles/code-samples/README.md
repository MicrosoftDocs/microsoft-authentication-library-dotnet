# Modern MSAL.NET Code Samples

This directory contains modern, production-ready code samples demonstrating the latest MSAL.NET features and best practices.

## Available Samples

### 1. [Modern Desktop Application with WAM](ModernDesktopWAM/)
**Target Audience**: Desktop application developers  
**Project Type**: .NET 8 Console Application with WPF/WinForms support  
**Key Features**:
- Windows Authentication Manager (WAM) integration
- Modern logging with structured logging
- Regional endpoint optimization
- Comprehensive error handling
- Async/await patterns throughout
- Dependency injection with .NET's built-in DI container

**Best For**: Windows desktop applications requiring modern authentication with enhanced security and user experience.

### 2. [Modern Confidential Client Application](ModernConfidentialClient/)
**Target Audience**: Web application, web API, and daemon application developers  
**Project Type**: .NET 8 Console Application (daemon service)  
**Key Features**:
- Azure Key Vault integration for secure credential management
- Managed Identity authentication
- Dependency injection with .NET's built-in DI container
- Client credentials and on-behalf-of flows
- Regional endpoint optimization
- Production-ready patterns with comprehensive error handling

**Best For**: Server-side applications, web APIs, and daemon applications requiring secure, scalable authentication.

## Quick Start

Each sample is a complete, runnable .NET project:

1. **Choose your sample**: Navigate to the appropriate directory
2. **Read the README**: Each project has detailed setup instructions
3. **Configure Azure AD**: Set up your Azure AD app registration
4. **Update configuration**: Modify `appsettings.json` with your values
5. **Run the sample**: Use `dotnet run` to execute

```bash
# Example: Run the Desktop WAM sample
cd ModernDesktopWAM
dotnet restore
dotnet build
dotnet run

# Example: Run the Confidential Client sample in demo mode
cd ModernConfidentialClient
dotnet restore
dotnet build
dotnet run -- --demo
```

## Common Modern Patterns Demonstrated

### Security Best Practices
- **No hardcoded credentials**: All samples use secure credential storage
- **Minimal permissions**: Following principle of least privilege
- **PII protection**: Logging configured to prevent sensitive data exposure
- **Certificate-based authentication**: Enhanced security over client secrets

### Performance Optimizations
- **Regional endpoints**: Automatic region discovery for reduced latency
- **Token caching**: Efficient token reuse and management
- **Connection pooling**: Optimal HTTP connection management
- **Async patterns**: Non-blocking operations throughout

### Modern Development Practices
- **Dependency injection**: Full integration with .NET's DI container
- **Structured logging**: Integration with Microsoft.Extensions.Logging
- **Configuration management**: Modern configuration patterns
- **Error handling**: Comprehensive exception handling with proper logging

## Prerequisites

All samples require:
- .NET 6.0 or later
- Visual Studio 2022 or later
- Azure subscription (for cloud features)
- Basic understanding of Azure Active Directory

## Getting Started

1. Choose the sample that matches your application type
2. Follow the setup instructions in each sample
3. Replace placeholder values with your actual Azure AD configuration
4. Test in a development environment before deploying to production

## Additional Resources

- [MSAL.NET Documentation](https://learn.microsoft.com/azure/active-directory/develop/msal-net-overview)
- [Azure Active Directory Documentation](https://learn.microsoft.com/azure/active-directory/)
- [Microsoft Authentication Library (MSAL) Overview](https://learn.microsoft.com/azure/active-directory/develop/msal-overview)
- [Best Practices for Azure AD Authentication](https://learn.microsoft.com/azure/active-directory/develop/authentication-best-practices)

## Contributing

When adding new samples to this directory:
1. Follow the established naming convention: `Modern[Scenario][Feature]` (e.g., `ModernWebAppIdentity`)
2. Create a complete .NET project with:
   - Proper `.csproj` file with latest package references
   - Comprehensive README with setup instructions
   - `appsettings.json` template with all required configurations
   - Example code demonstrating modern patterns
3. Include comprehensive error handling and logging
4. Demonstrate current best practices and latest MSAL.NET features
5. Provide clear setup instructions and prerequisites
6. Include security considerations and performance optimizations
7. Update this index file with the new sample
8. Ensure all samples can be built and run successfully

### Code Quality Standards
- Use modern C# features and patterns
- Follow .NET coding conventions
- Include comprehensive XML documentation
- Implement proper async/await patterns
- Use structured logging with Microsoft.Extensions.Logging
- Include proper error handling and graceful degradation
- Demonstrate security best practices (no hardcoded secrets, proper redirect URIs, etc.)

## Modernization Checklist

If you're updating existing MSAL.NET code, use this checklist to ensure you're following the latest best practices:

### ✅ Security & Authentication
- [ ] Replace `WithDefaultRedirectUri()` with explicit redirect URIs
- [ ] Remove `SecureString` usage in favor of regular strings
- [ ] Replace simple `WithBroker(true)` with modern `BrokerOptions`
- [ ] Avoid username/password flows (`AcquireTokenByUsernamePassword`, `AcquireTokenByIntegratedWindowsAuth`)
- [ ] Use Azure Key Vault for credential storage instead of hardcoded secrets
- [ ] Implement certificate-based authentication where possible

### ✅ Performance & Reliability
- [ ] Enable regional endpoints with `WithAzureRegion(ConfidentialClientApplication.AttemptRegionDiscovery)`
- [ ] Implement proper retry policies with exponential backoff
- [ ] Use distributed token caching for multi-instance scenarios
- [ ] Ensure single instance of MSAL client applications per application

### ✅ Logging & Monitoring
- [ ] Replace console logging with structured logging (Microsoft.Extensions.Logging)
- [ ] Integrate with Application Insights for production monitoring
- [ ] Set `enablePiiLogging: false` for production environments
- [ ] Implement proper log correlation and tracking

### ✅ Modern Development Practices
- [ ] Use dependency injection for MSAL client instances
- [ ] Implement proper async/await patterns throughout
- [ ] Use modern exception handling with specific MSAL exception types
- [ ] Integrate with .NET's configuration system (`IConfiguration`)
- [ ] Use Microsoft.Identity.Web for web applications

### ✅ Platform-Specific Optimizations
- [ ] **Desktop**: Use WAM (Windows Authentication Manager) for Windows apps
- [ ] **Web**: Use Microsoft.Identity.Web instead of direct MSAL.NET
- [ ] **Mobile**: Use system browsers instead of embedded web views
- [ ] **Cloud**: Use Managed Identity for Azure-hosted applications

### ✅ Documentation & URLs
- [ ] Update any `docs.microsoft.com` URLs to `learn.microsoft.com`
- [ ] Review and update API reference links
- [ ] Update NuGet package references to latest versions
- [ ] Ensure all code examples follow current patterns
