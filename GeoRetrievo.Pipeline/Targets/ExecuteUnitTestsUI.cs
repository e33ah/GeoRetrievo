namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target ExecuteUnitTestsUi => _ => _
        .DependsOn(BuildUI)
        .Executes(() => {
            Log.Information("Executing unit tests for GeoRetrievo.UI project...");
            NpmTasks.NpmRun(s => s
                .SetCommand("test:run")
                .SetProcessWorkingDirectory(UIDir)
            );
        });
}
