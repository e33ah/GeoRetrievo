namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    public Target ProvisionAPIInfrastructure => _ => _
        .Executes(() => {
            // Target to provision the API infrastructure using Terraform.
            // This target will validate the Terraform settings, initialize Terraform, and apply the Terraform configuration to provision the necessary resources in Google Cloud.
            // Only run this target to provision the infrastructure for the first time or make changes to the existing infrastructure. Not included as part of the pipelines.

            ValidateTerraformSettings();

            Log.Information("Provisioning API infrastructure for project {ProjectId} in {Region}", GoogleCloudProjectId, GoogleCloudRegion);
            InitializeTerraform();
            ApplyTerraform();

            Log.Information("API infrastructure provisioning completed");
        });
}
