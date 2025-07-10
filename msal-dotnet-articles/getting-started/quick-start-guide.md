---
title: MSAL.NET Quick Start Guide
description: Get up and running with MSAL.NET in 5 minutes. A step-by-step guide for new developers.
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 01/15/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: quickstart
ms.custom: devx-track-csharp, aaddev
#Customer intent: As a new developer, I want to quickly get MSAL.NET working in my application with minimal setup.
---

# MSAL.NET Quick Start Guide

Get authentication working in your .NET application in just 5 minutes with this step-by-step guide.

## Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/)
- An Azure account with an active subscription ([create one for free](https://azure.microsoft.com/free/))

## Step 1: Register Your Application

1. Sign in to the [Azure portal](https://portal.azure.com)
2. Navigate to **Microsoft Entra ID** > **App registrations**
3. Click **New registration**
4. Fill in:
   - **Name**: `MyMSALApp`
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: `http://localhost` (for desktop apps)
5. Click **Register**
6. Note down the **Application (client) ID** and **Directory (tenant) ID**

## Step 2: Create a New Console Application

```bash
dotnet new console -n MSALQuickStart
cd MSALQuickStart
```

## Step 3: Install MSAL.NET

```bash
dotnet add package Microsoft.Identity.Client
```

## Step 4: Add Authentication Code

Replace the contents of `Program.cs` with:

```csharp
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace MSALQuickStart
{
    class Program
    {
        // Replace with your app's client ID and tenant ID
        private static readonly string ClientId = "YOUR_CLIENT_ID";
        private static readonly string TenantId = "YOUR_TENANT_ID";
        
        // Microsoft Graph API scope
        private static readonly string[] Scopes = { "User.Read" };
        
        static async Task Main(string[] args)
        {
            try
            {
                // Create the public client application
                var app = PublicClientApplicationBuilder
                    .Create(ClientId)
                    .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
                    .WithDefaultRedirectUri()
                    .Build();

                // Try to acquire token silently first
                var accounts = await app.GetAccountsAsync();
                AuthenticationResult result;
                
                try
                {
                    result = await app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    // If silent acquisition fails, use interactive authentication
                    result = await app.AcquireTokenInteractive(Scopes)
                        .ExecuteAsync();
                }

                Console.WriteLine($"Token acquired! Welcome, {result.Account.Username}");
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
                
                var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
                var content = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine("User info from Microsoft Graph:");
                Console.WriteLine(content);
            }
        }
    }
}
```

## Step 5: Update Configuration

1. Replace `YOUR_CLIENT_ID` with your application's client ID
2. Replace `YOUR_TENANT_ID` with your tenant ID

## Step 6: Run Your Application

```bash
dotnet run
```

## What Happens Next?

1. A browser window opens asking you to sign in
2. After authentication, you'll see a welcome message
3. The app calls Microsoft Graph to get your user profile

## 🎉 Congratulations!

You've successfully integrated MSAL.NET into your application! 

## Next Steps

- [Learn about different authentication scenarios](scenarios.md)
- [Explore token caching options](../acquiring-tokens/acquire-token-silently.md)
- [Understand best practices](best-practices.md)
- [Handle errors and exceptions](../advanced/exceptions/index.md)

## Common Issues and Solutions

### Issue: "AADSTS50011: The reply URL specified in the request does not match..."
**Solution**: Ensure your redirect URI in Azure portal matches the one in your code.

### Issue: "MSAL.NET requires a redirect URI"
**Solution**: Use `.WithDefaultRedirectUri()` for public client applications.

### Issue: Token acquisition fails silently
**Solution**: Check that your app has the necessary permissions and they're granted admin consent.

## Additional Resources

- [MSAL.NET API Reference](../../../dotnet/api/overview/index.md)
- [Microsoft Graph Explorer](https://developer.microsoft.com/graph/graph-explorer)
- [Azure AD app registration guide](/azure/active-directory/develop/quickstart-register-app)
