---
title: Proof-of-Possession (PoP) tokens
description: Learn how to acquire Proof-of-Possession tokens for public and confidential clients in MSAL.NET
---

# Proof-of-Possession (PoP) tokens

Bearer tokens are the norm in modern identity flows, however they are vulnerable to being stolen from token caches and via man in the middle attacks.

Proof-of-Possession (PoP) tokens - as described by [RFC 7800](https://tools.ietf.org/html/rfc7800) - mitigate this threat. PoP tokens are bound to the client machine, via a public/private PoP key. The PoP public key is injected into the token by the token issuer (Entra ID), and the client
also signs the token using the private PoP key. A fully formed PoP token has 2 digital signatures - one from the token issuer and one from the client. So the PoP protocol has 2 protections:

- Protection against token cache compromise - MSAL will not store fully formed PoP tokens in the cache. Instead, it will sign tokens on demand, when the app needs them. An attacker who is able to compromise the token cache will not be able to digitally sign with the PoP private key. 
- Protection against man-in-the-middle attacks - A server nonce is added to the protocol.

> [!WARNING]
> The strength of the PoP protocol depends in the strength of the PoP keys. Microsoft recommends using hardware keys - [TPM](https://support.microsoft.com/topic/what-is-tpm-705f241d-025d-4470-80c5-4feeb24fa1ee) where possible.

## PoP Variants

There are several PoP protocols and variations, and Microsoft has decided to focus on 2 of them: 

- PoP via Signed Http Request (SHR) - see [PoP key distribution](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-pop-key-distribution-07) and [SHR](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-signed-http-request-03) 
- PoP via MTLS - see [RFC 8705](https://datatracker.ietf.org/doc/html/rfc8705)

MTLS is faster and has the advantage of including man-in-the-middle protection at the TLS layer, but it can be difficult to establish MTLS tunnels between the client and the Identity Provider and between the client and the resource. PoP via Signed Http Request (SHR) does not rely on transport protocol changes, however the server nonce must be handled explicitly by the app developer.

All client SDKs - MSAL libraries - support PoP via SHR for public client (desktop apps). For confidential client (web apps, web apis, managed identity), Microsoft is exploring PoP via MTLS.

## Support for PoP

Microsoft has enabled PoP via Signed Http Request (SHR) in some of its Web APIs. Microsoft Graph supports PoP tokens - for example if you make an unauthenticated request to https://graph.microsoft.com/v1.0/me/messages, you will get a 401 response with two WWW-Authenticate headers, indicating Bearer and PoP support.

![image](https://github.com/MicrosoftDocs/microsoft-authentication-library-dotnet/assets/12273384/2b4ec2d4-7d57-411e-ae27-f3c764d5909d)

### Token validation - could my own web api validate PoP tokens?

Microsoft does not currently offer an SDK for PoP token validation. A validator exists for Microsoft's own web apis, with plans to open source it.

## Proof-of-Possession for public clients

Proof-of-Possession on public client flows can be achieved with the use of the updated [Windows broker](../acquiring-tokens/desktop-mobile/wam.md) in MSAL 4.52.0 and above. 
MSAL Java, MSAL Python and MSAL NodeJS also support it in conjuction with the broker. 

MSAL will use the best available keys which exist on the machine, typically hardware keys (see [TPM](/windows/security/hardware-security/tpm/tpm-fundamentals)). There is no option to "bring your own key".

It is possible that a client does not support creating PoP tokens. This is due to the fact that brokers (WAM, Company Portal) are not always present on the device or that the SDK does not implement the protocol on a specific operating system. Currently, PoP tokens are available on Windows 10+ and Windows Server 2019+. Use the API `publicClientApp.IsProofOfPossessionSupportedByClient()` to understand if POP is supported by the client.

Example implementation:

```csharp
// Required for the use of the broker 
using Microsoft.Identity.Client.Broker; 

// The PoP token will be bound to this user / machine and to `GET https://www.contoso.com/tranfers` (the query parameters are not bound).
// The nonce is a requirement in this case and needs to be acquired from the resource before using this API.

// Server nonce is required
string nonce = "nonce";

//HttpMethod is optional
HttpMethod method = HttpMethod.Get;

//Request URI
Uri requestUri = new Uri("https://www.contoso.com/tranfers?user=me");
          
var pca = PublicClientApplicationBuilder.Create(CLIENT_ID)
    .WithBroker()  //Enables the use of broker on public clients only
    .Build();

//Interactive request
AuthenticationResult result = await pca
      .AcquireTokenInteractive(new[] { "scope" })
      .WithProofOfPossession(nonce, method, requestUri)
      .ExecuteAsync()
      .ConfigureAwait(false);

// The PoP token will be available in the AuthenticationResult.AccessToken returned form the acquire token call

//To create the auth header
var authHeader = new AuthenticationHeaderValue(result.TokenType, result.AccessToken);

//Silent request
var accounts = await pca.GetAccountsAsync().ConfigureAwait(false);
var result = await pca.AcquireTokenSilent(new[] { "scope" }, accounts.FirstOrDefault())
       .WithProofOfPossession(nonce, method, requestUri)
       .ExecuteAsync()
       .ConfigureAwait(false);

```

## Proof-of-Possession for confidential clients


> [!NOTE]
> Proof-of-Possession via Signed Http Request is experimental for confidential clients and will likely be renamed or removed in a future version. Proof-of-Posession via MTLS is being explored.
>

Example implementation:

```csharp
// The PoP token will be bound to this user / machine and to `GET https://www.contoso.com/tranfers` (the query params are not bound).
// Request URI is required in the PopAuthenticationConfiguration constructor
PopAuthenticationConfiguration popConfig = new PopAuthenticationConfiguration(new Uri("https://www.contoso.com/tranfers?user=me"));

//HttpMethod is optional
popConfig.HttpMethod = HttpMethod.Get;

// Server nonce is optional
popConfig.Nonce = "nonce";

//PopCryptoProvider is optional. Do not set to use MSAL's internal implementation.
popConfig.PopCryptoProvider = new ECDCertificatePopCryptoProvider();
          
var cca = ConfidentialClientApplicationBuilder.Create(CLIENT_ID)
    .WithExperimentalFeatures()     // Currently PoP for confidential client is an experimental feature
    .Build();


result = await cca
      .AcquireTokenForClient (new[] { "scope"})
      .WithProofOfPossession(popConfig)
      .ExecuteAsync()
      .ConfigureAwait(false);

//The PoP token will be available on the AuthenticationResult.AccessToken returned form the acquire token call

//To create the auth header
var authHeader = new AuthenticationHeaderValue(result.TokenType, result.AccessToken);
```

### No hardware keys by default

MSAL.NET experimental API uses in-memory / software keys. 

An RSA key pair of length 2048 is generated by MSAL and stored in memory which will be cycled every 8 hours. For details, see the implementation in [PoPProviderFactory](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/300fba16bd8096dceba3684311550b4b52a56177/src/client/Microsoft.Identity.Client/AuthScheme/PoP/PoPProviderFactory.cs#L18) and [InMemoryCryptoProvider](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/main/src/client/Microsoft.Identity.Client/AuthScheme/PoP/InMemoryCryptoProvider.cs).

### Bring your own key

To use a better key, the API allows app developers to provide their own key management. The interface is an abstraction over the asymmetric key operations needed by PoP that encapsulates a pair of public and private keys and some typical crypto operations. All symmetric operations use SHA256.

> [!IMPORTANT]
> Two properties and the sign method on this interface will be called at different times but MUST return details of the same private / public key pair, i.e. do not change to a different key pair mid way. It is best to make this class immutable. Ideally there should be a single public and private key pair associated with a machine, so that implementers of this interface should consider exposing a singleton. See [IPoPCryptoProvider interface](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/src/client/Microsoft.Identity.Client/AuthScheme/PoP/IPoPCryptoProvider.cs), [example RSA key implementation](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/9895855ac4fcf52893fbc2b06ee20ea3eda1549a/tests/Microsoft.Identity.Test.Integration.netfx/HeadlessTests/PoPTests.cs#L503), and [example ECD key implementation](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/9895855ac4fcf52893fbc2b06ee20ea3eda1549a/tests/Microsoft.Identity.Test.Common/Core/Helpers/ECDCertificatePopCryptoProvider.cs#L11).

### How to add more claims / How do I create the Signed HTTP Request (SHR) part of the PoP token myself?

To create the SHR yourself,  see [this example implementation](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/300fba16bd8096dceba3684311550b4b52a56177/tests/Microsoft.Identity.Test.Integration.netfx/HeadlessTests/PoPTests.cs#L286).


