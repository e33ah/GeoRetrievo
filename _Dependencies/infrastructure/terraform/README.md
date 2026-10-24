# GeoRetrievo API infrastructure

This Terraform configuration provisions the API's Artifact Registry repository,
read-only access to the existing country-data bucket, and (once an image is
supplied) the Cloud Run service. It references the existing Cloud Run runtime
service account; it does not create or manage that account.

Cloud Run is configured to scale to zero, use request-based CPU allocation,
allow at most one request at a time per instance, use the first-generation
execution environment, and allocate 0.25 vCPU with 256 MiB of memory. This is
a low-cost starting point for a small, low-traffic API, not a guarantee of
zero charges. Cloud Build, Artifact Registry storage, Cloud Logging, and other
Google Cloud usage may incur charges, and free-tier eligibility depends on
region and monthly usage. The one-instance cap and serial request handling
limit throughput and availability during traffic spikes.

## Prerequisites

- Terraform 1.6 or later and the Google Cloud CLI.
- A Google Cloud project with billing enabled and permission to enable APIs,
  create service accounts and Cloud Run/Artifact Registry resources, and update
  the country-data bucket IAM policy.
- The existing bucket and catalog object, or update the variables to their
  names.
- The existing runtime service account, defaulting to
  `georetrievo-api-svc`. Set `runtime_service_account_id` if it has another name.
- A private GCS bucket for Terraform state. Create it once before Terraform
  initialization; use a globally unique bucket name.

```powershell
gcloud auth login
gcloud auth application-default login
gcloud config set project YOUR_PROJECT_ID
gcloud storage buckets create gs://YOUR_UNIQUE_TERRAFORM_STATE_BUCKET `
  --location=US-CENTRAL1 `
  --uniform-bucket-level-access `
  --public-access-prevention
gcloud storage buckets update gs://YOUR_UNIQUE_TERRAFORM_STATE_BUCKET --versioning
```

## Initial infrastructure and deployment

From this directory, initialize Terraform with the state bucket and copy the
example variables file:

```powershell
terraform init `
  -backend-config="bucket=YOUR_UNIQUE_TERRAFORM_STATE_BUCKET"
Copy-Item terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars` with the project ID and region. If the runtime service
account and its bucket access were created manually as shown in
`..\..\scripts\gcloud.txt`, import the existing bucket IAM grant so
Terraform tracks it instead of trying to add an already-existing grant:

```powershell
terraform import google_storage_bucket_iam_member.country_data_reader `
  "country_data_01 roles/storage.objectViewer serviceAccount:georetrievo-api-svc@georetrievo.iam.gserviceaccount.com"
```

The first apply creates the required APIs and Artifact Registry repository. It
does not create Cloud Run until `container_image` is set:

```powershell
terraform plan
terraform apply
```

Build and push the API image from the repository root (replace project, region,
and tag as appropriate):

```powershell
$image = "REGION-docker.pkg.dev/PROJECT_ID/georetrievo/georetrievo-api:INITIAL_TAG"
gcloud builds submit --tag $image .\GeoRetrievo.Api
```

Set `container_image` in `terraform.tfvars` to that full image URI, then deploy:

```powershell
terraform plan
terraform apply
terraform output -raw api_url
```

For direct access from a browser-based UI, set `allow_unauthenticated = true`
only if the API is intended to be public. For local UI development, retain
`http://localhost:5173` in `cors_allowed_origins`. When the UI is hosted, replace
it with the UI's exact HTTPS origin (scheme and host, with no path); remove the
localhost origin if it is no longer needed.

Cloud Run uses the existing runtime service account. Terraform grants it
`roles/storage.objectViewer` on the country-data bucket only. Do not provide
local ADC credential files or service-account keys to the deployed service.

Artifact Registry automatically deletes untagged image versions older than
14 days. Tagged images are retained, so this policy will not remove tagged
release versions.

## Updating the API

Build and push a new, preferably immutable image tag, update `container_image`
in `terraform.tfvars`, and run `terraform apply` to deploy a new Cloud Run
revision. Do not commit `terraform.tfvars`, plan files, credentials, or state.
The GCS backend stores state remotely; the backend bucket must be protected and
its versioning kept enabled.

## Deploying with Fallout

The repository pipeline has `ProvisionAPIInfrastructure`, `BuildAPIImage`,
`PushAPIImage`, `PlanRelease`, and `ReleaseAPI` targets. `PlanRelease`
initializes Terraform with the configured remote state, checks formatting,
validates the configuration, and plans a release using the commit-tagged
Artifact Registry image. It does not apply changes or save a plan file. The CI
workflow authenticates and runs this plan only on pushes to `main` and manual
runs against `main`; pull requests skip the cloud-authenticated plan.
`ReleaseAPI` runs the required targets in order,
builds the Docker image from `GeoRetrievo.Api\Dockerfile`, pushes a
commit-based image tag to Artifact Registry, applies Terraform with that image,
and prints the resulting API URL. `BuildAPIImage` builds locally as
`georetrievo-api:local`, so it can run without Google Cloud settings or
authentication when Docker Desktop is running. Start that image locally with:

```powershell
docker run --rm -p 8080:8080 georetrievo-api:local
```

To use a different local tag, set the `LocalApiImage` Fallout parameter.
`PushAPIImage` re-tags the local image for Artifact Registry before pushing it.
Terraform configuration is still read from this directory, including options
such as `allow_unauthenticated` and `cors_allowed_origins`.

The machine or CI runner needs Docker, the Google Cloud CLI, and Terraform
installed, and must already be authenticated to Google Cloud with permissions
to provision the documented resources, push to Artifact Registry, and deploy
Cloud Run. Provision the infrastructure once before the first image push:
`.\build.ps1 ProvisionAPIInfrastructure`. After that, `ReleaseAPI` builds and
pushes the image and runs one Terraform apply to deploy it. For GitHub Actions,
the `georetrievo-cd` workflow authenticates with Workload Identity Federation
and installs Terraform and the Google Cloud CLI. It uses the Workload Identity
Provider
`projects/194067282433/locations/global/workloadIdentityPools/github-pool-01/providers/github-provider`
and service account `github-deploy-svc@georetrievo.iam.gserviceaccount.com`;
these identifiers are hard-coded in the workflow, not secrets. Configure the
provider to trust this repository and the intended branch/environment. Grant
the service account permission to provision the documented resources, push to
Artifact Registry, and deploy Cloud Run. Do not store a service-account key in
repository secrets.

The workflow sets the Google Cloud CLI project to `georetrievo`. The Fallout
pipeline defaults to project `georetrievo` and region `us-central1`, with
optional overrides through `GOOGLE_CLOUD_PROJECT` / `GCP_PROJECT_ID` and
`GOOGLE_CLOUD_REGION` / `GCP_REGION`. Create the
`georetrievo-terraform-state` bucket before the first run, and import any
existing country-data bucket IAM grant as described above before the first
apply.

The Terraform state bucket defaults to `georetrievo-terraform-state`; set
`TERRAFORM_STATE_BUCKET` only if using a different bucket. Run the target from
the repository root:

```powershell
.\build.ps1 PlanRelease
```

The image tag defaults to `GITHUB_SHA` in GitHub Actions and to the current
Git commit locally. Run `.\build.ps1 ReleaseAPI` to build, push, and deploy.
The separate targets can also be run individually:
`ProvisionAPIInfrastructure` prepares the Terraform-managed resources,
`BuildAPIImage` builds the image, and `PushAPIImage` pushes it.
