namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target PushAPIImage => _ => _
        .DependsOn(BuildAPIImage)
        .Executes(() => {
            string image = GetApiImageUri();
            string registryHost = $"{GoogleCloudRegion}-docker.pkg.dev";

            Log.Information("Configuring Docker authentication for {Registry}", registryHost);
            RunCommand("gcloud", ["auth", "configure-docker", registryHost, "--quiet"], RootDirectory);

            Log.Information("Tagging local image {LocalImage} as {RemoteImage}", LocalApiImage, image);
            DockerTasks.DockerTag(settings => settings
                .SetSourceImage(LocalApiImage)
                .SetTargetImage(image));

            Log.Information("Pushing API image {Image}", image);
            DockerTasks.DockerPush(settings => settings
                .SetName(image));

            Log.Information("Pushed API image {Image}", image);
        });
}
