# 🌍GeoRetrievo

<p align="center">
    <img width="806" height="464" alt="GeoRetrievo" src="https://github.com/user-attachments/assets/0ae891d5-28b2-4e44-9ecb-e1c3aae46c3b" />
</p>

GeoRetrievo is a full-stack country catalogue application built with a
React + TypeScript client, an ASP.NET Core UI host/proxy, and an ASP.NET Core
API. It provides structured search, filtering, and sorting over country
metadata through a responsive, component-driven frontend and a clean,
service-oriented backend.

The application is fully deployed on Google Cloud using Terraform‑managed
infrastructure, demonstrating cloud‑ready architecture, infrastructure‑as‑code
workflows, and a reproducible end‑to‑end deployment pipeline.


## Technology

| Area | Technology |
| --- | --- |
| API           | .NET 10, ASP.NET Core minimal APIs |
| Web client    | React 19, TypeScript, Vite |
| UI host/proxy | .NET 10, ASP.NET Core |
| Data          | Country catalogue in JSON; local file for development or Cloud Storage |
| Deployment    | Fallout, Docker, Google Cloud Run, Artifact Registry, Terraform |

## Run locally

### Requirements

- .NET 10 SDK
- Node.js 22 and npm
- Docker Desktop, only for the container workflow
- Fallout , only for CI/CD and local builds

Start the API from the repository root:

```sh
dotnet run --project GeoRetrievo.Api
```

The API uses the country JSON file included in the project when no Cloud
Storage bucket is configured. Start the web client by following the
[UI developer guide](GeoRetrievo.UI/README.md); Vite proxies its same-origin
`/api` requests to `http://localhost:5294` by default. The UI is available at
[http://localhost:5173](http://localhost:5173).

## Test and build

Run the backend and proxy tests:

```sh
dotnet run --project GeoRetrievo.Test\GeoRetrievo.Api.Test.csproj
dotnet run --project GeoRetrievo.Proxy.Test\GeoRetrievo.Proxy.Test.csproj
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

## Run the API container locally

The API container can run locally without Google Cloud authentication. From
the repository root, build and start it:

```powershell
docker build -t georetrievo-api:local .\GeoRetrievo.Api
docker run --rm -p 5294:8080 georetrievo-api:local
```

In another terminal, run the UI through Vite as described in the
[UI developer guide](GeoRetrievo.UI/README.md#develop), then open
[http://localhost:5173](http://localhost:5173). This keeps local development
independent of Cloud Run ID-token credentials.

To run the API against Cloud Storage instead, use `compose.yaml`, which mounts
local Google Application Default Credentials and reads the configured bucket.
Authenticate with `gcloud auth application-default login` first. See the
[infrastructure guide](_Dependencies/infrastructure/terraform/README.md) for
cloud deployment.
