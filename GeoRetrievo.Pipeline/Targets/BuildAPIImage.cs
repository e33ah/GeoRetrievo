namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target BuildAPIImage => _ => _
        .DependsOn(ExecuteUnitTests)
        .Executes(() => {
            Log.Information("Building API Docker image {Image} from {DockerContext}", LocalApiImage, ApiDockerContext);
            DockerTasks.DockerBuild(settings => settings
                .SetFile((ApiDockerContext / "Dockerfile").ToString())
                .SetTag(LocalApiImage)
                .SetPath(ApiDockerContext.ToString()));
            Log.Information("Built API Docker image {Image}", LocalApiImage);
        });
}
