# Confidential Client Assertions
In order to prove their identity, confidential client applications exchange a secret with Azure AD. This can be a:
- a client secret (application password), 
- a certificate, which is really used to build a signed assertion containing standard claims. 
This can also be a signed assertion directly.

MSAL.NET has 4 methods to provide either credentials or assertions to the confidential client app: `.WithClientSecret()` `.WithCertificate()`, `.WithSignedAssertion()` and `.WithClientClaims()`.

NOTE: While it is possible to use the `WithSignedAssertion()` api to acquire tokens for the confidential client, we do not recommend using it by default as it is more advanced and is designed to handle very specific scenarios which are not common. Using the `.WithCertificate()` api will allow MSAL.NET to handle this for you. This api offers you the ability to customize your authentication request if needed but the default assertion created by `.WithCertificate()` will suffice for most authentication scenarios. This api can also be used as a workaround in some scenarios where MSAL.NET fails to perform the signing operation internally.

### Signed Assertions

A signed client assertion takes the form of a signed JWT with the payload containing the required authentication claims mandated by Azure AD, Base64 encoded. To use it:

```CSharp
string signedClientAssertion = ComputeAssertion();
app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                                          .WithClientAssertion(signedClientAssertion)
                                          .Build();
```

There is also an override taking a delegate that will be called by MSAL.NET whenever it needs a signed assertion from your app:
```CSharp
string signedClientAssertion = ComputeAssertion();
app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                                          .WithClientAssertion(() => { ComputeSignedAssertion() })
                                          .Build();
```

The claims expected by Azure AD are:

Claim type | Value | Description
---------- | ---------- | ----------
aud | https://login.microsoftonline.com/{tenantId}/v2.0 | The "aud" (audience) claim identifies the recipients that the JWT is intended for (here Azure AD) See [RFC 7519, Section 4.1.3]
exp | Thu Jun 27 2019 15:04:17 GMT+0200 (Romance Daylight Time) | The "exp" (expiration time) claim identifies the expiration time on or after which the JWT MUST NOT be accepted for processing. See [RFC 7519, Section 4.1.4]
iss | {ClientID} | The "iss" (issuer) claim identifies the principal that issued the JWT. The processing of this claim is generally application specific. The "iss" value is a case-sensitive string containing a StringOrURI value. [RFC 7519, Section 4.1.1]
jti | (a Guid) | The "jti" (JWT ID) claim provides a unique identifier for the JWT. The identifier value MUST be assigned in a manner that ensures that there is a negligible probability that the same value will be accidentally assigned to a different data object; if the application uses multiple issuers, collisions MUST be prevented among values produced by different issuers as well. The "jti" claim can be used to prevent the JWT from being replayed. The "jti" value is a case-sensitive string. [RFC 7519, Section 4.1.7]
nbf | Thu Jun 27 2019 14:54:17 GMT+0200 (Romance Daylight Time) | The "nbf" (not before) claim identifies the time before which the JWT MUST NOT be accepted for processing. [RFC 7519, Section 4.1.5]
sub | {ClientID} | The "sub" (subject) claim identifies the subject of the JWT. The claims in a JWT are normally statements about the subject. The subject value MUST either be scoped to be locally unique in the context of the issuer or be globally unique. The See [RFC 7519, Section 4.1.2]
kid | {Certificate Thumbprint} | The X.509 certificate hash's (also known as the cert's SHA-1 thumbprint) Hex representation encoded as a Base64url string value. For example, given an X.509 certificate hash of 84E05C1D98BCE3A5421D225B140B36E86A3D5534 (Hex), the kid claim would be hOBcHZi846VCHSJbFAs26Go9VTQ= (Base64url).
x5c | {Certificate Public Key Value} | The "x5c" (X.509 certificate chain) Header Parameter contains the X.509 public key certificate or certificate chain [RFC5280] corresponding to the key used to digitally sign the JWS. The certificate or certificate chain is represented as a JSON array of certificate value strings. Each string in the array is a base64-encoded (not base64url-encoded) DER [ITU.X690.2008] PKIX certificate value.

Here is an example of how to craft these claims:

### Option 1 - using Microsoft.IdentityModel.JsonWebTokens library (Recommended)

You have the option of using [Microsoft.IdentityModel.JsonWebTokens](https://www.nuget.org/packages/Microsoft.IdentityModel.JsonWebTokens/) to create the assertion for you. 

```CSharp

         private static string GetSignedClientAssertionUsingWilson(
            string issuer, // client ID
            string aud, // $"{authority}/oauth2/v2.0/token" for AAD, $"{authority}/oauth2/token" for ADFS
            X509Certificate2 cert)
        {
            // no need to add exp, nbf as JsonWebTokenHandler will add them by default.
            var claims = new Dictionary<string, object>()
            {
                { "aud", aud },
                { "iss", issuer },
                { "jti", Guid.NewGuid().ToString() },
                { "sub", issuer }
            };

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                SigningCredentials = new X509SigningCredentials(cert)
            };

            var handler = new JsonWebTokenHandler();
            var signedClientAssertion = handler.CreateToken(securityTokenDescriptor);

            return signedClientAssertion;
        }
```

### Option 2 - creating a signed assertion manually

```CSharp
         private string GetSignedClientAssertionDirectly(
            string issuer, // client ID
            string audience, // ${authority}/oauth2/v2.0/token for AAD or ${authority}/oauth2/token for ADFS
            X509Certificate2 certificate)
        {
            const uint JwtToAadLifetimeInSeconds = 60 * 10; // Ten minutes
            DateTime validFrom = DateTime.UtcNow;
            var nbf = ConvertToTimeT(validFrom);
            var exp = ConvertToTimeT(validFrom + TimeSpan.FromSeconds(JwtToAadLifetimeInSeconds));

            var payload = new Dictionary<string, string>()
            {
                { "aud", audience },
                { "exp", exp.ToString(CultureInfo.InvariantCulture) },
                { "iss", issuer },
                { "jti", Guid.NewGuid().ToString() },
                { "nbf", nbf.ToString(CultureInfo.InvariantCulture) },
                { "sub", issuer }
            };

            RSACng rsa = certificate.GetRSAPrivateKey() as RSACng;

            //alg represents the desired signing algorithm, which is SHA-256 in this case
            //kid represents the certificate thumbprint
            var header = new Dictionary<string, string>()
            {
              { "alg", "RS256"},
              { "kid", Base64UrlEncode(certificate.GetCertHash()) }
            };

            string token = Base64UrlEncode(
                Encoding.UTF8.GetBytes(JObject.FromObject(header).ToString())) + 
                "." + 
                Base64UrlEncode(Encoding.UTF8.GetBytes(JObject.FromObject(payload).ToString()));

            string signature = Base64UrlEncode(
                rsa.SignData(
                    Encoding.UTF8.GetBytes(token),
                    HashAlgorithmName.SHA256, 
                    System.Security.Cryptography.RSASignaturePadding.Pkcs1));
            return string.Concat(token, ".", signature);
        }

        private static string Base64UrlEncode(byte[] arg)
        {
            char Base64PadCharacter = '=';
            char Base64Character62 = '+';
            char Base64Character63 = '/';
            char Base64UrlCharacter62 = '-';
            char Base64UrlCharacter63 = '_';

            string s = Convert.ToBase64String(arg);
            s = s.Split(Base64PadCharacter)[0]; // RemoveAccount any trailing padding
            s = s.Replace(Base64Character62, Base64UrlCharacter62); // 62nd char of encoding
            s = s.Replace(Base64Character63, Base64UrlCharacter63); // 63rd char of encoding

            return s;
        }        
       
        private static long ConvertToTimeT(DateTime time)
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = time - startTime;
            return (long)diff.TotalSeconds;
        }
```
### Using the signed assertion

Once you have your signed client assertion you can use it with the MSAL apis as shown below.
```CSharp
            string signedClientAssertion = GetSignedClientAssertionUsingWilson(
                issuer: "16dab2ba-145d-4b1b-8569-bf4b9aed4dc8", 
                aud: "https://login.windows.net/72f988bf-86f1-41af-91ab-2d7cd011db47/oauth2/v2.0/token",
                cert);

            var confidentialApp = ConfidentialClientApplicationBuilder
                .Create("16dab2ba-145d-4b1b-8569-bf4b9aed4dc8")
                .WithClientAssertion(signedClientAssertion)
                .Build();
```

In order to reduce the amount of overhead needed to perform this authentication, it is recommended to cache the assertion for the duration of the expiration time. The value of `JwtToAadLifetimeInSeconds` above can be adjusted to the desired expiration time of the assertion. It is in milliseconds and is set to 10 minutes which is what MSAL.NET uses internally by default. 

### WithClientClaims

WithClientClaims(X509Certificate2 certificate, IDictionary<string, string> claimsToSign, bool mergeWithDefaultClaims = true) by default will produce a signed assertion containing the claims expected by Azure AD plus additional client claims that you want to send. Here is a code snippet on how to do that.

```CSharp
string ipAddress = "192.168.1.2";
X509Certificate2 certificate = ReadCertificate(config.CertificateName);
app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                                          .WithAuthority(new Uri(config.Authority))
                                          .WithClientClaims(certificate, 
                                                                      new Dictionary<string, string> { { "client_ip", ipAddress } })
                                          .Build();

```

If one of the claims in the dictionary that you pass in is the same as one of the mandatory claims, the additional claims's value will be taken into account (it will override the claims computed by MSAL.NET)

If you want to provide your own claims, including the mandatory claims expected by Azure AD, simply pass in a false for the mergeWithDefaultClaims parameter.

### Subject Name Issuer Authentication

This mechanism is usable only on some internal Microsoft tenants. Details [here](https://identitydivision.visualstudio.com/IDDP/_git/first-party-docs?path=%2Fdocfx-src%2Fv2%2Ffirst-party-includes%2Fmsal-net-client-assertions-SNI.md&_a=preview).