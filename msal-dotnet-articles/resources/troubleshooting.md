---
title: Troubleshooting MSAL.NET
description: Troubleshoot MSAL.NET issues with our comprehensive guide. Learn to fix JavaScript errors, region auto-discovery failures, and more on Microsoft's official site.
author: 
manager: 
ms.author: 
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: reference
ms.custom: 
#Customer intent: 
---

# Troubleshooting MSAL.NET

For issues using WAM, see [Web Account Manager](../acquiring-tokens/desktop-mobile/wam.md).

## JavaScript errors in the embedded browser

### Symptom

You are using a desktop application which launches a browser in a window which is used to authenticate.

And you have one of the following issues:

- Javascript errors occur which block navigation.
- A message stating the `browser is not up-to-date or is not supported`

### Possible causes

WebView1 (WebBrowser) control is used by MSAL on Windows. By default it uses a very old engine, comparable to IE7, which breaks, especially if you use your own ADFS server or a custom MFA provider.

### Long term fix

Move to login with [WAM](../acquiring-tokens/desktop-mobile/wam.md), which is available on Win 10+ and on Win Server 2019+.

### Workaround

If the above is not possible, another way of handling this is to force the embedded browser to user newer version of IE. There is a registry property and enable IE11 emulation on the internal browser:

This is for `ssms.exe`, but you can replace with your app:

```powershell
New-ItemProperty -path 'HKLM:\SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION' -name 'Ssms.exe' -value '11000' -PropertyType 'DWord'
```

You might have to do it for Powershell.exe / Powershell_ISE.exe were added as well

## Region auto-discovery failure

### Symptom 

You hit the global ESTS endpoint instead of a regional endpoint.

### Possible causes

1. The Azure environment the app is deployed to does not support region discovery (e.g. IMDS service is not available, or REGION_NAME variable is not set by Azure team).
2. IMDS call times out or fails. Currently it is set to wait for up to 2 seconds for a response.

### How to debug the issue

Auto-discovery happens once and only once before the first call to acquire token. The outcome is stored in memory and auto-discovery is not attempted again, for performance reasons. So to understand why auto-discovery fails, you need to capture the logs when the application starts.

1. To understand what endpoint is hit, monitor `AuthenticationResult.AuthenticationResultMetadata.TokenEndpoint` and `AuthenticationResult.AuthenticationResultMetadata.RegionDetails`. Regionalized endpoints are in the format `<region>.login.microsoft.com/<tenant>/oauth2/v2.0/token`.
2. Add verbose logging to MSAL. Logging details are [in our documentation](../advanced/exceptions/msal-logging.md). Note that you should not run with Verbose logging for a long time in production as it impacts performance.
3. Restart the service.

Restart your service and capture logs. Look at `AuthenticationResult.AuthenticationResultMetadata.RegionDetails` to understand if auto-discovery failed. Send the logs to the MSAL team.

## GetAccountAsync doesn't return any account in clouds scenarios

### Symptom

- You are building an application which acquires tokens for a sovereign clouds, using `AcquireTokenX().WithAuthority()`.
- When you try to get an account for a given account ID, the method `GetAccountAsync` of the application returns null.

### Solution

When building your application, even if you don't know the tenant yet, be sure to use an authority of the same cloud as the one for which you'll acquire tokens.

### Explanation

When you create your `ConfidentialClientApplicaiton` object, if you do not specify the authority, it defaults to `https://login.microsoftonline.com/common`. You then override this at the request level. This works fine for getting tokens.
The problem is that when you call `app.GetAccountsAsync()`, MSAL needs to filter by the environment (public cloud, Fairfax, etc.) but it doesn’t have this information, so it uses the default (public cloud). Hence `GetAccountsAsync()` returns 0 accounts, because all your accounts are Fairfax.

To solve this, just add `WithAuthority` to the `ConfidentialClientApplicationBuilder`, even if you don’t know the tenant (e.g. use `common`), since you override it anyway at the request level. But the app object must be tied to the right cloud.

## A Null Pointer Exception is thrown when accessing a certificate

### Symptom

A Null Pointer Exception is thrown when MSAL tries to use a provided certificate.

### Solution

In code, verify that a certificate instance that is provided to MSAL is not disposed of prematurely. This can occur when a certificate instance is used inside of a `using` block, which disposes of it once it goes out of scope.

## `Keyset does not exist` exception

### Symptom

A `Keyset does not exist` cryptographic exception is thrown when MSAL tries to use a provided certificate.

### Solution

One possible solution is to make sure the certificate provided has a private key and correct permissions are given to the running applications to view it.

Another scenario is when the app is deployed to App Service or Azure Function instance and the certificate is rotated. The default behavior of App Service is to delete the old certificate from the certificate store, along with the key container, and install the new certificate. If the app has cached the old certificate instance, there can be a time when the old certificate gets deleted due to rotation but the code still uses the cache. So when the cached certificate tries to access the key container, which is now deleted, it gets “Keyset not found” error.

Set the app setting `WEBSITE_RECYCLE_ON_CERT_ROTATION` to `1`, which will ensure that your worker processes are recycled after a certificate rotation. If you do not set the above app setting, or cannot for some reason (like needing to minimize recycles on a heavy app where recycles have impact on availability), then you can use the app setting `WEBSITE_DELAY_CERT_DELETION` to `1`. You will need to use this setting in conjunction with making sure to pick the right certificate when you look up your certificate (e.g., by looking for the certificate with the latest expiration date).

With this setting present, upon certificate rotation, we will keep the old certificate around until any worker processes that started prior to the rotation (and so may have a handle to the old certificate) have not exited. Note that with this setting, in case of emergency certificate rotation scenarios (where you want your app to stop using an old certificate as soon as possible because it may be compromised), you will need to force a worker process recycle (the easiest way to do this would be to use the site-restart API or Portal’s Restart button).
