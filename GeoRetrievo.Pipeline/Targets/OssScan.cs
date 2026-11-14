namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target OssScan => _ => _
        .Executes(() => {
            ScanSourceCode();
        });

    private void ScanSourceCode() {
        Log.Information("Scanning source code with Trivy");
        string[] args = ["fs", "--skip-dirs", ".vs", "--exit-code", "1", "--severity", "HIGH,CRITICAL", RootDirectory.ToString()];
        RunTrivy(args);
    }
}
