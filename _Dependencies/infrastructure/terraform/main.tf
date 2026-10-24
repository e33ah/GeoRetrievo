locals {
  required_apis = toset([
    "artifactregistry.googleapis.com",
    "cloudbuild.googleapis.com",
    "iam.googleapis.com",
    "run.googleapis.com",
    "storage.googleapis.com",
  ])

  api_cors_allowed_origins = var.ui_container_image == "" ? var.cors_allowed_origins : concat(
    var.cors_allowed_origins,
    [google_cloud_run_v2_service.ui[0].uri],
  )
}

resource "google_project_service" "required" {
  for_each = local.required_apis

  project            = var.project_id
  service            = each.value
  disable_on_destroy = false
}

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

resource "google_artifact_registry_repository" "api" {
  project       = var.project_id
  location      = var.region
  repository_id = var.repository_id
  description   = "Container images for the GeoRetrievo API"
  format        = "DOCKER"

  cleanup_policy_dry_run = false

  cleanup_policies {
    id     = "delete-old-untagged-images"
    action = "DELETE"

    condition {
      tag_state  = "UNTAGGED"
      older_than = "1209600s"
    }
  }

  depends_on = [google_project_service.required]
}

resource "google_cloud_run_v2_service" "api" {
  count = var.container_image == "" ? 0 : 1

  project             = var.project_id
  name                = var.service_name
  location            = var.region
  ingress             = "INGRESS_TRAFFIC_ALL"
  deletion_protection = false

  template {
    service_account                  = data.google_service_account.api.email
    max_instance_request_concurrency = 1
    timeout                          = "300s"
    execution_environment            = "EXECUTION_ENVIRONMENT_GEN1"

    scaling {
      min_instance_count = 0
      max_instance_count = var.max_instance_count
    }

    containers {
      image = var.container_image

      ports {
        container_port = 8080
      }

      resources {
        cpu_idle = true

        limits = {
          cpu    = "0.25"
          memory = var.memory
        }
      }

      env {
        name  = "CountryData__Bucket"
        value = var.country_data_bucket
      }

      env {
        name  = "CountryData__Object"
        value = var.country_data_object
      }

      dynamic "env" {
        for_each = local.api_cors_allowed_origins

        content {
          name  = "Cors__AllowedOrigins__${env.key}"
          value = env.value
        }
      }
    }
  }

  traffic {
    percent = 100
    type    = "TRAFFIC_TARGET_ALLOCATION_TYPE_LATEST"
  }

  depends_on = [
    google_project_service.required,
    google_storage_bucket_iam_member.country_data_reader,
  ]
}

resource "google_cloud_run_v2_service_iam_member" "public_invoker" {
  count = var.container_image != "" && var.allow_unauthenticated ? 1 : 0

  project  = var.project_id
  location = var.region
  name     = google_cloud_run_v2_service.api[0].name
  role     = "roles/run.invoker"
  member   = "allUsers"
}

resource "google_cloud_run_v2_service" "ui" {
  count = var.ui_container_image == "" ? 0 : 1

  project             = var.project_id
  name                = var.ui_service_name
  location            = var.region
  ingress             = "INGRESS_TRAFFIC_ALL"
  deletion_protection = false

  template {
    service_account                  = google_service_account.ui.email
    max_instance_request_concurrency = 80
    timeout                          = "300s"
    execution_environment            = "EXECUTION_ENVIRONMENT_GEN1"

    scaling {
      min_instance_count = 0
      max_instance_count = var.ui_max_instance_count
    }

    containers {
      image = var.ui_container_image

      ports {
        container_port = 8080
      }

      resources {
        cpu_idle = true

        limits = {
          cpu    = "0.25"
          memory = var.ui_memory
        }
      }
    }
  }

  traffic {
    percent = 100
    type    = "TRAFFIC_TARGET_ALLOCATION_TYPE_LATEST"
  }

  depends_on = [
    google_project_service.required,
    google_service_account_iam_member.ui_deployer,
  ]
}

resource "google_cloud_run_v2_service_iam_member" "ui_public_invoker" {
  count = var.ui_container_image != "" && var.ui_allow_unauthenticated ? 1 : 0

  project  = var.project_id
  location = var.region
  name     = google_cloud_run_v2_service.ui[0].name
  role     = "roles/run.invoker"
  member   = "allUsers"
}
