using Fallout.Common.CI.GitHubActions;

namespace GeoRetrievo.Pipeline;

[GitHubActions("georetrievo-ci", GitHubActionsImage.WindowsLatest, On = new[] { GitHubActionsTrigger.Push, GitHubActionsTrigger.PullRequest, GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(BuildSolution), nameof(ExecuteUnitTests) }, AutoGenerate = false)]
[GitHubActions("georetrievo-cd", GitHubActionsImage.WindowsLatest, On = new[] { GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(ReleaseAPI) }, AutoGenerate = false)]
public partial class Build : FalloutBuild {
    public static int Main() => Execute<Build>(x => x.GeoRetrievoCI);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution]
    readonly Solution Solution;

    [Parameter("Google Cloud project ID")]
    readonly string GoogleCloudProjectId =
        Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT")
        ?? Environment.GetEnvironmentVariable("GCP_PROJECT_ID")
        ?? "georetrievo";

    [Parameter("Google Cloud region")]
    readonly string GoogleCloudRegion =
        Environment.GetEnvironmentVariable("GOOGLE_CLOUD_REGION")
        ?? Environment.GetEnvironmentVariable("GCP_REGION")
        ?? "us-central1";

    [Parameter("Artifact Registry repository ID")]
    readonly string ArtifactRegistryRepository = "georetrievo";

    [Parameter("Docker image name")]
    readonly string ApiImageName = "georetrievo-api";

    [Parameter("Local Docker image name and tag")]
    readonly string LocalApiImage = "georetrievo-api:local";

    [Parameter("Container image tag; defaults to GITHUB_SHA or the current Git commit")]
    readonly string ImageTag = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? string.Empty;

    [Parameter("GCS bucket used for Terraform remote state")]
    readonly string TerraformStateBucket =
        Environment.GetEnvironmentVariable("TERRAFORM_STATE_BUCKET") ?? "georetrievo-terraform-state";

    readonly AbsolutePath UIDir = RootDirectory / "GeoRetrievo.UI";
    readonly AbsolutePath testProjectFile = RootDirectory / "GeoRetrievo.Test" / "GeoRetrievo.Test.csproj";
    readonly AbsolutePath ApiDockerContext = RootDirectory / "GeoRetrievo.Api";
    readonly AbsolutePath TerraformDirectory = RootDirectory / "_Dependencies" / "infrastructure" / "terraform";


    public Target GeoRetrievoCI => _ => _
        .DependsOn(ExecuteUnitTests, ExecuteUnitTestsUi, BuildAPIImage, PlanRelease);
    public Target GeoRetrievoCD => _ => _
        .DependsOn(ReleaseAPI);
}
