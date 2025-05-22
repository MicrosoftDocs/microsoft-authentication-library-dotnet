---
title: Using SSH certificates with MSAL.NET
description: "Microsoft Entra ID is capable of issuing SSH certificates instead of bearer tokens."
author: 
manager: 
ms.author: 
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: 
ms.custom: 
#Customer intent: 
---

# Using SSH certificates with MSAL.NET

>[!NOTE]
>This feature is available from MSAL 4.3.2 onward

Microsoft Entra ID is capable of issuing SSH certificates instead of bearer tokens. These are not the same as SSH public keys. Currently this is available as an extension method on `AcquireTokenSilent` and `AcquireTokenInteractive`.

```csharp
var result = await pca
    .AcquireTokenSilent(s_scopes, account)
    .WithSSHCertificateAuthenticationScheme(jwk, "keyID1")
    .ExecuteAsync();
```

Paramters:

- `jwk` - The public SSH key in JWK format as described at https://tools.ietf.org/html/rfc7517 . Currently only RSA with a minimum key size of 2048 bytes is supported.
- `keyID` - Any string that distinguishes between keys (usually hash of the key, but format is not important)

Example creating a JWK

```csharp
private string CreateJwk()
{
     RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
     RSAParameters rsaKeyInfo = rsa.ExportParameters(false);

     // Algorithm behind Base64UrlHelpers.Encode is described here https://www.rfc-editor.org/rfc/rfc7515.html#appendix-C
     string modulus = Base64UrlHelpers.Encode(rsaKeyInfo.Modulus); 
     string exp = Base64UrlHelpers.Encode(rsaKeyInfo.Exponent);
     string jwk = $"{{\"kty\":\"RSA\", \"n\":\"{modulus}\", \"e\":\"{exp}\"}}";

     return jwk;
}
```
