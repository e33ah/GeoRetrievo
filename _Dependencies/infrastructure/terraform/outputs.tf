output "api_url" {
  description = "HTTPS URL of the deployed API. Null until container_image is set."
  value       = try(google_cloud_run_v2_service.api[0].uri, null)
}

output "ui_url" {
  description = "Public HTTPS URL of the deployed UI, using the custom domain when configured. Null until both images are set."
  value = var.ui_container_image != "" && var.container_image != "" ? (
    var.ui_domain_name != "" ? "https://${var.ui_domain_name}" : try(google_cloud_run_v2_service.ui[0].uri, null)
  ) : null
}

output "ui_cloud_run_url" {
  description = "Default HTTPS URL of the deployed UI Cloud Run service. Null until both images are set."
  value       = try(google_cloud_run_v2_service.ui[0].uri, null)
}

output "ui_domain_dns_records" {
  description = "DNS records required by the custom UI domain mapping. Empty until the mapping is created."
  value       = try(google_cloud_run_domain_mapping.ui[0].status[0].resource_records, [])
}

output "artifact_registry_repository" {
  description = "Artifact Registry repository URL for API container images."
  value       = "${var.region}-docker.pkg.dev/${var.project_id}/${google_artifact_registry_repository.api.repository_id}"
}

output "runtime_service_account" {
  description = "Existing Cloud Run runtime service account email."
  value       = data.google_service_account.api.email
}

output "ui_runtime_service_account" {
  description = "Dedicated UI Cloud Run runtime service account email."
  value       = google_service_account.ui.email
}
