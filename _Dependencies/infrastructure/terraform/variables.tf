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

variable "ui_domain_name" {
  description = "Custom domain mapped to the public web UI. Set to an empty string to disable the mapping."
  type        = string
  default     = "georetrievo.uk"
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

variable "ui_runtime_service_account_id" {
  description = "Account ID of the dedicated UI Cloud Run runtime service account to create."
  type        = string
  default     = "georetrievo-ui-svc"
}

variable "deployment_service_account_email" {
  description = "Email of the deployment service account allowed to attach the UI runtime service account."
  type        = string
  default     = "github-deploy-svc@georetrievo.iam.gserviceaccount.com"
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

variable "allow_unauthenticated" {
  description = "Deprecated compatibility setting; the API is always private and this value is ignored."
  type        = bool
  default     = false
}

variable "cors_allowed_origins" {
  description = "Deprecated compatibility setting; browser requests use the same-origin UI proxy."
  type        = list(string)
  default     = []
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
