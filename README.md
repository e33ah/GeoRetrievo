# GeoRetrievo

GeoRetrievo is a full-stack application for exploring country data across the world. It combines a .NET backend with a React frontend to provide search, filtering, and browsing over a shared country catalog.


## Project overview

- Backend API: `GeoRetrievo.Api`
  - Provides the country dataset over HTTP
  - Exposes endpoints for country retrieval and health checks
  - Built with ASP.NET Core minimal APIs

- Frontend app: `GeoRetrievo.UI`
  - Provides the user interface for browsing and searching countries
  - Built with React, TypeScript, and Vite
  - Connects to the backend API and renders filtered results

- Test project: `GeoRetrievo.Test`
  - Covers API and service behaviour for the backend



## Getting started

### 1. Run the API

```bash
dotnet run --project GeoRetrievo.Api
```

### 2. Run the UI

```bash
cd GeoRetrievo.UI
npm install
npm run dev
```

Then open the local Vite URL in your browser, typically `http://localhost:5173`.

## Typical workflow

1. Start the backend API.
2. Start the frontend client.
3. Search, filter, and browse the country catalog.
4. Use the test project to validate the API and service logic.


## Run the API in local Docker

On Windows, first authenticate for local Application Default Credentials:

```powershell
gcloud auth application-default login
docker compose up --build
```

The Compose file mounts the local ADC credentials read-only and serves the API
at `http://localhost:5294`, which matches the UI's default API URL. The default
bucket is `country_data_01` and the default object is `countries.json`. Override
either for the current PowerShell session if needed:

```powershell
$env:COUNTRY_DATA_BUCKET = "your-bucket-name"
$env:COUNTRY_DATA_OBJECT = "path/to/countries.json"
docker compose up --build
```

The authenticated Google account must have permission to read the object. Do
not add ADC credentials to the repository or container image.

**Compose and `docker run` behave differently by default:** Compose supplies
`CountryData__Bucket` and `CountryData__Object`, so it loads from Cloud Storage.
A plain `docker run` does not read `compose.yaml`; unless you pass
`CountryData__Bucket` yourself, the API uses the local `Data/countries.json`
file included in the image. To use Cloud Storage with `docker run`, pass those
configuration variables and provide Google credentials inside the container.
