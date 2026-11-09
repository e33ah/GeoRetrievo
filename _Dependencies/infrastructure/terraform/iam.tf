data "google_service_account" "api" {
  project    = var.project_id
  account_id = var.runtime_service_account_id

  depends_on = [google_project_service.required]
}

resource "google_service_account" "ui" {
  project      = var.project_id
  account_id   = var.ui_runtime_service_account_id
  display_name = "GeoRetrievo UI Cloud Run runtime"

  depends_on = [google_project_service.required]
}

resource "google_service_account_iam_member" "ui_deployer" {
  service_account_id = google_service_account.ui.name
  role               = "roles/iam.serviceAccountUser"
  member             = "serviceAccount:${var.deployment_service_account_email}"
}

resource "google_storage_bucket_iam_member" "country_data_reader" {
  bucket = var.country_data_bucket
  role   = "roles/storage.objectViewer"
  member = "serviceAccount:${data.google_service_account.api.email}"

  depends_on = [google_project_service.required]
}

resource "google_cloud_run_v2_service_iam_member" "ui_api_invoker" {
  count = var.container_image != "" && var.ui_container_image != "" ? 1 : 0

  project  = var.project_id
  location = var.region
  name     = google_cloud_run_v2_service.api[0].name
  role     = "roles/run.invoker"
  member   = "serviceAccount:${google_service_account.ui.email}"
}

resource "google_cloud_run_v2_service_iam_member" "ui_public_invoker" {
  count = var.ui_container_image != "" && var.ui_allow_unauthenticated ? 1 : 0

  project  = var.project_id
  location = var.region
  name     = google_cloud_run_v2_service.ui[0].name
  role     = "roles/run.invoker"
  member   = "allUsers"
}
