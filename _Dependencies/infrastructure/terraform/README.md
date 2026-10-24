# Google Cloud infrastructure

This Terraform configuration provisions the Google Cloud resources used by
GeoRetrievo: required APIs, an Artifact Registry repository, access for the
existing API runtime service account to read the country-data bucket, a
dedicated UI runtime service account, and (after images are supplied) separate
Cloud Run services for the API and UI. It does not create the API runtime
service account or the country-data bucket.

## Prerequisites

- Terraform 1.6 or later and the Google Cloud CLI.
- A Google Cloud project with billing enabled and permission to enable APIs,
  manage the documented resources, and update the country-data bucket IAM
  policy.
- An existing country-data bucket, API runtime service account, and deployment
  service account. Defaults are `country_data_01`, `georetrievo-api-svc`, and
  `github-deploy-svc@georetrievo.iam.gserviceaccount.com`; change the
  corresponding variables if your resources have different names.
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
creates the Artifact Registry repository. Cloud Run services are created once
their corresponding `container_image` or `ui_container_image` is set:

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

The API is private by default. The release pipeline makes it publicly
invokable because the public browser UI calls the API directly; catalogue
data is therefore available to unauthenticated callers. The UI is public by
default. Terraform automatically configures API CORS with the deployed UI
origin; `cors_allowed_origins` can add other origins when required.

Both services scale to zero and use request-based CPU allocation. The API
allows one request at a time per instance with 0.25 vCPU and 256 MiB of
memory; the UI allows up to 80 concurrent requests per instance with 0.25
vCPU and 128 MiB. These are low-cost starting settings, not a guarantee of
zero charges.

Artifact Registry deletes untagged images older than 14 days; tagged images
are retained. Cloud Build, storage, logging, and other Google Cloud services
may incur additional charges.

The API uses the existing runtime service account, which receives only
`roles/storage.objectViewer` on the country-data bucket. The UI uses the
dedicated `georetrievo-ui-svc` account, which receives no additional
permissions; Terraform grants the deployment account `roles/iam.serviceAccountUser`
on that UI account so it can attach it to Cloud Run. Do not deploy local ADC
files or service-account keys.

## Automated release

The `georetrievo-cd` GitHub Actions workflow is manually triggered and uses
Workload Identity Federation; it does not need a service-account key in
repository secrets. Configure its identity provider to trust this repository
and the intended branch or environment, and grant its service account the
permissions needed to provision infrastructure, push images, and deploy
Cloud Run.

The separate `PlanRelease` target formats, validates, and plans without
applying. The CD workflow runs `ReleaseUI`, which builds and pushes the API
image, deploys the API, builds the UI image with the deployed API URL, deploys
the public UI, and updates API CORS to allow the UI origin. Provision the API
infrastructure before the first release. The CI workflow runs on pushes,
pull requests, and manual dispatches to build and test the application. Run
deployment targets from the repository root:

```powershell
.\build.ps1 ProvisionAPIInfrastructure
.\build.ps1 PlanRelease
.\build.ps1 ReleaseUI
```

The pipeline defaults to Google Cloud project `georetrievo`, region
`us-central1`, and state bucket `georetrievo-terraform-state`. Override the
project with `GOOGLE_CLOUD_PROJECT` or `GCP_PROJECT_ID`, the region with
`GOOGLE_CLOUD_REGION` or `GCP_REGION`, and the state bucket with
`TERRAFORM_STATE_BUCKET`. Import any pre-existing bucket IAM grant before the
first Terraform apply, as described above.

Keep `terraform.tfvars`, credentials, state, and plan files out of source
control.
