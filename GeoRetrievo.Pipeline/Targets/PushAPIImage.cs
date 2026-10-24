namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target PushAPIImage => _ => _
        .DependsOn(BuildAPIImage)
        .Executes(() => {
            string image = GetApiImageUri();
            PushDockerImage(LocalApiImage, image);
            Log.Information("Pushed API image {Image}", image);
        });
}
