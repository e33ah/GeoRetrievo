variable "project_id" {
  description = "Google Cloud project that hosts the Cloud Run service and Artifact Registry repository."
  type        = string
}

variable "region" {
  description = "Google Cloud region for Cloud Run and Artifact Registry."
  type        = string
}

variable "service_name" {
  description = "Cloud Run service name."
  type        = string
  default     = "georetrievo-api"
}

variable "ui_service_name" {
  description = "Cloud Run service name for the web UI."
  type        = string
  default     = "georetrievo-ui"
}

variable "repository_id" {
  description = "Artifact Registry Docker repository ID."
  type        = string
  default     = "georetrievo"
}

variable "runtime_service_account_id" {
  description = "Account ID of the existing Cloud Run runtime service account."
  type        = string
  default     = "georetrievo-api-svc"
}

variable "container_image" {
  description = "Full Artifact Registry image URI. Leave empty during initial provisioning, then set after pushing the image."
  type        = string
  default     = ""
}

variable "ui_container_image" {
  description = "Full Artifact Registry image URI for the UI. Leave empty until the UI image is pushed."
  type        = string
  default     = ""
}

variable "country_data_bucket" {
  description = "Existing Cloud Storage bucket containing the country catalog."
  type        = string
  default     = "country_data_01"
}

variable "country_data_object" {
  description = "Object name of the country catalog in the bucket."
  type        = string
  default     = "countries.json"
}

variable "cors_allowed_origins" {
  description = "Additional allowed browser origins. The deployed UI origin is added automatically when the UI is deployed."
  type        = list(string)
  default     = []

  validation {
    condition     = alltrue([for origin in var.cors_allowed_origins : can(regex("^https?://[^/]+$", origin))])
    error_message = "Each CORS origin must contain only a scheme and host (and optional port), without a path or trailing slash."
  }
}

variable "allow_unauthenticated" {
  description = "Whether to allow public invocation of the API Cloud Run service."
  type        = bool
  default     = false
}

variable "ui_allow_unauthenticated" {
  description = "Whether to allow public invocation of the UI Cloud Run service."
  type        = bool
  default     = true
}

variable "max_instance_count" {
  description = "Maximum number of Cloud Run instances. A value of 1 limits capacity and helps control costs."
  type        = number
  default     = 1

  validation {
    condition     = var.max_instance_count >= 1
    error_message = "max_instance_count must be at least 1."
  }
}

variable "ui_max_instance_count" {
  description = "Maximum number of Cloud Run UI instances."
  type        = number
  default     = 2

  validation {
    condition     = var.ui_max_instance_count >= 1
    error_message = "ui_max_instance_count must be at least 1."
  }
}

variable "memory" {
  description = "Memory limit for each Cloud Run instance. Increase if the application runs out of memory."
  type        = string
  default     = "256Mi"
}

variable "ui_memory" {
  description = "Memory limit for each Cloud Run UI instance."
  type        = string
  default     = "128Mi"
}
