namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target ReleaseUI => _ => _
        .DependsOn(PushUIImage)
        .Executes(() => {
            ValidateTerraformSettings();
            InitializeTerraform();
            string apiImage = GetCurrentApiImage();
            string uiImage = GetUiImageUri();

            Log.Information("Deploying UI image {Image} to project {ProjectId} in {Region}", uiImage, GoogleCloudProjectId, GoogleCloudRegion);
            ApplyTerraform(apiImage, uiImage);

            string uiUrl = GetTerraformOutput("ui_url");
            Log.Information("UI release completed: {UiUrl}", uiUrl);
        });
}
