namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target PlanRelease => _ => _
        .Executes(() => {
            ValidateTerraformSettings();

            Log.Information("Planning release for project {ProjectId} in {Region} using Terraform state bucket {StateBucket}", GoogleCloudProjectId, GoogleCloudRegion, TerraformStateBucket);
            InitializeTerraform();

            Log.Information("Checking Terraform formatting");
            RunTerraform(["fmt", "-check", "-recursive"]);

            Log.Information("Validating Terraform configuration");
            RunTerraform(["validate"]);

            Log.Information("Creating release plan for image {Image}", GetApiImageUri());
            RunTerraform([
                "plan",
                "-input=false",
                $"-var=project_id={GoogleCloudProjectId}",
                $"-var=region={GoogleCloudRegion}",
                $"-var=container_image={GetApiImageUri()}",
            ]);

            Log.Information("Terraform release plan completed; no changes were applied");
        });
}
