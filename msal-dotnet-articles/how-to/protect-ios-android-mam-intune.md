---
title: Protecting iOS and Android applications with InTune
---

# Protecting iOS and Android applications with InTune

## Scenario

There are scenarios when just user authentication may not be sufficient to protect certain resources. The device that accesses it should also be compliant as per policies defined in Intune.  
Azure Active Directory (AAD) ensures that the access token is not issued till the device is compliant as per the conditional access policy. This page explains how a resource can be reached by MSAL.NET while being protected by Intune [Mobile Application Management (MAM)](/mem/intune/fundamentals/deployment-guide-enrollment-mamwe).

## Overview of the system configuration

The system comprises of two apps: a backend app that provides access to a hosted resource and a client App that needs to access the resource.The resource is defined by scope. When the client app needs the resource, it will request access to the scope.  

Azure Active Directory (AAD) protects the resource by applying conditional access on the resource. One of the conditions of the access is to have App Protection Policy on the client App.  

An App protection policy can be created in the Intune Portal for an App and it can be applied to one or more user groups.  

Here are the detail [setup steps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Steps-to-create-config-for-MAM-(Conditional-access))  .

## Workflow for iOS

As a result of the setup, when App attempts to reach the resource and if the device is not compliant, AAD returns ```protection_policy_required``` suberror.  

MSAL.NET catches the error and throw ```IntuneAppProtectionPolicyRequiredException```.  

It is app's responsibility to catch the error and invoke Intune MAM SDK to make the device compliant. When the device becomes compliant, Intune MAM SDK will write enrollmentID in the keychain.  

The App can then call the silent token acquisition method of MSAL.NET to obtain the token. MSAL.NET will retrieve the enrollment ID from the keychain and call the backend.

This will return the access token.

### Code snippets

App code that seeks access to protected scope "Hello.World"

```csharp
// The following parameters are for sample app in lab4. Please configure them as per your app registration.
            // And also update corresponding entries in info.plist -> IntuneMAMSettings -> ADALClientID and ADALRedirectUri
            string clientId = "bd9933c9-a825-4f9a-82a0-bbf23c9049fd";
            string redirectURI = $"msauth.com.xamarin.microsoftintunemamsample://auth";
            string tenantID = "f645ad92-e38d-4d1a-b510-d1b09a74a8ca";
            string[] Scopes = { "api://a8bf4bd3-c92d-44d0-8307-9753d975c21e/Hello.World" }; // needs admin consent
            string[] clientCapabilities = { "ProtApp" }; // Important: This must be passed to the PCABuilder

           try
            {
                    string authority = $"https://login.microsoftonline.com/{tenantID}/";
                    var pcaBuilder = PublicClientApplicationBuilder.Create(clientId)
                                                                        .WithRedirectUri(redirectURI)
                                                                        .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                                                                        .WithLogging(MSALLogCallback, LogLevel.Verbose)
                                                                        .WithAuthority(authority)
                                                                        .WithClientCapabilities(clientCapabilities)
                                                                        .WithHttpClientFactory(new HttpSnifferClientFactory())
                                                                        .WithBroker(true);
                    PCA = pcaBuilder.Build();
                }

                // attempt silent login.
                // If this is very first time and the device is not enrolled, it will throw MsalUiRequiredException
                // If the device is enrolled, this will succeed.
                var authResult = await DoSilentAsync(Scopes).ConfigureAwait(false);
                ShowAlert("Success Silent 1", authResult.AccessToken);
            }
            catch (MsalUiRequiredException _)
            {
                // This executes UI interaction
                try
                {
                    var interParamBuilder = PCA.AcquireTokenInteractive(Scopes)
                                                .WithParentActivityOrWindow(this)
                                                .WithUseEmbeddedWebView(true);

                    var authResult = await interParamBuilder.ExecuteAsync().ConfigureAwait(false);
                    ShowAlert("Success Interactive", authResult.AccessToken);
                }
                catch (IntuneAppProtectionPolicyRequiredException ex)
```

App code catches the exception and calls MAM SDK to make the App compliant. It will wait for the compliance.

```csharp
 catch (IntuneAppProtectionPolicyRequiredException ex)
            {
                _manualReset.Reset();

                IntuneMAMComplianceManager.Instance.RemediateComplianceForIdentity(ex.Upn, false);
                _manualReset.WaitOne();
            }
```

After the App becomes compliant, it will be notified in a delegate. The delegate will set the flag on the semaphore.

```csharp
public async override void IdentityHasComplianceStatus(string identity, IntuneMAMComplianceStatus status, string errorMessage, string errorTitle)
        {
            if (status == IntuneMAMComplianceStatus.Compliant)
            {
                _manualReset.Set();
            }
        }
```

When the semaphore is released, App should call the Silent token acquisition method.

```csharp
 var accts = await PCA.GetAccountsAsync().ConfigureAwait(false);
        var acct = accts.FirstOrDefault();
        if (acct != null)
        {
            try
            {
                var silentParamBuilder = PCA.AcquireTokenSilent(Scopes, acct);
                var authResult = await silentParamBuilder.ExecuteAsync().ConfigureAwait(false);
                ShowAlert("Success Silent 1", authResult.AccessToken);
            }
        }
```

The complete sample can be found [Here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/tree/d813f674da88d37272d7bd8fe81318d4243e7583/tests/devapps/Intune-xamarin-ios)

## Workflow for Android

As a result of the setup, when App attempts to reach the resource and if the device is not compliant, AAD returns ```protection_policy_required``` suberror.  

MSAL.NET catches the error and throws ```IntuneAppProtectionPolicyRequiredException```.  

It is app's responsibility to catch the error and invoke Intune MAM SDK to make the device compliant. In order for the device to become compliant, the app must register a callback for ```IMAMEnrollmentManager```.  

The callback is provided upn, aaid and resourceID. The resourceID points to the MAM API and callback must return token for the resource using silent token acquistion.  

The app should also register callback for ```MAMNotificationType.MamEnrollmentResult```. After the enrollment succeeds, the App can then call the silent token acquisition method of MSAL.NET to obtain the token. 

This will return the access token.

### Code snippets

Register callback ```OnMAMCreate()```

```csharp
            IMAMEnrollmentManager mgr = MAMComponents.Get<IMAMEnrollmentManager>();
            mgr.RegisterAuthenticationCallback(new MAMWEAuthCallback());

            // Register the notification receivers to receive MAM notifications.
            // Along with other, this will receive notification that the device has been enrolled.
            IMAMNotificationReceiverRegistry registry = MAMComponents.Get<IMAMNotificationReceiverRegistry>();
            registry.RegisterReceiver(new EnrollmentNotificationReceiver(), MAMNotificationType.MamEnrollmentResult);
```

App code that seeks access to protected scope "Hello.World" calls method in a wrapper class that wraps PCA.

```csharp
            try
            {
                // attempt silent login.
                // If this is very first time and the device is not enrolled, it will throw MsalUiRequiredException
                // If the device is enrolled, this will succeed.
                result = await PCAWrapper.Instance.DoSilentAsync(Scopes).ConfigureAwait(false);

                _ = await ShowMessage("Silent 1", result.AccessToken).ConfigureAwait(false);
            }
            catch (MsalUiRequiredException )
            {
                try
                {
                    // This executes UI interaction
                    result = await PCAWrapper.Instance.DoInteractiveAsync(Scopes, this).ConfigureAwait(false);

                    _ = await ShowMessage("Interactive 1", result.AccessToken).ConfigureAwait(false);
                }
                catch (IntuneAppProtectionPolicyRequiredException exProtection)
                {
                    // if the scope requires App Protection Policy,  IntuneAppProtectionPolicyRequiredException is thrown.
                    // Perform registration operation here and then do the silent token acquisition
                    _ = await DoMAMRegister(exProtection).ContinueWith(async (s) =>
                      {
                          try
                          {
                              // Now the device is registered, perform silent token acquisition
                              result = await PCAWrapper.Instance.DoSilentAsync(Scopes).ConfigureAwait(false);

                              _ = await ShowMessage("Silent 2", result.AccessToken).ConfigureAwait(false) ;
                          }
                          catch (Exception ex)
                          {
                              _ = await ShowMessage("Exception 1", ex.Message).ConfigureAwait(false);
                          }
                      }).ConfigureAwait(false);
                }
            }
```

Code for MAM registration is as follows

```csharp
        private async Task DoMAMRegister(IntuneAppProtectionPolicyRequiredException exProtection)
        {
            // reset the registered event
            IntuneSampleApp.MAMRegsiteredEvent.Reset();
            
            // Invoke compliance API on a different thread
            await Task.Run(() =>
                                {
                                    IMAMComplianceManager mgr = MAMComponents.Get<IMAMComplianceManager>();
                                    mgr.RemediateCompliance(exProtection.Upn, exProtection.AccountUserId, exProtection.TenantId, exProtection.AuthorityUrl, false);
                                }).ConfigureAwait(false);

            // wait till the registration completes
            // Note: This is a sample app for MSAL.NET. Scenarios such as what if enrollment fails or user chooses not to enroll will be as
            // per the business requirements of the app and not considered in the sample app.
            IntuneSampleApp.MAMRegsiteredEvent.WaitOne();
        }
```

After the App becomes compliant, it will be notified in a callback. The callback will set the flag on the semaphore. This will unblock `DoMAMRegister`.

```csharp
            if (notification.Type == MAMNotificationType.MamEnrollmentResult)
            {
                IMAMEnrollmentNotification enrollmentNotification = notification.JavaCast<IMAMEnrollmentNotification>();
                MAMEnrollmentManagerResult result = enrollmentNotification.EnrollmentResult;

                if (result.Equals(MAMEnrollmentManagerResult.EnrollmentSucceeded))
                {
                    // this signals that MAM registration is complete and the app can proceed
                    IntuneSampleApp.MAMRegsiteredEvent.Set();
                }
            }}
```

The complete sample can be found [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/tree/master/tests/devapps/Intune-xamarin-Android).
