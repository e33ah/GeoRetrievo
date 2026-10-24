namespace GeoRetrievo.Pipeline;

[GitHubActions("georetrievo-ci", GitHubActionsImage.WindowsLatest, On = new[] { GitHubActionsTrigger.Push, GitHubActionsTrigger.PullRequest, GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(BuildSolution), nameof(ExecuteUnitTests) }, AutoGenerate = false)]
[GitHubActions("georetrievo-cd", GitHubActionsImage.UbuntuLatest, On = new[] { GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(ReleaseAPI) }, AutoGenerate = false)]
public partial class Build : FalloutBuild {
    public static int Main() => Execute<Build>(x => x.GeoRetrievoCI);
    public Target GeoRetrievoCI => _ => _
        .DependsOn(ExecuteUnitTests, ExecuteUnitTestsUi, BuildAPIImage, PlanRelease);
    public Target GeoRetrievoCD => _ => _
        .DependsOn(ReleaseAPI);

    [Solution]
    readonly Solution Solution;

    readonly AbsolutePath ApiDockerContext = RootDirectory / "GeoRetrievo.Api";
    readonly AbsolutePath TestProjectFile = RootDirectory / "GeoRetrievo.Test" / "GeoRetrievo.Test.csproj";
    readonly AbsolutePath TerraformDirectory = RootDirectory / "_Dependencies" / "infrastructure" / "terraform";
    readonly AbsolutePath UIDir = RootDirectory / "GeoRetrievo.UI";


}
