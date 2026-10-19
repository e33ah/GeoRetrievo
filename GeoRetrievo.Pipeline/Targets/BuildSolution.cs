namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target BuildSolution => _ => _
        .DependsOn(CleanSolution)
        .Executes(() => {
            DotNetTasks.DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetDeterministic(IsServerBuild)
                .SetContinuousIntegrationBuild(IsServerBuild)
            );
        });

    public Target CleanSolution => _ => _
        .OnlyWhenStatic(() => IsLocalBuild)
        .Executes(() => {
            DotNetTasks.DotNetClean(s => s
                .SetConfiguration(Configuration)
                .SetVerbosity(DotNetVerbosity.minimal)
            );
        });
}