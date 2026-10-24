namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target ExecuteUnitTests => _ => _
        .DependsOn(BuildSolution)
        .Executes(() => {
            Log.Information("Executing unit tests for GeoRetrievo solution...");
            DotNetTasks.DotNetRun(s => s
                .SetProjectFile(TestProjectFile)
                .SetConfiguration(Configuration)
                .SetNoBuild(true)
            );
        });
}
