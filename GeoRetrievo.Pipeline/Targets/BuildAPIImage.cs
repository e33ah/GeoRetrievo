namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target BuildAPIImage => _ => _
         .DependsOn(ExecuteUnitTests)
        .Executes(() => {
            if (IsLocalBuild && !IsDockerEngineAvailable()) {
                Log.Information("Skipping API image build because the Docker engine is not available");
                return;
            }

            Log.Information("Building API Docker image {Image} from {DockerContext}", LocalApiImage, ApiDockerContext);
            DockerTasks.DockerBuild(settings => settings
                .SetFile((ApiDockerContext / "Dockerfile").ToString())
                .SetTag(LocalApiImage)
                .SetPath(ApiDockerContext.ToString()));
            Log.Information("Built API Docker image {Image}", LocalApiImage);
        });

    private static bool IsDockerEngineAvailable() {
        var startInfo = new ProcessStartInfo("docker") {
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        startInfo.ArgumentList.Add("info");
        startInfo.ArgumentList.Add("--format");
        startInfo.ArgumentList.Add("{{.ServerVersion}}");

        Process process;
        try {
            process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Could not start 'docker info'.");
        }
        catch (Win32Exception exception) {
            Log.Debug("Docker CLI could not be started: {Error}", exception.Message);
            return false;
        }

        using (process) {
            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0) {
                Log.Debug("Docker engine availability check failed: {Error}", standardError.GetAwaiter().GetResult().Trim());
                return false;
            }

            return !string.IsNullOrWhiteSpace(standardOutput.GetAwaiter().GetResult());
        }
    }
}
