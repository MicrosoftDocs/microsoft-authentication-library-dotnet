# Getting tenant profiles with MSAL.NET

Here is a code sample that acquires tokens for the same account, but in different tenant, and then displays the tenants
and the claims of the ID token in each tenant

```csharp
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        IPublicClientApplication app = PublicClientApplicationBuilder.Create("4a1aa1d5-c567-49d0-ad0b-cd957a47f842")
            .WithDefaultRedirectUri()                                    
            .Build();

        // Authenticate in my home tenant (Authority is 'common')
        AuthenticationResult result = await app.AcquireTokenInteractive(new[] { "user.read" })
            .ExecuteAsync();

        // Get a new token for myself, but in another tenant.
        result = await app.AcquireTokenSilent(new[] { "user.read" }, result.Account)
            .WithAuthority(app.Authority.Replace("common", "msidentitysamplestesting.onmicrosoft.com"))
            .ExecuteAsync();

        // Display tenants, and claims
        foreach (var tenantProfile in result.Account.GetTenantProfiles())
        {
            Console.WriteLine($"Tenant= {tenantProfile.TenantId}");
            foreach(var claim in tenantProfile.ClaimsPrincipal.Claims)
            {
                Console.WriteLine($"  {claim.Type}={claim.Value}");
            }
        }
    }
}
```