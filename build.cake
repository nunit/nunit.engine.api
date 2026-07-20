// Load the recipe
#load nuget:?package=NUnit.Cake.Recipe&version=2.0.0-beta.4.2
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../NUnit.Cake.Recipe/src/NUnit.Cake.Recipe/content/*.cake

// Load additional cake files
//#load package-tests.cake
#load KnownExtensions.cake

// Initialize BuildSettings
BuildSettings.Initialize(
    Context,
    title: "NUnit Engine API",
    githubRepository: "NUnit.Engine.Api",
    solutionFile: "NUnit.Engine.Api.sln",
    buildWithMSBuild: true);

//////////////////////////////////////////////////////////////////////
// INDIVIDUAL PACKAGE DEFINITIONS
//////////////////////////////////////////////////////////////////////

//PackageDefinition NUnitExtensibilityApiPackage = new NuGetPackage(
//    id: "NUnit.Extensibility.Api",
//    source: BuildSettings.SourceDirectory + "NUnitCommon/nunit.extensibility.api/nunit.extensibility.api.csproj",
//    checks: new PackageCheck[] {
//        HasFile("LICENSE.txt"),
//        HasDirectory("lib/net462").WithFile("nunit.extensibility.api.dll"),
//        HasDirectory("lib/netstandard2.0").WithFile("nunit.extensibility.api.dll")
//    },
//    symbols: new PackageCheck[] {
//        HasDirectory("lib/net462").WithFile("nunit.extensibility.api.pdb"),
//        HasDirectory("lib/netstandard2.0").WithFile("nunit.extensibility.api.pdb")
//    });

BuildSettings.Packages.Add(new NuGetPackage(
    id: "NUnit.Engine.Api",
    source: BuildSettings.SourceDirectory + "NUnit.Engine.Api/NUnit.Engine.Api.csproj",
    checks: new PackageCheck[] {
        HasFile("LICENSE.txt"),
        HasDirectory("lib/net462").WithFile("nunit.engine.api.dll"),
        HasDirectory("lib/netstandard2.0").WithFile("nunit.engine.api.dll"),
        HasDependency("NUnit.Extensibility.Api", "4.0.0-beta.2.2")
    },
    symbols: new PackageCheck[] {
        HasDirectory("lib/net462").WithFile("nunit.engine.api.pdb"),
        HasDirectory("lib/netstandard2.0").WithFile("nunit.engine.api.pdb")
    }));

Task("BuildPackages")
    .Description("Just build packages, without installing or running package tests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        foreach (var package in BuildSettings.Packages)
            package.BuildPackage();
    });

//////////////////////////////////////////////////////////////////////
// CONSOLE PACKAGE TEST RUNNER
//////////////////////////////////////////////////////////////////////

// Use the console runner we just built to run package tests
public class ConsoleRunnerSelfTester : TestRunner, IPackageTestRunner
{
    private string _executablePath;

    public ConsoleRunnerSelfTester(string executablePath)
    {
        _executablePath = executablePath;
    }

    public int RunPackageTest(string arguments, bool redirectOutput)
    {
        Console.WriteLine($"Running package test with arguments {arguments}");
        return base.RunPackageTest(_executablePath, new ProcessSettings() { Arguments = arguments, RedirectStandardOutput = redirectOutput });
    }
}

//////////////////////////////////////////////////////////////////////
// AGENT CORE PACKAGE TEST RUNNER
//////////////////////////////////////////////////////////////////////

public class DirectTestAgentRunner : TestRunner, IPackageTestRunner
{
    public int RunPackageTest(string arguments, bool redirectOutput)
    {
        // First argument must be relative path to a test assembly.
        // It's immediate directory name is the name of the runtime.
        string testAssembly = arguments.Trim();
        testAssembly = BuildSettings.OutputDirectory + (testAssembly[0] == '"'
            ? testAssembly.Substring(1, testAssembly.IndexOf('"', 1) - 1)
            : testAssembly.Substring(0, testAssembly.IndexOf(' ')));

        if (!System.IO.File.Exists(testAssembly))
            throw new FileNotFoundException($"File not found: {testAssembly}");

        string testRuntime = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(testAssembly));
        string agentRuntime = testRuntime;

        if (agentRuntime.EndsWith("-windows"))
            agentRuntime = agentRuntime.Substring(0, 6);

        // Avoid builds we don't have
        if (agentRuntime == "net35")
            agentRuntime = "net20";
        else if (agentRuntime == "net5.0")
            agentRuntime = "net6.0";

        var executablePath = BuildSettings.OutputDirectory + $"{agentRuntime}/DirectTestAgent.exe";

        if (!System.IO.File.Exists(executablePath))
            throw new FileNotFoundException($"File not found: {executablePath}");

        Console.WriteLine($"Trying to run {executablePath} with arguments {arguments}");

        return BuildSettings.Context.StartProcess(executablePath, new ProcessSettings()
        {
            Arguments = arguments,
            WorkingDirectory = BuildSettings.OutputDirectory
        });
    }
}

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run()
