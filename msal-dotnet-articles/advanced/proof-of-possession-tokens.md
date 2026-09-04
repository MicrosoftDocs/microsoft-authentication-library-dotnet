---
title: Proof-of-Possession (PoP) tokens
description: Learn how to acquire Proof-of-Possession tokens for public and confidential clients in MSAL.NET
author: Dickson-Mwendia
manager: 
ms.author: dmwendia
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: concept-article
ms.custom: 
#Customer intent: 
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

- [mTLS POP - RFC 8705](https://datatracker.ietf.org/doc/html/rfc8705). Intended for service to service communication, e.g. a workloads fetching secrets from Azure KeyVault.
- [DPOP  - RFC 9449](https://datatracker.ietf.org/doc/html/rfc9449). Intended for public client applications.

Why 2 protocols? mTLS POP is faster and has the advantage of including nonce protections at the TLS layer; however, it can be difficult to establish mTLS tunnels between the client and the identity provider and between the client and the resource. dPOP does not rely on transport protocol changes; however the server nonce must be handled explicitly by the app developer. 

## Legacy support for PoP SHR

Microsoft provided support for **PoP via Signed HTTP Request (SHR)** . See [PoP key distribution](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-pop-key-distribution-07) and [SHR](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-signed-http-request-03) for the detailed specifications. This protocol is being phased out and replaced with DPOP.

## Token validation

Microsoft offers token validation primitives for .NET via https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet - both types of tokens can be validated this way.

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

#### Adding more claims or creating the SHR request part of the PoP token

To create the SHR yourself, refer to [the example implementation](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/300fba16bd8096dceba3684311550b4b52a56177/tests/Microsoft.Identity.Test.Integration.netfx/HeadlessTests/PoPTests.cs#L286).
