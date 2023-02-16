## Token Cache

In MSAL.NET like in ADAL.NET, a token cache is provided by default. The serialization is provided by default by the ADAL and MSAL libraries for a certain number of platforms where a secure storage is available for a user as part of the platform. This is for instance the case of UWP, Xamarin.iOS, Xamarin.Android. 
> Note that when you migrate a Xamarin.Android project from MSAL.NET 1.x to MSAL.NET 2.x, you might want to add | `android:allowBackup="false"` to your project to avoid old cached token keeping coming back because Visual Studio deployments are triggering a restore of local storage. See [#659](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/659#issuecomment-436181938)

In the case of .NET Framework and .NET core, libraries also provide a default cache but this only lasts for the duration of the application. To understand why, remember that ADAL|MSAL .NET desktop/core applications can be Web applications or Web API, which might use some specific caches mechanisms like databases. To have a persistent token cache application in .NET Desktop or Core developers need to customize the serialization. The way to do it, is different in ADAL.NET and MSAL.NET, though.

## Custom token cache serialization in MSAL.NET

This feature is not available on mobile platforms (UWP, Xamarin.iOS, Xamarin.Android) because MSAL already defines a secure and performant serialization mechanism. Net desktop and .Net core applications, on the other hand, have varied architectures, and MSAL cannot implement a serialization mechanism that fits all purposes (e.g. web sites may choose to store tokens in a Redis cache, desktop apps in an encrypted file etc. )

The classes involved in TokenCache serialization are the following:
- ``TokenCache``. MSAL.NET exposes for this class way less public methods and properties than ADAL.NET. Also it's a sealed class and the token cache "events" delegates (as in the case of ADAL.NET) are not available directly in TokenCache, but are defined in an extension class: ``TokenCacheExtensions`` (see below)
- ``TokenCacheNotification`` and the associated even args ``TokenCacheNotificationArgs``. Here again the ``TokenCacheNotificationArgs`` only provides the ``ClientId`` of the application and a reference to the user for which the token is available
- ``TokenCacheExtensions``* which is unique to MSAL.NET, provides extension methods for the TokenCache, that application developers need to use only when overriding the serialization.

![image](https://user-images.githubusercontent.com/13203188/37083057-3dc4a566-21e6-11e8-86a4-34de333fe820.png)

You probably understood it, in MSAL.NET, the idea is this time that the application developer won't inherit from ``TokenCache`` (it's sealed), but, given an instance of Token cache, s/he will set delegates on the token cache to react to ``BeforeAccess`` and ``AfterAccess`` "events". The`` BeforeAccess`` delegate is responsible to deserialize the cache, whereas the AfterAccess is responsible of serializing the cache. 

### Token cache for a public client application

Since MSAL V2.x you have two options, depending on if you want to serialize the cache only to the MSAL.NET format (unified format cache which is common with MSAL, but also across the platforms), or if you also want to also support the [legacy](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Token-cache-serialization) Token cache serialization of ADAL V3.

The customization of Token cache serialization to share the SSO state between ADAL.NET 3.x, ADAL.NET 4.x and MSAL.NET is explained part of the following sample: [active-directory-dotnet-v1-to-v2](https://github.com/Azure-Samples/active-directory-dotnet-v1-to-v2)

> Note: the MSAL.NET 1.1.4-preview token cache format is no longer supported in MSAL 2.x. If you have applications leveraging MSAL.NET 1.x, your users will have to re-sign-in. On the other hand the migration from ADAL 4.x (and 3.x) is supported.

#### Simple token cache serialization (MSAL only)

Below is an example of a naive implementation of custom serialization of a token cache (here the user token cache for a public client application).
The constructor gets the token cache by calling ``TokenCacheHelper.GetUserCache()``

```CSharp
PublicClientApplication _clientApp = new PublicClientApplication(ClientId, Authority, 
                                               TokenCacheHelper.GetUserCache());

```
This helper class looks like the following. 

```C#
static class TokenCacheHelper
{

    /// <summary>
    /// Get the user token cache
    /// </summary>
    /// <returns></returns>
    public static TokenCache GetUserCache()
    {
        if (usertokenCache == null)
        {
            usertokenCache = new TokenCache();
            usertokenCache.SetBeforeAccess(BeforeAccessNotification);
            usertokenCache.SetAfterAccess(AfterAccessNotification);
        }
        return usertokenCache;
    }

    static TokenCache usertokenCache;

    /// <summary>
    /// Path to the token cache
    /// </summary>
    public static readonly string CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin";

    public static void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
     args.TokenCache.Deserialize(File.Exists(CacheFilePath)
        ? ProtectedData.Unprotect(File.ReadAllBytes(CacheFilePath),
                                  null,
                                  DataProtectionScope.CurrentUser)
        : null);
    }

    public static void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        // if the access operation resulted in a cache update
        if (args.HasStateChanged)
        {
         // reflect changesgs in the persistent store
         File.WriteAllBytes(CacheFilePath,
                            ProtectedData.Protect(args.TokenCache.Serialize(),
                                                  null,
                                                  DataProtectionScope.CurrentUser)
                           );
        }
    }
}
```

#### Dual token cache serialization (MSAL unified cache + ADAL V3)

If you want to implement token cache serialization both with the Unified cache format (common to ADAL.NET 4.x and MSAL.NET 2.x, and with other MSAL of the same generation or older, on the same platform), you can get inspired by the following code:

```CSharp
string cacheFolder = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"..\..\..\..");
string adalV3cacheFileName = Path.Combine(cacheFolder, "cacheAdalV3.bin");
string unifiedCacheFileName = Path.Combine(cacheFolder, "unifiedCache.bin");
TokenCache tokenCache = FilesBasedTokenCacheHelper.GetUserCache(unifiedCacheFileName, adalV3cacheFileName);
```

This time the helper class looks like the following:

```CSharp
using System.IO;
using System.Security.Cryptography;
using Microsoft.Identity.Client;
using Microsoft.Identity.Core.Cache;

namespace CommonCacheMsal2
{
    /// <summary>
    /// Simple persistent cache implementation of the dual cache serialization (ADAL V3 legacy
    /// and unified cache format) for a desktop applications (from MSAL 2.x)
    /// </summary>
    static class FilesBasedTokenCacheHelper
    {
        /// <summary>
        /// Get the user token cache
        /// </summary>
        /// <param name="adalV3CacheFileName">File name where the cache is serialized with the ADAL V3 token cache format. Can
        /// be <c>null</c> if you don't want to implement the legacy ADAL V3 token cache serialization in your MSAL 2.x+ application</param>
        /// <param name="unifiedCacheFileName">File name where the cache is serialized with the Unified cache format, common to
        /// ADAL V4 and MSAL V2 and above, and also accross ADAL/MSAL on the same platform. Should not be <c>null</c></param>
        /// <returns></returns>
        public static TokenCache GetUserCache(string unifiedCacheFileName, string adalV3CacheFileName)
        {
            UnifiedCacheFileName = unifiedCacheFileName;
            AdalV3CacheFileName = adalV3CacheFileName;
            if (usertokenCache == null)
            {
                usertokenCache = new TokenCache();
                usertokenCache.SetBeforeAccess(BeforeAccessNotification);
                usertokenCache.SetAfterAccess(AfterAccessNotification);
            }
            return usertokenCache;
        }

        /// <summary>
        /// Token cache
        /// </summary>
        static TokenCache usertokenCache;

        /// <summary>
        /// File path where the token cache is serialiazed with the unified cache format (ADAL.NET V4, MSAL.NET V3)
        /// </summary>
        public static string UnifiedCacheFileName { get; private set; }

        /// <summary>
        /// File path where the token cache is serialiazed with the legacy ADAL V3 format
        /// </summary>
        public static string AdalV3CacheFileName { get; private set; }

        public static void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
                CacheData cacheData = new CacheData
                {
                    UnifiedState = ReadFromFileIfExists(UnifiedCacheFileName),
                    AdalV3State = ReadFromFileIfExists(AdalV3CacheFileName)
                };
                args.TokenCache.DeserializeUnifiedAndAdalCache(cacheData);
        }

        public static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                    CacheData cacheData = args.TokenCache.SerializeUnifiedAndAdalCache();

                    // reflect changesgs in the persistent store
                    WriteToFileIfNotNull(UnifiedCacheFileName, cacheData.UnifiedState);
                    if (!string.IsNullOrWhiteSpace(AdalV3CacheFileName))
                    {
                        WriteToFileIfNotNull(AdalV3CacheFileName, cacheData.AdalV3State);
                    }
            }
        }

        /// <summary>
        /// Read the content of a file if it exists
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>Content of the file (in bytes)</returns>
        private static byte[] ReadFromFileIfExists(string path)
        {
            byte[] protectedBytes = (!string.IsNullOrEmpty(path) && File.Exists(path)) ? File.ReadAllBytes(path) : null;
            byte[] unprotectedBytes = (protectedBytes != null) ? ProtectedData.Unprotect(protectedBytes, null, 
                                                                 DataProtectionScope.CurrentUser) : null;
            return unprotectedBytes;
        }

        /// <summary>
        /// Writes a blob of bytes to a file. If the blob is <c>null</c>, deletes the file
        /// </summary>
        /// <param name="path">path to the file to write</param>
        /// <param name="blob">Blob of bytes to write</param>
        private static void WriteToFileIfNotNull(string path, byte[] blob)
        {
            if (blob != null)
            {
                byte[] protectedBytes = ProtectedData.Protect(blob, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(path, protectedBytes);
            }
            else
            {
                File.Delete(path);
            }
        }
    }
}
```


### Token cache for a Web app (confidential client application)
In the case of Web Apps or Web APIs, the cache can be very different, leveraging the session, or a Redis cache, or a database. 
Below is an example of Session cache extracted from the [Models/MSALSesssionCache.cs](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2/blob/master/WebApp/Models/MSALSessionCache.cs) file in the [Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2) code sample.

The way to use it is illustrated in the [HomeController#L56](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2/blob/c2087374e849fd58b5bf75ffebef1ac0e106884d/WebApp/Controllers/HomeController.cs#L56)

```C#
TokenCache userTokenCache = new MSALSessionCache(signedInUserID, this.HttpContext).GetMsalCacheInstance();            
ConfidentialClientApplication cca = new ConfidentialClientApplication(clientId, 
                                                                      redirectUri,
                                                                      new ClientCredential(appKey), 
                                                                      userTokenCache, null);
```

And the implementation of the Token cache itself is the following:
```C#
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace WebApp_OpenIDConnect_DotNet.Models
{
    public class MSALSessionCache
    {
        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        string UserId = string.Empty;
        string CacheId = string.Empty;
        HttpContextBase httpContext = null;

        TokenCache cache = new TokenCache();

        public MSALSessionCache(string userId, HttpContextBase httpcontext)
        {
            // not object, we want the SUB
            UserId = userId;
            CacheId = UserId + "_TokenCache";
            httpContext = httpcontext;
            Load();
        }

        public TokenCache GetMsalCacheInstance()
        {
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            Load();
            return cache;
        }

        public void SaveUserStateValue(string state)
        {
            SessionLock.EnterWriteLock();
            httpContext.Session[CacheId + "_state"] = state;
            SessionLock.ExitWriteLock();
        }
        public string ReadUserStateValue()
        {
            string state = string.Empty;
            SessionLock.EnterReadLock();
            state = (string)httpContext.Session[CacheId + "_state"];
            SessionLock.ExitReadLock();
            return state;
        }
        public void Load()
        {
            SessionLock.EnterReadLock();
            cache.Deserialize((byte[])httpContext.Session[CacheId]);
            SessionLock.ExitReadLock();
        }

        public void Persist()
        {
            SessionLock.EnterWriteLock();

            // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
            // Reflect changes in the persistent store
            httpContext.Session[CacheId] = cache.Serialize();
            SessionLock.ExitWriteLock();
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                Persist();
            }
        }
    }
}
```
## Some of the samples illustrating token cache serialization
Sample | Platform | Description
------ | -------- | -----------
[active-directory-dotnet-desktop-msgraph-v2](http://github.com/azure-samples/active-directory-dotnet-desktop-msgraph-v2) | Desktop (WPF) | Windows Desktop .NET (WPF) application calling the Microsoft Graph API. ![](https://github.com/Azure-Samples/active-directory-dotnet-desktop-msgraph-v2/blob/master/ReadmeFiles/Topology.png)
[active-directory-dotnet-v1-to-v2](https://github.com/Azure-Samples/active-directory-dotnet-v1-to-v2) | Desktop (Console) | Set of Visual Studio solutions illustrating the migration of Azure AD v1.0 applications (using ADAL.NET) to Azure AD v2.0 applications, also named converged applications (using MSAL.NET), in particular [Token Cache Migration](https://github.com/Azure-Samples/active-directory-dotnet-v1-to-v2/blob/master/TokenCacheMigration/README.md)