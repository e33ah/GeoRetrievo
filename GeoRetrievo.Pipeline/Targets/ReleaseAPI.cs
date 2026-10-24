namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target ReleaseAPI => _ => _
        .DependsOn(PushAPIImage)
        .Executes(() => {
            ValidateTerraformSettings();
            string image = GetApiImageUri();

            Log.Information("Deploying API image {Image} to project {ProjectId} in {Region}", image, GoogleCloudProjectId, GoogleCloudRegion);
            ApplyTerraform(image);

            Log.Information("Fetching deployed API URL");
            RunTerraform(["output", "-raw", "api_url"]);

            Log.Information("API release completed");
        });
}
