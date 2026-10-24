terraform {
  backend "gcs" {
    bucket = "georetrievo-terraform-state"
  }
}
