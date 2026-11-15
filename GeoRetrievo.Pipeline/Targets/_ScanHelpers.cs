namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    private void RunTrivy(IEnumerable<string> arguments) {
        try {
            RunCommand("trivy", arguments, RootDirectory);
        } catch (Win32Exception exception) {
            throw new InvalidOperationException(
                "Could not start Trivy. Install the Trivy CLI and ensure 'trivy' is available on PATH.",
                exception);
        }
    }
    private void RunTruffleHog(IEnumerable<string> arguments) {
        try {
            RunCommand("trufflehog", arguments, RootDirectory);
        } catch (Win32Exception exception) {
            throw new InvalidOperationException(
                "Could not start TruffleHog. Install the TruffleHog CLI and ensure 'trufflehog' is available on PATH.",
                exception);
        }
    }
    private static string GetSecretScanBaseCommitFromEnvironment() {
        string baseCommit = Environment.GetEnvironmentVariable("TRUFFLEHOG_BASE");
        const string PREVIOUS_COMMIT_REF = "HEAD~1";
        return string.IsNullOrWhiteSpace(baseCommit) || baseCommit.All(character => character == '0') ? PREVIOUS_COMMIT_REF : baseCommit;
    }
}
