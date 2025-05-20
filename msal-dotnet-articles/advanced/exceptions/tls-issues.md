---
title: TLS issues
description: "How to diagnose and address TLS issues when using MSAL.NET"
ms.service: msal
ms.subservice: msal-dotnet
---

# TLS issues

## What is happening

Microsoft has an initiative to disable anything less that TLS 1.2 for security reasons. The [Microsoft TLS 1.0 implementation](https://support.microsoft.com/help/3117336/schannel-implementation-of-tls-1-0-in-windows-security-status-update-n) has no known security vulnerabilities. But because of the potential for future protocol downgrade attacks and other TLS vulnerabilities, Office, for instance are [discontinuing](/microsoft-365/compliance/prepare-tls-1.2-in-office-365) support for TLS 1.0 and 1.1 in Microsoft Office 365.

As this initiative is going through, you ask more and more questions about the fact that some services deployed to Azure require TLS 2.0, and this is caught by MSAL.NET. See for instance [#657](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/657)

MSAL.NET already supports TLS 2.0 (as previous versions). Some of you have proposed to set System.Net.ServicePointManager.SecurityProtocol to System.Net.SecurityProtocolType.Tls12, however this is not the right fix as when TLS 1.3 shows up, the apps would have to change.

## What is the right fix?

We suggest you read [Transport Layer Security (TLS) best practices with the .NET Framework](/dotnet/framework/network-programming/tls). The simplest fix would be, if you can, to make sure  your app moves to .NET Framework 4.7+, otherwise the best practices document details your options.
