# MSAL.NET 4.3 released

We are excited to announce the release of MSAL.NET 4.3 which brings one feature, and fixes bugs.

- [Broker support for Xamarin iOS](#broker-support-on-xamarinios), 

## Broker support on Xamarin.iOS

### What are brokers?

Brokers are applications, provided by Microsoft on Android and iOS (Microsoft Authenticator on iOS and Android, Intune Company Portal on Android). They enable:

- Single-Sign-On, 
- Device identification, which is required by some [conditional access](https://docs.microsoft.com/azure/active-directory/conditional-access/overview) policies (See [Device management](https://docs.microsoft.com/azure/active-directory/conditional-access/conditions#device-platforms)
- Application identification verification also required in some enterprise scenarios (See for instance [Intune mobile application management](https://docs.microsoft.com/en-us/intune/mam-faq), or MAM)

### How to enable them?

If you build an application that needs to work in tenants where conditional access is enabled, or if you want your users can benefit from a better experience, you should enable brokers. This is simple. you'll need to call `WithBroker()` at the construction of the application. Then when the user signs-in interactively, they will be directed by Azure AD to install a broker from the store depending on the conditional access policies. When this is done, the next interactive authentication will use the broker.

For details, see https://aka.ms/msal-net-brokers for more information on platform specific settings required to enable the broker.

```CSharp
IPublicClientApplication application = PublicClientApplicationBuilder.Create(clientId)
  .WithDefaultRedirectUri()
  .WithBroker()
  .Build();
```

> Broker support is only available on iOS at this time. Microsoft Authenticator is supporting the microsoft
> identity platform v2.0 endpoint. When brokers are deployed for Android, MSAL.NET will also support brokers
> on Android with the same mechanism.

### How to migrate from ADAL.NET iOS Broker to MSAL.NET iOS Broker?
To assist in moving your ADAL.NET Xamarin iOS application to MSAL.NET, see this [migration page](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/How-to-migrate-from-using-iOS-Broker-on-ADAL.NET-to-MSAL.NET) for the code changes needed between ADAL.NET and MSAL.NET to target the iOS Broker. 