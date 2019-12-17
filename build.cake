#tool "nuget:https://api.nuget.org/v3/index.json?package=Wyam&version=2.2.9"
#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.Wyam&version=2.2.9"
#addin "nuget:https://api.nuget.org/v3/index.json?package=NetlifySharp&version=1.1.0"


using NetlifySharp;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .Does(() =>
    {
        Wyam(new WyamSettings
        {
            Recipe = "Blog",
            Theme = "CleanBlog",
            UpdatePackages = true
        });
    });

Task("Preview")
    .Does(() =>
    {
        Wyam(new WyamSettings
        {
            Recipe = "Blog",
            Theme = "CleanBlog",
            UpdatePackages = true,
            Preview = true,
            Watch = true
        });
    });

Task("Deploy")
    .Does(async () =>
    {
        var netlifyToken = EnvironmentVariable("NETLIFY_TOKEN");
        if(string.IsNullOrEmpty(netlifyToken))
        {
            throw new Exception("Could not get Netlify token environment variable");
        }

        var siteId = EnvironmentVariable("NETLIFY_SITE_ID");
        if(string.IsNullOrEmpty(siteId))
        {
            throw new Exception("Could not get Netlify Site Id environment variable");
        }

        Information("Deploying output to Netlify");
        var client = new NetlifyClient(netlifyToken);
        await client.UpdateSiteAsync(MakeAbsolute(Directory("./output")).FullPath, siteId);
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Preview");
    
Task("BuildServer")
    .IsDependentOn("Build")
    .IsDependentOn("Deploy");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);