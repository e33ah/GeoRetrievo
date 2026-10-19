using Fallout.Common.CI.GitHubActions;

namespace GeoRetrievo.Pipeline;

[GitHubActions("georetrievo-ci", GitHubActionsImage.WindowsLatest, On = new[] { GitHubActionsTrigger.Push, GitHubActionsTrigger.PullRequest, GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(BuildSolution), nameof(ExecuteUnitTests) }, AutoGenerate = false)]
public partial class Build : FalloutBuild {
    public static int Main() => Execute<Build>(x => x.GeoRetrievoCI);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution]
    readonly Solution Solution;

    readonly AbsolutePath UIDir = RootDirectory / "GeoRetrievo.UI";
    readonly AbsolutePath testProjectFile = RootDirectory / "GeoRetrievo.Test" / "GeoRetrievo.Test.csproj";


    public Target GeoRetrievoCI => _ => _
        .DependsOn(ExecuteUnitTests, ExecuteUnitTestsUi);
}
