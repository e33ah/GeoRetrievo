namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target ReleaseAPI => _ => _
        .DependsOn(PushAPIImage)
        .Executes(() => {
            ValidateTerraformSettings();
            InitializeTerraform();
            string image = GetApiImageUri();
            string currentUiImage = GetCurrentUiImage();

            Log.Information("Deploying API image {Image} to project {ProjectId} in {Region}", image, GoogleCloudProjectId, GoogleCloudRegion);
            ApplyTerraform(image, currentUiImage);

            Log.Information("Fetching deployed API URL");
            RunTerraform(["output", "-raw", "api_url"]);

            Log.Information("API release completed");
        });
}
