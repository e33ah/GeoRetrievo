namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {

    [Parameter("Docker image name")]
    readonly string ApiImageName = "georetrievo-api";

    [Parameter("Artifact Registry repository ID")]
    readonly string ArtifactRegistryRepository = "georetrievo";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

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

    [Parameter("Container image tag; defaults to GITHUB_SHA or the current Git commit")]
    readonly string ImageTag = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? string.Empty;

    [Parameter("Local Docker image name and tag")]
    readonly string LocalApiImage = "georetrievo-api:local";

    [Parameter("GCS bucket used for Terraform remote state")]
    readonly string TerraformStateBucket =
        Environment.GetEnvironmentVariable("TERRAFORM_STATE_BUCKET") ?? "georetrievo-terraform-state";
}
