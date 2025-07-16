---
title: Choose the right MSAL.NET authentication approach
description: A decision tree guide to help you choose the right authentication approach for your application using MSAL.NET.
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 01/15/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: conceptual
ms.custom: devx-track-csharp, aaddev
#Customer intent: As a developer, I want to understand which authentication approach is best for my specific application scenario.
---

# Choose the right MSAL.NET authentication approach

This guide helps you choose the right authentication approach for your application using MSAL.NET.

## Decision tree

```
START: What type of application are you building?
│
├── Web Application (ASP.NET Core, ASP.NET Framework)
│   │
│   ├── Do you need to call APIs on behalf of users?
│   │   ├── YES → Use Microsoft.Identity.Web
│   │   └── NO → Use Microsoft.Identity.Web (sign-in only)
│   │
│   └── Is this a Single Page Application (SPA)?
│       ├── YES → Use MSAL.js (JavaScript)
│       └── NO → Use Microsoft.Identity.Web
│
├── Desktop Application (WPF, WinForms, Console)
│   │
│   ├── Do you need interactive authentication?
│   │   ├── YES → Use IPublicClientApplication with AcquireTokenInteractive
│   │   └── NO → Consider service account or client credentials
│   │
│   └── Are you running on Windows?
│       ├── YES → Consider Web Account Manager (WAM)
│       └── NO → Use standard interactive flow
│
├── Mobile Application (iOS, Android, UWP)
│   │
│   ├── Platform?
│   │   ├── iOS → Use IPublicClientApplication with broker
│   │   ├── Android → Use IPublicClientApplication with broker
│   │   └── UWP → Use IPublicClientApplication (deprecated)
│   │
│   └── Note: UWP, Xamarin iOS, and Xamarin Android are deprecated
│
├── Service/Daemon Application
│   │
│   ├── Authentication method?
│   │   ├── Client Secret → Use IConfidentialClientApplication
│   │   ├── Client Certificate → Use IConfidentialClientApplication
│   │   └── Managed Identity → Use Azure.Identity
│   │
│   └── Use AcquireTokenForClient
│
└── Web API
    │
    ├── Do you need to call downstream APIs?
    │   ├── YES → Use Microsoft.Identity.Web with On-Behalf-Of
    │   └── NO → Use Microsoft.Identity.Web (token validation only)
    │
    └── Consider using Azure API Management for additional security
```

## Choose the right library

### Microsoft.Identity.Web (Recommended for web apps)

**Use when:**
- Building ASP.NET Core web applications
- Building ASP.NET Framework web applications
- Need to sign in users and call APIs
- Want simplified configuration and setup

**Benefits:**
- Simplified setup and configuration
- Built-in token cache management
- Automatic token refresh
- Integration with ASP.NET Core dependency injection
- Support for multiple authentication schemes

**Example:**
```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddMicrosoftGraph(Configuration.GetSection("MicrosoftGraph"))
        .AddInMemoryTokenCaches();
}
```

### Microsoft.Identity.Client (Core library)

**Use when:**
- Building desktop applications
- Building mobile applications (with limitations)
- Need fine-grained control over authentication flow
- Building daemon/service applications

**Benefits:**
- Maximum flexibility and control
- Cross-platform support
- Advanced features like custom token cache
- Support for all authentication flows

**Example:**
```csharp
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(authority)
    .WithDefaultRedirectUri()
    .Build();

var result = await app.AcquireTokenInteractive(scopes)
    .ExecuteAsync();
```

## Authentication flow decision matrix

| Scenario | Recommended Flow | Library | Code Example |
|----------|------------------|---------|--------------|
| Web app signing in users | Authorization Code | Microsoft.Identity.Web | `AddMicrosoftIdentityWebApp()` |
| Web app calling API | Authorization Code + OBO | Microsoft.Identity.Web | `EnableTokenAcquisitionToCallDownstreamApi()` |
| Desktop app (interactive) | Authorization Code + PKCE | Microsoft.Identity.Client | `AcquireTokenInteractive()` |
| Desktop app (non-interactive) | Integrated Windows Auth | Microsoft.Identity.Client | `AcquireTokenByIntegratedWindowsAuth()` |
| Mobile app | Authorization Code + PKCE | Microsoft.Identity.Client | `AcquireTokenInteractive()` |
| Daemon/Service app | Client Credentials | Microsoft.Identity.Client | `AcquireTokenForClient()` |
| Web API calling another API | On-Behalf-Of | Microsoft.Identity.Web | `EnableTokenAcquisitionToCallDownstreamApi()` |

## Platform-specific considerations

### Windows Desktop Applications

**Choose WAM when:**
- Running on Windows 10 or later
- Want native Windows authentication experience
- Need single sign-on with Windows accounts
- Users are domain-joined or Azure AD joined

```csharp
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(authority)
    .WithBroker(true) // Enable WAM
    .Build();
```

**Choose standard authentication when:**
- Need cross-platform compatibility
- Running on older Windows versions
- Want consistent behavior across platforms

### Mobile Applications

**iOS:**
```csharp
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(authority)
    .WithIosKeychainSecurityGroup("com.yourcompany.yourapp")
    .WithBroker(true)
    .Build();
```

**Android:**
```csharp
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(authority)
    .WithParentActivityOrWindow(() => Platform.CurrentActivity)
    .WithBroker(true)
    .Build();
```

### Web Applications

**ASP.NET Core:**
```csharp
// Program.cs (.NET 6+)
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
```

**ASP.NET Framework:**
```csharp
// Startup.cs
public void ConfigureAuth(IAppBuilder app)
{
    app.UseMicrosoftIdentityWebApp(
        Configuration.GetSection("AzureAd"),
        OpenIdConnectDefaults.AuthenticationScheme);
}
```

## Environment-specific decisions

### Development Environment

**Choose during development:**
- Enable comprehensive logging
- Use localhost redirect URIs
- Store secrets in user secrets or environment variables
- Use development-specific tenant if available

```csharp
#if DEBUG
    .WithLogging((level, message, containsPii) =>
    {
        Console.WriteLine($"[{level}] {message}");
    }, LogLevel.Verbose, enablePiiLogging: true)
#endif
```

### Production Environment

**Choose for production:**
- Use HTTPS redirect URIs
- Implement proper secret management (Azure Key Vault)
- Enable monitoring and alerting
- Use distributed token caching for web apps
- Configure regional endpoints for better performance

```csharp
var app = ConfidentialClientApplicationBuilder
    .Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(authority)
    .WithAzureRegion(ConfidentialClientApplication.AttemptRegionDiscovery)
    .Build();
```

## Security considerations

### Public vs. Confidential Client

**Use Public Client when:**
- Application cannot securely store secrets
- Desktop applications
- Mobile applications
- Single-page applications (use MSAL.js)

**Use Confidential Client when:**
- Application can securely store secrets
- Web applications
- Web APIs
- Daemon/service applications

### Authentication Method Selection

**Client Secret:**
- Simple to implement
- Suitable for development and testing
- Less secure than certificates
- Has expiration dates to manage

**Client Certificate:**
- More secure than client secrets
- Recommended for production
- Requires certificate management
- Better for compliance scenarios

**Managed Identity:**
- Most secure option
- No credentials to manage
- Only available in Azure
- Recommended for Azure-hosted applications

## Performance considerations

### Token Cache Strategy

**In-Memory Cache:**
- Fastest access
- Lost on application restart
- Good for development and testing

**Distributed Cache:**
- Survives application restarts
- Required for web application scaling
- Redis, SQL Server, or Cosmos DB options

**File-based Cache:**
- Suitable for desktop applications
- Persists across application restarts
- Requires proper file system permissions

### Regional Endpoints

Use regional endpoints for better performance:

```csharp
var app = ConfidentialClientApplicationBuilder
    .Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(authority)
    .WithAzureRegion(ConfidentialClientApplication.AttemptRegionDiscovery)
    .Build();
```

## Common scenarios and solutions

### Scenario 1: ASP.NET Core web app that calls Microsoft Graph

**Solution:** Microsoft.Identity.Web

```csharp
// Program.cs
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    .AddInMemoryTokenCaches();
```

### Scenario 2: Windows desktop app with single sign-on

**Solution:** MSAL.NET with WAM

```csharp
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(authority)
    .WithBroker(true)
    .Build();
```

### Scenario 3: Console app running as a service

**Solution:** MSAL.NET with client credentials

```csharp
var app = ConfidentialClientApplicationBuilder
    .Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(authority)
    .Build();

var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
```

### Scenario 4: Mobile app with broker authentication

**Solution:** MSAL.NET with broker

```csharp
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(authority)
    .WithBroker(true)
    .Build();
```

## Decision checklist

Before implementing authentication, ask yourself:

- [ ] What type of application am I building?
- [ ] Do I need user authentication or application authentication?
- [ ] What platforms do I need to support?
- [ ] Do I need to call APIs on behalf of users?
- [ ] What are my security requirements?
- [ ] What are my performance requirements?
- [ ] Do I need offline access?
- [ ] What's my deployment environment?

## Next steps

Based on your decisions:

1. **For web applications**: [Microsoft.Identity.Web documentation](../microsoft-identity-web/index.md)
2. **For desktop applications**: [Desktop authentication guide](../acquiring-tokens/desktop-mobile/acquiring-tokens-interactively.md)
3. **For mobile applications**: [Mobile authentication guide](../acquiring-tokens/desktop-mobile/mobile-applications.md)
4. **For service applications**: [Client credentials flow](../acquiring-tokens/web-apps-apis/client-credential-flows.md)
5. **For migration**: [ADAL to MSAL migration guide](msal-net-migration.md)

## Get help

If you're still unsure which approach to choose:

- [Browse code samples](/azure/active-directory/develop/sample-v2-code)
- [Ask questions on Stack Overflow](https://stackoverflow.com/questions/tagged/msal)
- [Review the decision diagram](../media/msal-net-migration/decision-diagram.png)
- [Check the scenario documentation](../getting-started/scenarios.md)
