# 🌍GeoRetrievo

<p align="center">
    <img width="806" height="464" alt="GeoRetrievo" src="https://github.com/user-attachments/assets/0ae891d5-28b2-4e44-9ecb-e1c3aae46c3b" />
</p>

GeoRetrievo is a full‑stack country catalogue application built with a 
React + TypeScript client and an ASP.NET Core API. It provides structured
search, filtering, and sorting over country metadata through a responsive,
component‑driven frontend and a clean, service‑oriented backend.

The application is deployed on Google Cloud using Terraform‑managed
infrastructure, demonstrating cloud‑ready architecture, infrastructure‑as‑code
workflows, and a reproducible end‑to‑end deployment pipeline.


## Technology

| Area | Technology |
| --- | --- |
| API           | .NET 10, ASP.NET Core minimal APIs |
| Web client    | React 19, TypeScript, Vite |
| Data          | Country catalogue in JSON; local file for development or Cloud Storage |
| Deployment    | Fallout, Docker, Google Cloud Run, Artifact Registry, Terraform |

## Run locally

### Requirements

- .NET 10 SDK
- Node.js 22 and npm
- Docker Desktop, only for the container workflow

Start the API from the repository root:

```sh
dotnet run --project GeoRetrievo.Api
```

The API uses the country JSON file included in the project when no Cloud
Storage bucket is configured. Start the web client by following the
[UI developer guide](GeoRetrievo.UI/README.md); it connects to the API at
`http://localhost:5294` by default and is available at
[http://localhost:5173](http://localhost:5173).

## Test and build

Run the backend tests:

```sh
dotnet run --project GeoRetrievo.Test
```

Frontend tests, linting, and production builds are documented in the
[UI developer guide](GeoRetrievo.UI/README.md#scripts).

## CI/CD

Fallout orchestrates the build pipeline. GitHub Actions runs backend and UI
builds and tests on pushes and pull requests; a separate workflow deploys the
API and UI containers to Cloud Run.

CI builds are triggered on pushes to the `main` branch and on pull requests.
They can also be run locally:
```sh
./build.sh GeoRetrievoCI
```

Application releases are manually triggered and deploy the API and UI images
to separate Google Cloud Run services. See the
[infrastructure guide](_Dependencies/infrastructure/terraform/README.md#automated-release)
for deployment requirements and steps.

## Run the containers locally

The API and UI can be run as separate containers without Google Cloud
authentication. From the repository root, build and start the API:

```powershell
docker build -t georetrievo-api:local .\GeoRetrievo.Api
docker run --rm -p 5294:8080 `
  -e "Cors__AllowedOrigins__0=http://localhost:8080" `
  georetrievo-api:local
```

In another terminal, build and start the UI container using the instructions
in the [UI developer guide](GeoRetrievo.UI/README.md#build-and-run-the-container),
passing `http://localhost:5294` as its API URL. Open
[http://localhost:8080](http://localhost:8080). Keep both containers running
while using the app; press **Ctrl+C** in each terminal to stop them. The API
image uses the included country file unless configured to use Cloud Storage.

To run the API against Cloud Storage instead, use `compose.yaml`, which mounts
local Google Application Default Credentials and reads the configured bucket.
Authenticate with `gcloud auth application-default login` first. See the
[infrastructure guide](_Dependencies/infrastructure/terraform/README.md) for
cloud deployment.
