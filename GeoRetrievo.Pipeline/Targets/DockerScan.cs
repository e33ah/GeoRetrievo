namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target DockerScan => _ => _
        .DependsOn(BuildAPIImage, BuildUIImage)
        .Executes(() => {
            ScanDockerImage(LocalApiImage);
            ScanDockerImage(LocalUiImage);
        });

    private void ScanDockerImage(string image) {
        Log.Information("Scanning Docker image {Image} with Trivy", image);
        RunTrivy(["image", "--exit-code", "1", "--severity", "HIGH,CRITICAL", image]);
    }
}
