#### Availability

MSAL is a multi-framework library. All Confidential Client flows, including this one, **are available on**:

- .NET Core
- .NET Desktop
- .NET Standard

They are not available on the mobile platforms, because the OAuth2 spec states that there should be a secure, dedicated connection between the application and the Identity Provider. This secure connection can be achieved on web server / web API back-ends by deploying a certificate (or a secret string, but this is not recommended for production). It cannot be achieved on mobile and other client applications that are distributed to users. As such, these flows **are not available on**: 

- Xamarin.Android
- Xamarin.iOS
- UWP
