---
title: Troubleshooting brokered applications
description: Master brokered authentication on Android with our troubleshooting guide. Learn about redirect URIs, broker versions, precedence, and log acquisition.
ms.service: msal
ms.subservice: msal-dotnet
ms.date: 05/20/2025
---

# Troubleshooting brokered applications

## Tips for Android brokered authentication

Here are a few tips on avoiding issues when you implement brokered authentication on Android:

- **Redirect URI** - Add a redirect URI to your application registration in the [Azure portal](https://portal.azure.com/). A missing or incorrect redirect URI is a common issue encountered by developers.
- **Broker version** - Install the minimum required version of the broker apps. Either of these two apps can be used for brokered authentication on Android.
  - [InTune Company Portal](https://play.google.com/store/apps/details?id=com.microsoft.windowsintune.companyportal) (version 5.0.4689.0 or greater)
  - [Microsoft Authenticator](https://play.google.com/store/apps/details?id=com.azure.authenticator) (version 6.2001.0140 or greater).
- **Broker precedence** - MSAL communicates with the *first broker installed* on the device when multiple brokers are installed.

    Example: If you first install Microsoft Authenticator and then install Intune Company Portal, brokered authentication will *only* happen on the Microsoft Authenticator.
- **Logs** - If you encounter an issue with brokered authentication, viewing the broker's logs might help you diagnose the cause.
  - Acquiring Microsoft Authenticator logs:

    1. Select the menu button in the top-right corner of the app.
    1. Select **Send Feedback** > **Having Trouble?**.
    1. Select one of the options under **What are you trying to do?** to add a description
    1. You can then hit the arrow on the top right of the screen to send the logs.
    1. Once you send the logs you will be presented with a popup that will contain an **Incident ID**. Please provide this incident ID when requesting assistance.

  - Acquiring Intune Company Portal logs:

    1. Select the menu button on the top-left corner of the app
    1. Select **Help** > **Email Support**
    1. Select **Upload Logs Only** to send the logs.
    1. Once you send the logs you will be presented with a popup that will contain an **Incident ID**. Please provide this incident ID when requesting assistance.