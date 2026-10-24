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
  -backend-config="bucket=YOUR_UNIQUE_TERRAFORM_STATE_BUCKET" `
  -backend-config="prefix=georetrievo/terraform/state"
Copy-Item terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars` with the project ID and region. If the runtime service
account and its bucket access were created manually as shown in
`_Dependencies/scripts/gcloud.txt`, import the existing bucket IAM grant so
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
$image = "REGION-docker.pkg.dev/PROJECT_ID/georetrievo/api:INITIAL_TAG"
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
