namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target SecretScan => _ => _
        .Executes(() => {
            Log.Information("Scanning Git changes since {BaseCommit} for secrets with TruffleHog", SecretScanBaseCommit);
            string[] args = [
                "git",
                "file://.",
                "--since-commit",
                SecretScanBaseCommit,
                "--branch",
                "HEAD",
                "--exclude-paths",
                ".trufflehog-exclude.txt",
                "--no-update",
                "--fail",
            ];
            RunTruffleHog(args);
        });
}
