
## Deploying the API to Google Cloud Run

The API reads `Data/countries.json` locally by default. To load the dataset from
Cloud Storage, set `CountryData__Bucket` to the bucket name (without `gs://`) and
`CountryData__Object` to the object's name/path, for example
`data/countries.json`. The API downloads and parses the file at startup, so a
missing object, invalid JSON, or missing permission will prevent startup rather
than silently serving stale/local data.

1. Create or select a Google Cloud service account for the API, then grant it
   `roles/storage.objectViewer` on the bucket containing the country JSON. For
   example:

   ```powershell
   gcloud storage buckets add-iam-policy-binding gs://YOUR_BUCKET `
     --member="serviceAccount:YOUR_SERVICE_ACCOUNT" `
     --role="roles/storage.objectViewer"
   ```

2. Deploy the API source directory to Cloud Run, assigning that service account
   and setting the bucket/object configuration:

   ```powershell
   gcloud run deploy georetrievo-api `
     --source .\GeoRetrievo.Api `
     --region YOUR_REGION `
     --service-account YOUR_SERVICE_ACCOUNT `
     --set-env-vars "CountryData__Bucket=YOUR_BUCKET,CountryData__Object=data/countries.json"
   ```

   Cloud Run uses Application Default Credentials for the assigned service
   account; do not add a service-account key file to the application or bucket.
   The API listens on the port supplied by Cloud Run.
   Add `--allow-unauthenticated` to the deploy command only if the API should
   be publicly reachable.
3. Set `Cors__AllowedOrigins__0` on the API service to the deployed UI's exact
   origin (for example, `https://your-ui.example.com`). The localhost origin is
   used when no allowed origins are configured. In the UI's build environment,
   set `VITE_API_BASE_URL` to the Cloud Run API URL.

For local testing against Cloud Storage, authenticate with Application Default
Credentials (`gcloud auth application-default login`) and set
`CountryData__Bucket` / `CountryData__Object` in the API's environment. Leaving
the bucket setting unset continues to use the local JSON file.