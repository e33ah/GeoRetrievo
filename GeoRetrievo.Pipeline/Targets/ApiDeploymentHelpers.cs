namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    private string GetApiImageUri() {
        ValidateImageSettings();

        string tag = ImageTag;
        if (string.IsNullOrWhiteSpace(tag)) {
            tag = GetCurrentCommitTag();
        }

        return $"{GoogleCloudRegion}-docker.pkg.dev/{GoogleCloudProjectId}/{ArtifactRegistryRepository}/{ApiImageName}:{tag}";
    }

    private void ValidateImageSettings() {
        RequireSetting(nameof(GoogleCloudProjectId), GoogleCloudProjectId);
        RequireSetting(nameof(GoogleCloudRegion), GoogleCloudRegion);
        RequireSetting(nameof(ArtifactRegistryRepository), ArtifactRegistryRepository);
        RequireSetting(nameof(ApiImageName), ApiImageName);
    }

    private void ValidateTerraformSettings() {
        ValidateImageSettings();
        RequireSetting(nameof(TerraformStateBucket), TerraformStateBucket);
    }

    private static void RequireSetting(string name, string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            throw new InvalidOperationException(
                $"{name} is required. Set it with the corresponding Fallout parameter or environment variable.");
        }
    }

    private void InitializeTerraform() {
        Log.Information(
            "Initializing Terraform in {TerraformDirectory} with GCS state bucket {StateBucket}",
            TerraformDirectory,
            TerraformStateBucket);
        RunTerraform([
            "init",
            "-input=false",
            $"-backend-config=bucket={TerraformStateBucket}",
        ]);
        Log.Information("Terraform initialization completed");
    }

    private void ApplyTerraform(string containerImage = null) {
        var arguments = new List<string> {
            "apply",
            "-auto-approve",
            "-input=false",
            $"-var=project_id={GoogleCloudProjectId}",
            $"-var=region={GoogleCloudRegion}",
        };

        if (containerImage is not null) {
            arguments.Add($"-var=container_image={containerImage}");
        }

        Log.Information("Applying Terraform configuration");
        RunTerraform(arguments);
        Log.Information("Terraform apply completed");
    }

    private void RunTerraform(IEnumerable<string> arguments) {
        var terraformArguments = new List<string> {
            $"-chdir={TerraformDirectory}",
        };
        terraformArguments.AddRange(arguments);
        RunCommand("terraform", terraformArguments, RootDirectory);
    }

    private static void RunCommand(
        string fileName,
        IEnumerable<string> arguments,
        AbsolutePath workingDirectory) {
        var commandArguments = arguments.ToList();
        var startInfo = OperatingSystem.IsWindows()
            && string.Equals(fileName, "gcloud", StringComparison.OrdinalIgnoreCase)
                ? CreateWindowsGCloudStartInfo(commandArguments, workingDirectory)
                : new ProcessStartInfo(fileName) {
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory.ToString(),
                };

        if (!OperatingSystem.IsWindows()
            || !string.Equals(fileName, "gcloud", StringComparison.OrdinalIgnoreCase)) {
            foreach (string argument in commandArguments) {
                startInfo.ArgumentList.Add(argument);
            }
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start '{fileName}'.");

        Log.Information("Started command {Command}", FormatCommand(fileName, commandArguments));
        process.WaitForExit();
        if (process.ExitCode != 0) {
            Log.Error(
                "Command {Command} failed with exit code {ExitCode}",
                FormatCommand(fileName, commandArguments),
                process.ExitCode);
            throw new InvalidOperationException(
                $"'{fileName}' exited with code {process.ExitCode}.");
        }

        Log.Information("Command {Command} completed successfully", FormatCommand(fileName, commandArguments));
    }

    private static ProcessStartInfo CreateWindowsGCloudStartInfo(
        IReadOnlyCollection<string> arguments,
        AbsolutePath workingDirectory) {
        if (arguments.Any(argument => argument.Any(character =>
                !char.IsAsciiLetterOrDigit(character)
                && character is not '.' and not '_' and not ':' and not '/' and not '-'))) {
            throw new InvalidOperationException(
                "Google Cloud CLI arguments contain characters that cannot safely be passed through cmd.exe.");
        }

        var startInfo = new ProcessStartInfo(
            Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe") {
            UseShellExecute = false,
            WorkingDirectory = workingDirectory.ToString(),
        };

        startInfo.ArgumentList.Add("/d");
        startInfo.ArgumentList.Add("/s");
        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add($"gcloud.cmd {string.Join(" ", arguments)}");
        return startInfo;
    }

    private static string FormatCommand(string fileName, IEnumerable<string> arguments) =>
        $"{fileName} {string.Join(" ", arguments)}";

    private string GetCurrentCommitTag() {
        var startInfo = new ProcessStartInfo("git") {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            WorkingDirectory = RootDirectory.ToString(),
        };
        startInfo.ArgumentList.Add("rev-parse");
        startInfo.ArgumentList.Add("--short=12");
        startInfo.ArgumentList.Add("HEAD");

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start 'git'.");

        string tag = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(tag)) {
            throw new InvalidOperationException("Could not determine the current Git commit for the image tag.");
        }

        return tag;
    }
}
