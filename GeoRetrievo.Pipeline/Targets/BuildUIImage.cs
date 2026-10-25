namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target BuildUIImage => _ => _
        .DependsOn(ExecuteUnitTestsUi, ExecuteUnitTests)
        .Executes(() => {
            if (IsLocalBuild && !IsDockerEngineAvailable()) {
                Log.Information("Skipping UI image build because the Docker engine is not available");
                return;
            }

            BuildDockerImage(
                UIDockerContext,
                LocalUiImage,
                dockerfile: (RootDirectory / "GeoRetrievo.Proxy" / "Dockerfile").ToString());
            Log.Information("Built UI Docker image {Image}", LocalUiImage);
        });
}
