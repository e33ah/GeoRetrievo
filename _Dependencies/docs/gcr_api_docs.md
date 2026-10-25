
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
   Do not add `--allow-unauthenticated`; the API must remain private.
3. Grant only the UI runtime service account permission to invoke the API:

   ```powershell
   gcloud run services add-iam-policy-binding georetrievo-api `
     --region YOUR_REGION `
     --member="serviceAccount:YOUR_UI_SERVICE_ACCOUNT" `
     --role="roles/run.invoker"
   ```

   In production, the UI's .NET proxy gets an ID token for the API URL from
   Cloud Run's metadata server. The React app calls same-origin `/api` routes;
   no API URL or token is included in browser assets, and API CORS is not
   needed.

For local testing against Cloud Storage, authenticate with Application Default
Credentials (`gcloud auth application-default login`) and set
`CountryData__Bucket` / `CountryData__Object` in the API's environment. Leaving
the bucket setting unset continues to use the local JSON file.