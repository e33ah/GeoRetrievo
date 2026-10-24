namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    private void BuildDockerImage(
        AbsolutePath dockerContext,
        string image,
        IEnumerable<string> buildArguments = null) {
        var arguments = new List<string> {
            "build",
        };
        if (buildArguments is not null) {
            arguments.AddRange(buildArguments);
        }

        arguments.Add("--tag");
        arguments.Add(image);
        arguments.Add("--file");
        arguments.Add((dockerContext / "Dockerfile").ToString());
        arguments.Add(dockerContext.ToString());

        Log.Information("Building Docker image {Image} from {DockerContext}", image, dockerContext);
        RunCommand("docker", arguments, RootDirectory);
    }

    private void PushDockerImage(string localImage, string image) {
        string registryHost = $"{GoogleCloudRegion}-docker.pkg.dev";

        Log.Information("Configuring Docker authentication for {Registry}", registryHost);
        RunCommand("gcloud", ["auth", "configure-docker", registryHost, "--quiet"], RootDirectory);

        Log.Information("Tagging local image {LocalImage} as {RemoteImage}", localImage, image);
        DockerTasks.DockerTag(settings => settings
            .SetSourceImage(localImage)
            .SetTargetImage(image));

        Log.Information("Pushing container image {Image}", image);
        DockerTasks.DockerPush(settings => settings.SetName(image));
    }
}
