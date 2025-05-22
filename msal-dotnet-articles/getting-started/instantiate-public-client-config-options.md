---
title: Instantiate a public client app (MSAL.NET)
description: Learn how to instantiate a public client application with configuration options using the Microsoft Authentication Library for .NET (MSAL.NET).
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 08/24/2023
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: how-to
ms.custom: devx-track-csharp, aaddev, devx-track-dotnet
#Customer intent: As an application developer, I want to learn how to use application config options so I can instantiate a public client app.
---

# Instantiate a public client application with configuration options using MSAL.NET

This article describes how to instantiate a [public client application](/azure/active-directory/develop/msal-client-applications) using the Microsoft Authentication Library for .NET (MSAL.NET).  The application is instantiated with configuration options defined in a settings file.

Before initializing an application, you first need to [register](/azure/active-directory/develop/quickstart-register-app) it so that your app can be integrated with the Microsoft identity platform. After registration, you may need the following information (which can be found in the Azure portal):

- The client ID (a string representing a GUID)
- The identity provider URL (named the instance) and the sign-in audience for your application. These two parameters are collectively known as the authority.
- The tenant ID if you are writing a line of business application solely for your organization (also named single-tenant application).
- For web apps, and sometimes for public client apps (in particular when your app needs to use a broker), you'll have also set the redirectUri where the identity provider will contact back your application with the security tokens.

## Default Reply URI

In MSAL.NET 4.1+ the default redirect URI (Reply URI) can now be set with the `public PublicClientApplicationBuilder WithDefaultRedirectUri()` method. This method will set the redirect URI property of public client application to the recommended default.

This method's behavior is dependent upon the platform that you are using at the time. Here is a table that describes what redirect URI is set on certain platforms:

| Platform  | Redirect URI |
|:----------|:-------------|
| Desktop app (.NET Framework) | `https://login.microsoftonline.com/common/oauth2/nativeclient` |
| .NET Core | `http://localhost` |

For .NET, MSAL.NET is setting the value to the host to enable the user to use the system browser for interactive authentication.

> [!NOTE]
> For embedded browsers in desktop scenarios the redirect URI used is intercepted by MSAL to detect that a response is returned from the identity provider that an auth code has been returned. This URI can therefore be used in any cloud without seeing an actual redirect to that URI. This means you can and should use `https://login.microsoftonline.com/common/oauth2/nativeclient` in any cloud. If you prefer you can also use any other URI as long as you configure the redirect URI correctly with MSAL and in the app registration. Specifying the default URI in the application registration means there is the least amount of setup in MSAL.


A .NET Core console application could have the following *appsettings.json* configuration file:

```json
{
  "Authentication": {
    "AzureCloudInstance": "AzurePublic",
    "AadAuthorityAudience": "AzureAdMultipleOrgs",
    "ClientId": "00001111-aaaa-2222-bbbb-3333cccc4444"
  },

  "WebAPI": {
    "MicrosoftGraphBaseEndpoint": "https://graph.microsoft.com"
  }
}
```

The following code reads this file using the .NET configuration framework:

```csharp
public class SampleConfiguration
{
    /// <summary>
    /// Authentication options
    /// </summary>
    public PublicClientApplicationOptions PublicClientApplicationOptions { get; set; }

    /// <summary>
    /// Base URL for Microsoft Graph (it varies depending on whether the application is ran
    /// in Microsoft Azure public clouds or national / sovereign clouds
    /// </summary>
    public string MicrosoftGraphBaseEndpoint { get; set; }

    /// <summary>
    /// Reads the configuration from a json file
    /// </summary>
    /// <param name="path">Path to the configuration json file</param>
    /// <returns>SampleConfiguration as read from the json file</returns>
    public static SampleConfiguration ReadFromJsonFile(string path)
    {
        // .NET configuration
        IConfigurationRoot Configuration;
        var builder = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile(path);
        Configuration = builder.Build();

        // Read the auth and graph endpoint config
        SampleConfiguration config = new SampleConfiguration()
        {
            PublicClientApplicationOptions = new PublicClientApplicationOptions()
        };
        Configuration.Bind("Authentication", config.PublicClientApplicationOptions);
        config.MicrosoftGraphBaseEndpoint = Configuration.GetValue<string>("WebAPI:MicrosoftGraphBaseEndpoint");
        return config;
    }
}
```

The following code creates your application, using the configuration from the settings file:

```csharp
SampleConfiguration config = SampleConfiguration.ReadFromJsonFile("appsettings.json");
var app = PublicClientApplicationBuilder.CreateWithApplicationOptions(config.PublicClientApplicationOptions)
           .Build();
```
