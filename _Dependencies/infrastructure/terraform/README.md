# API infrastructure

This Terraform configuration provisions the Google Cloud resources used by
the GeoRetrievo API: required APIs, an Artifact Registry repository, access
for the existing runtime service account to read the country-data bucket,
and (after an image is supplied) a Cloud Run service. It does not create the
runtime service account or the country-data bucket.

## Prerequisites

- Terraform 1.6 or later and the Google Cloud CLI.
- A Google Cloud project with billing enabled and permission to enable APIs,
  manage the documented resources, and update the country-data bucket IAM
  policy.
- An existing country-data bucket and runtime service account. Defaults are
  `country_data_01` and `georetrievo-api-svc`; change the corresponding
  variables if your resources have different names.
- A private, versioned Cloud Storage bucket for Terraform state. The default
  backend bucket is `georetrievo-terraform-state`; create it before running
  Terraform. Keep it protected and do not commit state or plan files.

Authenticate and set the target project:

```powershell
gcloud auth login
gcloud auth application-default login
gcloud config set project YOUR_PROJECT_ID
gcloud storage buckets create gs://georetrievo-terraform-state `
  --location=us-central1 `
  --uniform-bucket-level-access `
  --public-access-prevention
gcloud storage buckets update gs://georetrievo-terraform-state --versioning
```

## Provision and deploy

From this directory, initialize Terraform and create a local variables file:

```powershell
terraform init
Copy-Item terraform.tfvars.example terraform.tfvars
```

Set `project_id` and `region` in `terraform.tfvars`. If the runtime account's
bucket access was granted outside Terraform, import that existing IAM grant
before the first apply:

```powershell
terraform import google_storage_bucket_iam_member.country_data_reader `
  "country_data_01 roles/storage.objectViewer serviceAccount:georetrievo-api-svc@YOUR_PROJECT_ID.iam.gserviceaccount.com"
```

The initial apply enables the required APIs, grants the bucket access, and
creates the Artifact Registry repository. Cloud Run is not created until
`container_image` is set:

```powershell
terraform plan
terraform apply
```

From the repository root, build and push an API image (replace the project,
region, and tag):

```powershell
$image = "REGION-docker.pkg.dev/PROJECT_ID/georetrievo/georetrievo-api:IMAGE_TAG"
gcloud builds submit --tag $image .\GeoRetrievo.Api
```

Set `container_image` to that full image URI, then apply again:

```powershell
terraform plan
terraform apply
terraform output -raw api_url
```

Use immutable image tags for releases. To deploy a new revision, build and
push a new image, update `container_image`, and apply again.

## Access and operating costs

The API is private by default. Set `allow_unauthenticated = true` only when
public invocation is intended. Set `cors_allowed_origins` to the exact UI
origin (scheme, host, and optional port; no path or trailing slash). Remove
the localhost origin when it is no longer needed.

Cloud Run scales to zero, uses request-based CPU allocation, and allows at
most one instance with one request at a time per instance, 0.25 vCPU, and
256 MiB of memory. These are low-cost starting settings for a small API, not
a guarantee of zero charges; the instance and concurrency limits also
constrain throughput.

Artifact Registry deletes untagged images older than 14 days; tagged images
are retained. Cloud Build, storage, logging, and other Google Cloud services
may incur additional charges.

Cloud Run uses the existing runtime service account, which receives only
`roles/storage.objectViewer` on the country-data bucket. Do not deploy local
ADC files or service-account keys.

## Automated release

The `georetrievo-cd` GitHub Actions workflow is manually triggered and uses
Workload Identity Federation; it does not need a service-account key in
repository secrets. Configure its identity provider to trust this repository
and the intended branch or environment, and grant its service account the
permissions needed to provision infrastructure, push images, and deploy
Cloud Run.

The separate `PlanRelease` target formats, validates, and plans without
applying. The CD workflow runs `ReleaseAPI`, which builds and pushes a
commit-tagged image, applies Terraform, and prints the API URL. The CI
workflow runs on pushes, pull requests, and manual dispatches to build and
test the application. Run deployment targets from the repository root:

```powershell
.\build.ps1 ProvisionAPIInfrastructure
.\build.ps1 PlanRelease
.\build.ps1 ReleaseAPI
```

The pipeline defaults to Google Cloud project `georetrievo`, region
`us-central1`, and state bucket `georetrievo-terraform-state`. Override the
project with `GOOGLE_CLOUD_PROJECT` or `GCP_PROJECT_ID`, the region with
`GOOGLE_CLOUD_REGION` or `GCP_REGION`, and the state bucket with
`TERRAFORM_STATE_BUCKET`. Import any pre-existing bucket IAM grant before the
first Terraform apply, as described above.

Keep `terraform.tfvars`, credentials, state, and plan files out of source
control.
