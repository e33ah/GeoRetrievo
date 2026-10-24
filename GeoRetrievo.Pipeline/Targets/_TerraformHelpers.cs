namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    private void ValidateTerraformSettings() {
        ValidateImageSettings(ImageNameApi, nameof(ImageNameApi));
        RequireSetting(nameof(TerraformStateBucket), TerraformStateBucket);
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

    private void ApplyTerraform(
        string apiContainerImage = null,
        string uiContainerImage = null,
        bool? allowUnauthenticated = null) {
        var arguments = new List<string> {
            "apply",
            "-auto-approve",
            "-input=false",
            $"-var=project_id={GoogleCloudProjectId}",
            $"-var=region={GoogleCloudRegion}",
        };

        if (apiContainerImage is not null) {
            arguments.Add($"-var=container_image={apiContainerImage}");
        }

        if (uiContainerImage is not null) {
            arguments.Add($"-var=ui_container_image={uiContainerImage}");
        }

        if (allowUnauthenticated.HasValue) {
            arguments.Add($"-var=allow_unauthenticated={allowUnauthenticated.Value.ToString().ToLowerInvariant()}");
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

    private string GetTerraformOutput(string outputName) {
        var startInfo = new ProcessStartInfo("terraform") {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = RootDirectory.ToString(),
        };
        startInfo.ArgumentList.Add($"-chdir={TerraformDirectory}");
        startInfo.ArgumentList.Add("output");
        startInfo.ArgumentList.Add("-raw");
        startInfo.ArgumentList.Add(outputName);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start 'terraform output'.");
        string output = process.StandardOutput.ReadToEnd().Trim();
        string error = process.StandardError.ReadToEnd().Trim();
        process.WaitForExit();
        if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output)) {
            throw new InvalidOperationException(
                $"Could not read Terraform output '{outputName}'. {error}".Trim());
        }

        return output;
    }
}
