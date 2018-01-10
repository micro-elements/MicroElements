///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var solutionFile    = "./MicroElements.sln";
string baseVersion  = "1.0.5";
var target          = Argument("target", "Default");
var configuration   = Argument("configuration", "Release");

var buildSuffix     = "beta-20180103";
string nugetVersion = baseVersion;
/*
master: 1.0.0
dev   : 1.0.0-beta
other : no nuget
*/
//////////////////////////////////////////////////////////////////////
// FILES & DIRECTORIES
//////////////////////////////////////////////////////////////////////

var rootDir                     = Directory("./");
var artifactsDir                = Directory("./artifacts");
var testResultsDir              = artifactsDir + Directory("test-results");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(testResultsDir);
    CleanDirectory(artifactsDir);

    var delSettings = new DeleteDirectorySettings(){Recursive=true, Force=true};
    GetDirectories("./src/**/bin/Debug").ToList().ForEach(dir=>DeleteDirectory(dir, delSettings));
    GetDirectories("./src/**/bin/Release").ToList().ForEach(dir=>DeleteDirectory(dir, delSettings));
    //GetDirectories("./src/**/obj/Debug").ToList().ForEach(dir=>DeleteDirectory(dir, delSettings));
    //GetDirectories("./src/**/obj/Release").ToList().ForEach(dir=>DeleteDirectory(dir, delSettings));

    DeleteFiles("./src/**/*.nupkg");
});

Task("Restore")
    .IsDependentOn("Clean")    
    .Does(() =>
{	
	var settings = new DotNetCoreRestoreSettings
    {        
        Sources = new [] { "https://api.nuget.org/v3/index.json" }
    };

	//DotNetCoreRestore(solutionFile, settings);
});

Task("Build")    
    .IsDependentOn("Restore")
    .Does(() =>
{	
	var settings = new DotNetCoreBuildSettings 
    { 
        Configuration = configuration,
        ArgumentCustomization =
          args => args
            .Append("/p:NugetVersion=" + nugetVersion)
            .Append("/p:SourceLinkCreate=true")
    };

	DotNetCoreBuild(solutionFile, settings);
});

Task("Test")
    .Does(() =>
{
    var test_projects = GetFiles("./src/*.Tests/*.csproj");
    foreach(var test_project in test_projects)
    {
        var testSettings = new DotNetCoreTestSettings()
        {
            Configuration = configuration,
            NoBuild = true
        };
        DotNetCoreTest(test_project.FullPath, testSettings);
    }
});

Task("CopyPackages")
    .IsDependentOn("Build")
    .Does(() =>
{
    var files = GetFiles("./src/**/*.nupkg");
    CopyFiles(files, artifactsDir);
});

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("CopyPackages");

RunTarget(target);
