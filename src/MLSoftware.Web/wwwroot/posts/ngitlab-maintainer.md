---
title: New NGitLab maintainer
author: Matteo
published: 2016-09-14 08:17pm
description: New NGitLab maintainer
---

Couple of days ago I became the new maintainer of [NGitLab](). And here is my first blog post about it telling you what will change in the future.

### What is NGitLab?
_NGitLab_ is a .NET REST client implementation of the GitLab API.

### Roadmap - vNext

Next steps:

- Support for Net Standard (This way it can also be used on in .Net Core apps or in Xamarin)
- Integration Tests using Docker
    - Depending on the support in AppVeyor, this might only be possible offline and not in continuous integration.
- New interface for the _GitLabClient_

### Contribution

Anybody is welcome to contribute and help on this project. I will try and get back to anybody as soon as possible. Open issues, fork the repo and send pull requests.

### Where can I get it?

Get it from NuGet. You can simply install it with the Package Manager console:

```
    PM> Install-Package NGitLab
```