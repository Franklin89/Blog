---
title:  OTA-Library
author: Matteo
categories:
published: 2016-12-20 02:12am
description: Open Travel Alliance Class Library for .Net
tags: OTA;Open Travel Alliance
---

![Icon](https://raw.githubusercontent.com/Franklin89/OTA-Library/master/docs/ota.png)

### What is it?

The OTA-Library is a collection for .Net classes that can be used to implement interfaces that comply to the Open Travel Alliance.

### Motivation

I am currently implementing an interface to _Feratel Deskline_ which follows the specifications of the _Open Travel Alliance_. The _Open Travel Alliance_ only define how the communication shall be handled and has a set of _*.xsd_ files that can be downloaded from [OpenTravel.org](http://opentravel.org). Converting these _*.xsd_ files to C# classes might sound easier than it is. There are so many classes and they are not very well structured. To make it easier for me and maybe some other developers I converted all the _*.xsd_ to classes.

### Where can I get it?

Get it from NuGet. You can simply install it with the Package Manager console:

```
    PM> Install-Package OTA-Library
```