# MSAL.NET 4.9.0

We are excited to announce the release of MSAL.NET 4.9, which includes one new feature and bug fixes.

- [Broker support for Xamarin Android](#broker-support-on-xamarinandroid) 

Broker support for Xamarin iOS was released in MSAL.NET 4.3.

## Broker support on Xamarin.Android

### What are brokers?

Brokers are applications, provided by Microsoft on Android and iOS (Microsoft Authenticator on iOS and Android, Intune Company Portal on Android). They enable:

- Single-Sign-On - your users will not have to individually sign-in to each app
- Device identification - which is required by some [conditional access](https://docs.microsoft.com/azure/active-directory/conditional-access/overview) policies (See [Device management](https://docs.microsoft.com/azure/active-directory/conditional-access/conditions#device-platforms))
- Application identification - app verification is also required in some enterprise scenarios (See [Intune mobile application management or MAM](https://docs.microsoft.com/en-us/intune/mam-faq))

We highly recommend the use of a broker for a smooth app sign-in experience. A broker is required for conditional access scenarios and can also provide value in other identification scenarios.

### How do you use brokers for your application?

You call `WithBroker()` at the construction of the application. 
When the user signs-in interactively, they will be prompted by Azure AD to install the correct broker from the store, depending on the conditional access policies in your organization. For subsequent sign-ins, the interactive authentication will directly use the broker instead of prompting the user for credentials.

For platform-specific details on how this works, see [the docs here](https://aka.ms/msal-net-brokers).

```CSharp
var app = PublicClientApplicationBuilder
  .Create(ClientId)
  .WithBroker()
  .WithRedirectUri(redirectUriOnAndroid)
  .Build();
```

> Broker support available on both iOS and Android. 
> Microsoft Authenticator is supporting the Microsoft identity platform v2.0 endpoint. 

### How do I migrate from using a ADAL.NET mobile broker to a MSAL.NET mobile broker?
- [ADAL to MSAL .NET Xamarin Android migration page](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/How-to-migrate-from-using-Android-Broker-on-ADAL.NET-to-MSAL.NET)
- [ADAL to MSAL .NET Xamarin iOS migration page](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/How-to-migrate-from-using-iOS-Broker-on-ADAL.NET-to-MSAL.NET)
