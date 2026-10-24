namespace GeoRetrievo.Pipeline;

[GitHubActions("georetrievo-ci", GitHubActionsImage.WindowsLatest, On = new[] { GitHubActionsTrigger.Push, GitHubActionsTrigger.PullRequest, GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(BuildSolution), nameof(ExecuteUnitTests) }, AutoGenerate = false)]
[GitHubActions("georetrievo-api-cd", GitHubActionsImage.UbuntuLatest, On = new[] { GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(ReleaseAPI) }, AutoGenerate = false)]
[GitHubActions("georetrievo-ui-cd", GitHubActionsImage.UbuntuLatest, On = new[] { GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(ReleaseUI) }, AutoGenerate = false)]
public partial class Build : FalloutBuild {
    public static int Main() => Execute<Build>(x => x.GeoRetrievoCI);
    public Target GeoRetrievoCI => _ => _
        .DependsOn(ExecuteUnitTests, ExecuteUnitTestsUi, BuildAPIImage, BuildUIImage, PlanRelease);
    public Target GeoRetrievoCD => _ => _
        .DependsOn(ReleaseAPI, ReleaseUI);

    [Solution]
    readonly Solution Solution;

    readonly AbsolutePath ApiDockerContext = RootDirectory / "GeoRetrievo.Api";
    readonly AbsolutePath UIDockerContext = RootDirectory / "GeoRetrievo.UI";
    readonly AbsolutePath TestProjectFile = RootDirectory / "GeoRetrievo.Test" / "GeoRetrievo.Test.csproj";
    readonly AbsolutePath TerraformDirectory = RootDirectory / "_Dependencies" / "infrastructure" / "terraform";
    readonly AbsolutePath UIDir = RootDirectory / "GeoRetrievo.UI";
}
