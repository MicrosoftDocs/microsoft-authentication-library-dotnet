---
title: Contributing to MSAL.NET
---

# Contributing to MSAL.NET

Microsoft Authentication Library (MSAL) for .NET welcomes new contributors.  This document will guide you through the process.

## Contributor License agreement

Please visit [https://cla.microsoft.com/](https://cla.microsoft.com/) and sign the Contributor License Agreement.  You only need to do that once. We can not look at your code until you've submitted this request.

## Building and testing the library

Please see the [Build and test](build-and-test.md) page.

## Tests

It's all standard stuff, but please note that you won't be able to run integration tests locally because they connect to a KeyVault to fetch some test users and passwords. The CI will run them for you.

## How the MSAL team deals with forks

The CI build will not run on a PR opened from a fork, as a security measure. The MSAL team will manually move your branch from your fork to the main repository, to be able to run the CI. This will preserve the identity of the commit.

```bash
# list existing remotes
git remote -v 

# add a remote to the fork of the contributor
git remote add joe joes_repo_url

# sync
git fetch joe

# checkout the contributor's branch 
git checkout joes_feature_branch

# push it to the original repository (AzureAD/MSAL)
git push origin
```
