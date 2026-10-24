namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target PushUIImage => _ => _
        .DependsOn(BuildUIImage)
        .Executes(() => {
            string image = GetUiImageUri();
            PushDockerImage(LocalUiImage, image);
            Log.Information("Pushed UI image {Image}", image);
        });
}
