---
title: Proof-of-Possession (PoP) tokens
description: Learn how to acquire Proof-of-Possession tokens for public and confidential clients in MSAL.NET
---

# Proof-of-Possession (PoP) tokens

Bearer tokens are the norm in modern identity flows, however they are vulnerable to being stolen and used to access a protected resource.

Proof-of-Possession (PoP) tokens mitigate this threat via 2 mechanisms:

- They are bound to the user/machine that wants to access a protected resource, via a public/private key pair
- They are bound to the protected resource itself, i.e. a token that is used to access `GET https://contoso.com/transactions` cannot be used to access `GET https://contoso.com/tranfer/100`

For more details, see [RFC 7800](https://tools.ietf.org/html/rfc7800).

## Does the protected resource accept PoP tokens?

If you make an unauthenticated request to a protected API, it should reply with HTTP 401 Unauthorized response, and with some [WWW-Authenticate](https://developer.mozilla.org/docs/Web/HTTP/Headers/WWW-Authenticate) headers. These headers inform the clients of the available authentication schemes, such as Basic, NTLM, Bearer, and POP. The MSAL family of libraries can help with Bearer and PoP.

Programatically, MSAL.NET offers [a helper API](extract-authentication-parameters.md) for parsing these headers.

## Can my own web api validate PoP tokens?

Microsoft does not currently offer an out-of-the-box PoP token validation experience, in the same way that it offers a Bearer token validation experience for web apis. A validator exists for Microsoft's own web apis.

## Proof-of-Possession for public clients

Proof-of-Possession on public client flows can be achieved with the use of the updated [Windows broker](../acquiring-tokens/desktop-mobile/wam.md) in MSAL 4.52.0 and above. MSAL will use the best available keys which exist on the machine, typically hardware keys (see [TPM](/windows/security/hardware-security/tpm/tpm-fundamentals)).

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
> Proof-of-Possession is experimental for confidential clients. 
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

## How does MSAL manage the keys

An RSA key pair of length 2048 is generated by MSAL and stored in memory which will be cycled every 8 hours. For details, see the implementation in [PoPProviderFactory](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/300fba16bd8096dceba3684311550b4b52a56177/src/client/Microsoft.Identity.Client/AuthScheme/PoP/PoPProviderFactory.cs#L18) and [InMemoryCryptoProvider](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/main/src/client/Microsoft.Identity.Client/AuthScheme/PoP/InMemoryCryptoProvider.cs).

## Bring your own key

The PoP feature in MSAL allows users to provide their own key management for additional control over cryptographic operations. The interface is an abstraction over the asymmetric key operations needed by PoP that encapsulates a pair of public and private keys and some typical crypto operations. All symmetric operations use SHA256.

> [!IMPORTANT]
> Two properties and the sign method on this interface will be called at different times but MUST return details of the same private / public key pair, i.e. do not change to a different key pair mid way. It is best to make this class immutable. Ideally there should be a single public and private key pair associated with a machine, so that implementers of this interface should consider exposing a singleton. See [IPoPCryptoProvider interface](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/src/client/Microsoft.Identity.Client/AuthScheme/PoP/IPoPCryptoProvider.cs), [example RSA key implementation](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/9895855ac4fcf52893fbc2b06ee20ea3eda1549a/tests/Microsoft.Identity.Test.Integration.netfx/HeadlessTests/PoPTests.cs#L503), and [example ECD key implementation](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/9895855ac4fcf52893fbc2b06ee20ea3eda1549a/tests/Microsoft.Identity.Test.Common/Core/Helpers/ECDCertificatePopCryptoProvider.cs#L11).

## How to add more claims / How do I create the Signed HTTP Request (SHR) part of the PoP token myself?

If you want to do key management and to create the SHR yourself,  see [this example implementation](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/300fba16bd8096dceba3684311550b4b52a56177/tests/Microsoft.Identity.Test.Integration.netfx/HeadlessTests/PoPTests.cs#L286).

An end to end implementation would need to: 

1. [Enable the use of broker](../acquiring-tokens/desktop-mobile/wam.md)
1. Check if the client is capable of creating PoP tokens using `publicClientApp.IsProofOfPossessionSupportedByClient()`
2. Make an unauthenticated call to the service
3. [Parse the WWW-Authenticate headers](extract-authentication-parameters.md) and if PoP is supported, extract the nonce
4. Request PoP tokens using the `AcquireTokenSilent` / `AcquireTokenInteractive` pattern, by adding the `.WithProofOfPossession(nonce, method, requestUri)` modifier
5. Make the request to the protected resource. If the request results in 200 OK, [parse the Authenticate-Info](extract-authentication-parameters.md)  header and extract the new `nonce` - it needs to be used at step 4 when requesting a new token. If the request results in a 401 Unauthenticated, observe the error - it may be because of an expired nonce. In that case, repeat steps 3-5. 
