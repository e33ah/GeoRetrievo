output "api_url" {
  description = "HTTPS URL of the deployed API. Null until container_image is set."
  value       = try(google_cloud_run_v2_service.api[0].uri, null)
}

output "ui_url" {
  description = "HTTPS URL of the deployed UI. Null until ui_container_image is set."
  value       = try(google_cloud_run_v2_service.ui[0].uri, null)
}

output "artifact_registry_repository" {
  description = "Artifact Registry repository URL for API container images."
  value       = "${var.region}-docker.pkg.dev/${var.project_id}/${google_artifact_registry_repository.api.repository_id}"
}

output "runtime_service_account" {
  description = "Existing Cloud Run runtime service account email."
  value       = data.google_service_account.api.email
}
