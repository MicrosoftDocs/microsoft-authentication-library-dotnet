---
title: Proof-of-Possession (PoP) tokens
description: Learn how to acquire Proof-of-Possession tokens for public and confidential clients in MSAL.NET
---

# Proof-of-Possession (PoP) tokens

Bearer tokens are the norm in modern identity flows; however they are vulnerable to being stolen from token caches.

Proof-of-Possession (PoP) tokens, as described by [RFC 7800](https://tools.ietf.org/html/rfc7800), mitigate this threat. PoP tokens are bound to the client machine, via a public/private PoP key. The PoP public key is injected into the token by the token issuer (Entra ID) and the client
also signs the token using the private PoP key. A fully formed PoP token has two digital signatures - one from the token issuer and one from the client. The PoP protocol has two protections in place:

- **Protection against token cache compromise**. MSAL will not store fully-formed PoP tokens in the cache. Instead, it will sign tokens only when the app requests them. An attacker who is able to compromise the token cache should not be able to digitally sign the incomplete tokens in there, as they do not have access to the PoP private key. The ability of an attacker to steal a private key can be mitigated by using hardware protected keys.
- **Protection against man-in-the-middle attacks**. A server nonce is added to the protocol.

> [!WARNING]
> The strength of the PoP protocol depends in the strength of the PoP keys. Microsoft recommends using hardware keys via the [Trusted Platform Module (TPM)](https://support.microsoft.com/topic/what-is-tpm-705f241d-025d-4470-80c5-4feeb24fa1ee) where possible.

## PoP Variants

There are several PoP protocols and variations. The Microsoft Entra ID infrastructure aims to supports two types:

- **PoP via Signed HTTP Request (SHR)** . See [PoP key distribution](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-pop-key-distribution-07) and [SHR](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-signed-http-request-03) for the detailed specifications. This is fully supported by Entra ID and by the SDKs for public client scenarios, i.e. desktop and mobile apps.
- **PoP via mutual TLS (mTLS)**. See [RFC 8705](https://datatracker.ietf.org/doc/html/rfc8705) for details. Investigated for confidential clients, i.e. web sites, web apis, server to server calls. No support exists currently.

mTLS is faster and has the advantage of including man-in-the-middle protections at the TLS layer; however, it can be difficult to establish mTLS tunnels between the client and the identity provider and between the client and the resource. PoP via Signed HTTP Request (SHR) does not rely on transport protocol changes; however the server nonce must be handled explicitly by the app developer. 

## Support for PoP SHR

Microsoft has enabled PoP via Signed HTTP Request (SHR) in some of its web APIs. Microsoft Graph supports PoP tokens. For example, if you make an unauthenticated request to `https://graph.microsoft.com/v1.0/me/messages` you will get a `HTTP 401` response with two `WWW-Authenticate` headers, indicating bearer and PoP token support.

:::image type="content" source="../media/proof-of-possession-tokens/example-www-authenticate-headers.png" alt-text="Example of WWW-Authenticate headers in response":::

## Token validation

Microsoft does not currently offer a public SDK for PoP token validation.

## Usage

### Public client applications

PoP on public client flows can be achieved with the use of the [Windows broker](../acquiring-tokens/desktop-mobile/wam.md) (WAM). Other MSAL libraries also support PoP through WAM.

The broker (via MSAL) will use the best available keys which exist on the machine, typically hardware keys (e.g., [TPM](/windows/security/hardware-security/tpm/tpm-fundamentals)). There is no option to bring your own key.

It is possible that a client does not support creating PoP tokens. This is caused by the fact that brokers (such as WAM or Company Portal) are not always present on the device or the SDK does not implement the protocol on a specific operating system. Currently, PoP tokens are available on Windows 10 and above, as well as Windows Server 2019 and above. Use [`IsProofOfPossessionSupportedByClient()`](xref:Microsoft.Identity.Client.PublicClientApplication.IsProofOfPossessionSupportedByClient) to check if PoP is supported by the client.

#### Example

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

### Confidential client applications

> [!NOTE]
> Proof-of-Possession via Signed HTTP Request is experimental for confidential clients and will likely be renamed or removed in a future version. Future APIs will rely on PoP via mTLS.

#### Example

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

#### No hardware keys by default

MSAL.NET experimental API uses in-memory/software keys. An RSA key pair of length 2048 is generated by MSAL and stored in memory, cycled every eight hours. For details, see the implementation in [`PoPProviderFactory`](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/300fba16bd8096dceba3684311550b4b52a56177/src/client/Microsoft.Identity.Client/AuthScheme/PoP/PoPProviderFactory.cs#L18) and [`InMemoryCryptoProvider`](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/main/src/client/Microsoft.Identity.Client/AuthScheme/PoP/InMemoryCryptoProvider.cs).

#### Bring your own key

To use a better key, the API allows app developers to provide their own managed keys. The interface is an abstraction over the asymmetric key operations needed by PoP that encapsulates a pair of public and private keys and related crypto operations. All symmetric operations use SHA256.

> [!IMPORTANT]
> Two properties and the sign method on this interface will be called at different times but **must** return details of the same private/public key pair. Do not change to a different key pair through the process. It is best to make this class immutable. Ideally there should be a single public and private key pair associated with a machine. Implementers of this interface should consider exposing a singleton. See [`IPoPCryptoProvider`](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/src/client/Microsoft.Identity.Client/AuthScheme/PoP/IPoPCryptoProvider.cs), [example RSA key implementation](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/9895855ac4fcf52893fbc2b06ee20ea3eda1549a/tests/Microsoft.Identity.Test.Integration.netfx/HeadlessTests/PoPTests.cs#L503), and [an example ECD key implementation](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/9895855ac4fcf52893fbc2b06ee20ea3eda1549a/tests/Microsoft.Identity.Test.Common/Core/Helpers/ECDCertificatePopCryptoProvider.cs#L11) for reference.

#### Adding more claims or creating the SHR request part of the PoP token

To create the SHR yourself, refer to [the example implementation](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/300fba16bd8096dceba3684311550b4b52a56177/tests/Microsoft.Identity.Test.Integration.netfx/HeadlessTests/PoPTests.cs#L286).
