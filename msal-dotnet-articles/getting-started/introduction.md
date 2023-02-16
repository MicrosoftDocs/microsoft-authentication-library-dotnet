## Why use MSAL.NET ?
MSAL.NET ([Microsoft Authentication Library for .NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet)) enables developers of .NET applications to **acquire [tokens](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-dev-glossary#security-token) in order to call secured Web APIs**. These Web APIs can be the Microsoft Graph, other Microsoft APIS, 3rd party Web APIs, or your own Web API. 

### MSAL.NET supports multiple application architectures
MSAL.NET supports all the possible application topologies including:
- [native client](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-dev-glossary#native-client)  (mobile/desktop applications) calling the Microsoft Graph in the name of the user, 
- daemons/services or [web clients](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-dev-glossary#web-client)  (Web Apps/ Web APIs) calling the Microsoft Graph in the name of a user, or without a user. 

With the exception of:
- [User-agent based client](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-dev-glossary#user-agent-based-client) which is only supported in JavaScript

For details about the supported scenarios see [Scenarios](https://aka.ms/msal-net-scenarios)

### MSAL.NET supports multiple platforms

- .NET Framework,
- [.NET Core](https://www.microsoft.com/net/learn/get-started/windows)(including .NET 6), 
- [Xamarin](https://www.xamarin.com/) Android, 
- Xamarin iOS,
- [UWP](https://docs.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide)

> Important

Not all the authentication features are available in all platforms, mostly because:
- mobile platforms (Xamarin and UWP) do not allow confidential client flows, because they are not meant to function as a backend and to store secrets, 
- on public client (mobile and desktop), the default browser and redirect URIs are different from platform to platform and broker availability varies (details [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/MSAL.NET-uses-web-browser#at-a-glance))

Most of the pages in the wiki describe the most complete platform (.NET Framework), but, topic by topic, it occasionally calls out differences between platforms.

### Added value by using MSAL.NET over OAuth libraries or coding against the protocol?
MSAL.NET is a token acquisition library. Depending on your scenario it provides you with various way of getting a token, with a consistent API for a number of platforms.
It also adds value by:
- maintaining a **token cache** and **refreshes tokens** for you when they are close to expire. 
  > you don't need to handle expiration on your own.
- helping you specify which **audience** you want your application to sign-in (your org, several orgs, work and school and Microsoft personal accounts, Social identities with Azure AD B2C, users in sovereign and national clouds)
- helping you setting-up your application from **configuration** files
- **helping you troubleshooting** your app by exposing actionable exceptions, logging and telemetry.

## MSAL.NET is about acquiring tokens, not protecting an API
MSAL.NET is used to acquire tokens. It's not used to protect a Web API. If you are interested in protecting a Web API with Azure AD, you might want to check out:
- [Azure Active Directory with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/azure-active-directory/). Note that some of these examples present Web Apps which also call a Web API with ADAL.NET or MSAL.NET
- [active-directory-dotnet-native-aspnetcore-v2](https://github.com/azure-samples/active-directory-dotnet-native-aspnetcore-v2) which demoes calling a ASP.NET Core Web API from a WPF application using Azure AD V2
- The [IdentityModel extensions for .Net](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet) open source library providing middleware used by ASP.NET and ASP.NET Core to protect APIs
