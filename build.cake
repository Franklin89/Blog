// The following environment variables need to be set for Publish target:
// NETLIFY_TOKEN

#tool "nuget:https://api.nuget.org/v3/index.json?package=Wyam&version=2.0.0"
#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.Wyam&version=2.0.0"
#addin "NetlifySharp"

using NetlifySharp;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

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

Task("Netlify")
    .Does(() =>
    {
        var netlifyToken = EnvironmentVariable("NETLIFY_TOKEN");
        if(string.IsNullOrEmpty(netlifyToken))
        {
            throw new Exception("Could not get Netlify token environment variable");
        }

        Information("Deploying output to Netlify");
        var client = new NetlifyClient(netlifyToken);
        client.UpdateSite($"matteo.netlify.com", MakeAbsolute(Directory("./output")).FullPath).SendAsync().Wait();
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Preview");
    
Task("BuildServer")
    .IsDependentOn("Build")
    .IsDependentOn("Netlify");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);