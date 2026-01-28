---
title: Single-page applications (SPA) and authorization codes
description: "This flow enables confidential client applications to request an additional SPA auth code from the eSTS /token endpoint, and this authorization code can be redeemed silently by the front end running in the browser."
author: Dickson-Mwendia
manager: 
ms.author: dmwendia
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: how-to
ms.custom: 
#Customer intent: 
---

# Single-page applications (SPA) and authorization codes

This flow enables confidential client applications to request an additional SPA auth code from the eSTS /token endpoint, and this authorization code can be redeemed silently by the front end running in the browser. This feature is intended for applications that perform server-side (web apps) and browser-side (SPA) authentication, using a confidential SDK such as MSAL.net or MSAL Node server-side, and MSAL.js in the browser (e.g., an ASP.net web application hosting a React single-page application). In these scenarios, the application will likely need authentication both browser-side (e.g., a public client using MSAL.js) and server-side (e.g., a confidential client using MSAL.net), and each application context will need to acquire its own tokens.

Today, applications using this architecture will first interactively authenticate the user via the confidential client application, and then attempt to silently authenticate the user a second time with the public client. Unfortunately, this process is both relatively slow, and the silent network request made client-side (in a hidden iframe) will deterministically fail if third-party cookies are disabled/blocked. By acquiring a second authorization code server-side, MSAL.js can skip hidden iframe step, and immediately redeem the authorization code against the /token endpoint. This mitigates issued caused by third-party cookie blocking, and is also more performant.

## Availability

MSAL 4.40+ enables confidential clients to request an additional SPA auth code from the Microsoft Entra ID token endpoint.

## Required Redirect URI setup to support the flow

The redirect_uri used to acquire the spa auth code must be of type web.

## Acquire SPA Auth Code in the backend

In MSAL.Net, using the new `WithSpaAuthorizationCode` API get the `SpaAuthCode`.

```csharp
private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification context)
{
 try
 {
  // Upon successful sign in, get the access token & cache it using MSAL
  IConfidentialClientApplication clientApp = MsalAppBuilder.BuildConfidentialClientApplication();
  AuthenticationResult result = await clientApp.AcquireTokenByAuthorizationCode(new[] { "user.read" }, context.Code)
      .WithSpaAuthorizationCode(true)
      .ExecuteAsync();

   HttpContext.Current.Session.Add("Spa_Auth_Code", result.SpaAuthCode);
 }
 catch
 {
 
 }
}
```

## Using SPA Auth Code in the Front End

Configure a PublicClientApplication from MSAL.js in your single-page application:

```JS
const msalInstance = new msal.PublicClientApplication({
    auth: {
        clientId: "{{clientId}}",
        redirectUri: "http://localhost:3000/auth/client-redirect",
        authority: "{{authority}}"
    }
})
```

Next, render the code that was acquired server-side, and provide it to the acquireTokenByCode API on the MSAL.js PublicClientApplication instance. Do not include any additional scopes that were not included in the first login request, otherwise the user may be prompted for consent.

The application should also render any account hints, as they will be needed for any interactive requests to ensure the same user is used for both requests

```js
const code = "{{code}}";
const loginHint = "{{loginHint}}";

const scopes = [ "user.read" ];

return msalInstance.acquireTokenByCode({
    code,
    scopes
})
    .catch(error => {
         if (error instanceof msal.InteractionRequiredAuthError) {
            // Use loginHint/sid from server to ensure same user
            return msalInstance.loginRedirect({
                loginHint,
                scopes
            })
        }
    });
```

Once the Access Token is retrieved using the new MSAL.js `acquireTokenByCode` api, the token is then used to read the user's profile 

```js
function callMSGraph(endpoint, token, callback) {
    const headers = new Headers();
    const bearer = `Bearer ${token}`;
    headers.append("Authorization", bearer);

    const options = {
        method: "GET",
        headers: headers
    };

    console.log('request made to Graph API at: ' + new Date().toString());

    fetch(endpoint, options)
        .then(response => response.json())
        .then(response => callback(response, endpoint))
        .then(result => {
            console.log('Successfully Fetched Data from Graph API:', result);
        })
        .catch(error => console.log(error))
}
```

## Sample

[ASP.NET MVC project that uses the SPA Authorization Code in the Front End](https://github.com/Azure-Samples/ms-identity-aspnet-webapp-openidconnect)
