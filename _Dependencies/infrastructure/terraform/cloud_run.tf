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

resource "google_cloud_run_v2_service" "ui" {
  count = var.ui_container_image == "" || var.container_image == "" ? 0 : 1

  project             = var.project_id
  name                = var.ui_service_name
  location            = var.region
  ingress             = "INGRESS_TRAFFIC_ALL"
  deletion_protection = false

  template {
    service_account                  = google_service_account.ui.email
    max_instance_request_concurrency = 1
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

      env {
        name  = "Api__BaseUrl"
        value = google_cloud_run_v2_service.api[0].uri
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
    google_cloud_run_v2_service_iam_member.ui_api_invoker,
  ]
}

resource "google_cloud_run_domain_mapping" "ui" {
  count = var.ui_domain_name == "" || var.ui_container_image == "" || var.container_image == "" ? 0 : 1

  project  = var.project_id
  location = var.region
  name     = var.ui_domain_name

  metadata {
    namespace = var.project_id
  }

  spec {
    route_name = google_cloud_run_v2_service.ui[0].name
  }
}
