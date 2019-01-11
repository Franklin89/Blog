Title: SQL Server Management Studio refreshing IntelliSense Cache
Published: 8/22/2018
Image: /images/home-bg.jpg
Tags: 
    - Wyam
    - Blog
---

If you are writing a sql query in SQL Server Management Studio and you had new columns through Entity Framework Migrations for example the IntelliSense becomes stale. Meaning that the new columns are there but you will get the red squiggly lines. This can cause some confusion but there are two simple solutions.

1. Go to `Edit` -> `IntelliSense` -> `Refresh Local Cache`
1. Hit `CTRL + Shift + R`

This will refresh the Local Cache and the red squiggly lines should disappear.
