namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target BuildUIImage => _ => _
        .DependsOn(ExecuteUnitTestsUi)
        .Executes(() => {
            if (IsLocalBuild && !IsDockerEngineAvailable()) {
                Log.Information("Skipping UI image build because the Docker engine is not available");
                return;
            }

            IEnumerable<string> buildArguments = [];
            if (BuildForRelease) {
                ValidateTerraformSettings();
                InitializeTerraform();
                string apiUrl = GetTerraformOutput("api_url");
                buildArguments = ["--build-arg", $"VITE_API_BASE_URL={apiUrl}"];
            }

            BuildDockerImage(
                UIDockerContext,
                LocalUiImage,
                buildArguments);
            Log.Information("Built UI Docker image {Image}", LocalUiImage);
        });
}
