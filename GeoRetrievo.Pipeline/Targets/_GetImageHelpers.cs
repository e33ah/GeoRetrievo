namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    private string GetApiImageUri() {
        return GetImageUri(ImageNameApi, nameof(ImageNameApi));
    }

    private string GetUiImageUri() {
        return GetImageUri(ImageNameUI, nameof(ImageNameUI));
    }

    private string GetImageUri(string imageName, string imageSettingName) {
        ValidateImageSettings(imageName, imageSettingName);
        string tag = ImageTag;
        if (string.IsNullOrWhiteSpace(tag)) {
            tag = GetCurrentCommitTag();
        }

        return $"{GoogleCloudRegion}-docker.pkg.dev/{GoogleCloudProjectId}/{ArtifactRegistryRepository}/{imageName}:{tag}";
    }

    private void ValidateImageSettings(string imageName, string imageSettingName) {
        RequireSetting(nameof(GoogleCloudProjectId), GoogleCloudProjectId);
        RequireSetting(nameof(GoogleCloudRegion), GoogleCloudRegion);
        RequireSetting(nameof(ArtifactRegistryRepository), ArtifactRegistryRepository);
        RequireSetting(imageSettingName, imageName);
    }

    private static void RequireSetting(string name, string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            throw new InvalidOperationException(
                $"{name} is required. Set it with the corresponding Fallout parameter or environment variable.");
        }
    }

    private string GetCurrentUiImage() {
        return GetCurrentImage("google_cloud_run_v2_service.ui[0]", "UI");
    }

    private string GetCurrentApiImage() {
        string image = GetCurrentImage("google_cloud_run_v2_service.api[0]", "API");
        if (string.IsNullOrWhiteSpace(image)) {
            throw new InvalidOperationException(
                "Could not read the existing API image from Terraform state. Deploy the API before releasing the UI.");
        }

        return image;
    }

    private string GetCurrentImage(string resourceAddress, string serviceName) {
        var startInfo = new ProcessStartInfo("terraform") {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = RootDirectory.ToString(),
        };
        startInfo.ArgumentList.Add($"-chdir={TerraformDirectory}");
        startInfo.ArgumentList.Add("show");
        startInfo.ArgumentList.Add("-json");

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start 'terraform show'.");
        string state = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd().Trim();
        process.WaitForExit();
        if (process.ExitCode != 0) {
            throw new InvalidOperationException($"Could not inspect Terraform state. {error}".Trim());
        }

        using var document = JsonDocument.Parse(state);
        if (!document.RootElement.TryGetProperty("values", out var values)
            || !values.TryGetProperty("root_module", out var rootModule)) {
            return string.Empty;
        }

        return FindImage(rootModule, resourceAddress, serviceName);
    }

    private static string FindImage(JsonElement module, string resourceAddress, string serviceName) {
        if (module.TryGetProperty("resources", out var resources)) {
            foreach (var resource in resources.EnumerateArray()) {
                if (!resource.TryGetProperty("address", out var address)
                    || address.GetString() != resourceAddress) {
                    continue;
                }

                if (!resource.TryGetProperty("values", out var values)
                    || !values.TryGetProperty("template", out var template)) {
                    throw new InvalidOperationException($"Could not read the existing {serviceName} image from Terraform state.");
                }

                template = GetSingleValue(template);
                if (!template.TryGetProperty("containers", out var containers)
                    || containers.GetArrayLength() == 0
                    || !containers[0].TryGetProperty("image", out var imageValue)
                    || string.IsNullOrWhiteSpace(imageValue.GetString())) {
                    throw new InvalidOperationException($"Could not read the existing {serviceName} image from Terraform state.");
                }

                return imageValue.GetString();
            }
        }
        if (module.TryGetProperty("child_modules", out var childModules)) {
            foreach (var childModule in childModules.EnumerateArray()) {
                string image = FindImage(childModule, resourceAddress, serviceName);
                if (!string.IsNullOrEmpty(image)) {
                    return image;
                }
            }
        }
        return string.Empty;
    }

    private static JsonElement GetSingleValue(JsonElement value) =>
        value.ValueKind == JsonValueKind.Array && value.GetArrayLength() > 0 ? value[0] : value;

}
