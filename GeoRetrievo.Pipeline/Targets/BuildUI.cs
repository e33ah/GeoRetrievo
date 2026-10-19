namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target BuildUI => _ => _
        .DependsOn(CleanInstall)
        .Executes(() => {
            NpmTasks.NpmRun(s => s
                .SetCommand("lint")
                .SetProcessWorkingDirectory(UIDir)
            );

            NpmTasks.NpmRun(s => s
                .SetCommand("build")
                .SetProcessWorkingDirectory(UIDir)
            );
        });

    public Target CleanInstall => _ => _
        .OnlyWhenStatic(() => IsServerBuild)
        .Executes(() => {
            NpmTasks.NpmCi(s => s
                .SetProcessWorkingDirectory(UIDir)
            );
        });
}