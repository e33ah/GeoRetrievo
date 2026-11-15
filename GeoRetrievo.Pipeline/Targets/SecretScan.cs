namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target SecretScan => _ => _
        .Executes(() => {
            Log.Information("Scanning repository files for secrets with TruffleHog");
            string[] args = [
                "filesystem",
                ".",
                "--exclude-paths",
                ".trufflehog-exclude.txt",
                "--no-update",
                "--log-level=5",
            ];
            RunTruffleHog(args);
        });
}
