---
title: Creating configuration for Intune Mobile App Management conditional access
description: "This scenario includes a backend application, and an iOS and Android client applications. This article describes the steps to correctly configure these applications for Intune MAM."
author: cilwerner
manager: 
ms.author: cwerner
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: how-to
ms.custom: 
#Customer intent: 
---

# Creating configuration for Intune Mobile App Management conditional access

## Scenario

There is a scenario when a user of a client application wants to access resources protected by specific permissions (i.e., scopes) in a backend application. The resource is accessible only when certain app protection policies and access conditions are met. In such situation, an access token is issued only when the conditions are met.  

This scenario includes a backend application, and an iOS and Android client applications. The setup for the two platforms is slightly different. This article describes the steps to correctly configure these applications for the above scenario to work. At the same time, the article avoids going into granular details. Read more in [Intune Mobile App Management](/mem/intune/apps/app-management).

## Setup application

### Setup User and Group for testing

1. Sign in to [Microsoft Entra ID](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/Overview).
2. Create a test user (e.g. `XamTestuser@XamTester.onmicrosoft.com`).
3. On the user profile page, go to **Licenses**.
4. Click on **Assignments** and select the following:
    - Microsoft Entra ID P1 or P2 License
    - Enterprise Mobility + Security
    - Intune
    - Microsoft 365 Business standard

    >[!NOTE]
    >These policies do not apply to guest users.
5. Create a test group (e.g. `MAM_Test_Users`).

    >[!NOTE]
    >Remember the name of the group. This will need to be assigned at later stages.

6. Add the new user to this group.

### Setup Enterprise App and Conditional Access policy

Register a backend application:

1. In [Microsoft Entra ID](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/Overview), go to Enterprise Applications section.
2. Click on **New application**.
3. Click **Create your own application**.
4. Select **Register an application to integrate with Microsoft Entra ID (App you're developing)** option.
5. After **Create** screen, it will take you to **Register An Application** screen.
6. Select **Multitenant** and click **Register**.
7. This will take you to the screen in #1.

Enable conditional access:

1. Navigate to **Enterprise Applications**.
2. Select the application that you created.
3. Assign the user group that was created earlier.
4. Click on **Conditional Access**.
5. Click **New policy** and select:
    - In **Users or workload identities**, select the group that was created earlier.
    - In **Cloud Apps or actions**, verify that this has the enterprise application that was created.
    - In **Conditions**, select multiple options:
        - **Device Platforms** - select **Yes** and choose **iOS + Android**.
        - **Client Apps** - select **Yes** and select all the options.
    - In **Grant**, select **Require app protection policy**.
    - At the bottom of the screen in **Enable Policy**, select **On**.
6. Click **Create**.

Configure permissions (e.e. scopes):

1. Navigate to **App registrations**. (Note: This is not **Enterprise applications**.)
2. Select the app you created.
3. Click on "**Add Application ID URI**".
4. Click on **Add a scope** (e.g. Hello.World).
5. It will generate a Guid and **Application ID Uri** and ask you to create a scope.
6. Note the URI of the scope. This is needed in the client application.
7. Click on **API Permissions** section.
8. Click on **Add a permission**.
9. Select the permission created in the earlier stage and click **Add Permission**.
10. Click **Add a permission** again.
11. Select **APIs my organization uses**.
12. Select **Microsoft Mobile Application Management - DeviceManagementManagedApps.ReadWrite**.
13. Click **Add permission**.
14. **Grant Admin** consent.

### Setup Client Apps

1. In [Microsoft Entra ID](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/Overview), go to **App registration**.
2. Create a new app and choose **multitenant** option.
3. Add platform URI for iOS.
4. Add platform URI for Android.
5. Go to **API Permissions**.
6. Add permissions for the scope created in the backend application:
    1. Click on **Add a Permission**.
    2. Choose **My APIs**.
    3. Select the scope that was added in the backend app (i.e. Hello.World).
    4. Click **Add a permission** again
    5. Select **APIs my organization uses**.
    6. Select **Microsoft Mobile Application Management - DeviceManagementManagedApps.ReadWrite**.
    7. Click **Add permission**.
    8. Select **Grant admin consent for your tenant** (even if **Admin consent required** column shows **No**).

## Setup in Intune

### Build the iOS Client App

1. Build a skeleton app with the client ID from the Microsoft Entra ID.
2. Make sure that the iOS app references **Xamarin.Intune.MAM.SDK.iOS** package.
3. For iOS, the IPA file should be built.

### Build the Android Client App

1. Build a skeleton app with the client ID from the Microsoft Entra ID.
2. Make sure that the Android app references **Microsoft.Intune.MAM.Xamarin.Android** package.
3. For Android, the APK file should be built.

### Setup Apps in Intune

You will have to follow the same steps twice, once for the iOS app and again for the Android app with the differences as noted.

1. Go to In **[Intune Portal](https://endpoint.microsoft.com/).**
2. Create an app:
    - For iOS, click on **Apps** > **iOS Apps** section.
    - For Android, click on **Apps** -> **Android Apps** section.
3. Select **Add**.
4. Select **App Type** as **Line-of-business app**.
5. Upload the build file.
    - For iOS, select the .ipa file that was built.
    - For Android, select the .apk file that was built.
6. You may need to fill out some information in the **App information**, like **Publisher** name.
7. In the **Assignments** screen, under **Available for enrolled devices**:
    - Select **Add all users**.
8. In the **Assignments** screen, under **Available with or without enrollment**:
    - Select **Add Group**.
    - Select the group that was created.
9. Select **Create** to complete the client application registration.

### Setup App Protection Policy in Intune

You will have to follow the same steps twice, once for the iOS app and again for the Android app with the differences as noted.

1. Go to In **[Intune Portal](https://endpoint.microsoft.com/).**
2. Go to **Apps** > **App Protection policies**:
    - For iOS, click **Create Policy iOS/MacOS**.
    - For Android, click **Create Policy Android**.
3. After the **Basic** screen, navigate to the **Apps** screen.
4. In the **Apps** screen, set **Target policy** to **Selected Apps**.
5. Under **Custom Apps**, select the app you created.
6. On **Data Protection** screen, select the options you want, for example:
    - **Send org data to other apps** - **Policy managed apps**.
    - **Save copies of org data** - **block**.
7. Click **Next** to advance to **Access requirements** screen.
8. In **Access requirements**, keep the default values.
9. In **Conditional Launch**, keep the default values.
10. In **Assignments** > **Included groups**, add the group you created.
