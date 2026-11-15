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
            // Findings may contain secret values, so only forward TruffleHog's diagnostic stderr.
            RunCommand("trufflehog", arguments, RootDirectory, captureOutput: true, logStandardOutput: false);
        } catch (Win32Exception exception) {
            throw new InvalidOperationException("Could not start TruffleHog. Install the TruffleHog CLI and ensure 'trufflehog' is available on PATH.", exception);
        }
    }
}
