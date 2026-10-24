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

### Typical workflow

1. Start the backend API.
2. Start the frontend client.
3. Search, filter, and browse the country catalog.
4. Use the test project to validate the API and service logic.

### 1. Run the API

```sh
dotnet run --project GeoRetrievo.Api
```

### 2. Run the UI

```sh
cd GeoRetrievo.UI
npm install
npm run dev
```

Then open the local Vite URL in your browser, typically `http://localhost:5173`.

### 3. Run tests

```sh
dotnet run --project GeoRetrievo.test

cd GeoRetrievo.UI
npm run test:run
```

## Run the full app in Docker Desktop

These commands build and run the API and UI as separate local Docker
containers. They use the country data file included in the API image and do
not require Google Cloud authentication. Run the API in one PowerShell
terminal from the repository root:

```powershell
docker build -t georetrievo-api:local .\GeoRetrievo.Api
docker run --rm -p 5294:8080 `
  -e "Cors__AllowedOrigins__0=http://localhost:8080" `
  georetrievo-api:local
```

In a second PowerShell terminal, also from the repository root, build and run
the UI:

```powershell
docker build `
  --build-arg VITE_API_BASE_URL=http://localhost:5294 `
  -t georetrievo-ui:local `
  .\GeoRetrievo.UI
docker run --rm -p 8080:8080 georetrievo-ui:local
```

Open [http://localhost:8080](http://localhost:8080). The browser loads the UI
from port `8080` and calls the API through `http://localhost:5294`; the API's
CORS setting above allows that browser origin. Keep both terminal processes
running while using the app, and press **Ctrl+C** in each terminal to stop its
container.

This direct `docker run` setup does not read `compose.yaml`. Compose is
configured to mount local Application Default Credentials and load country
data from Cloud Storage; use that route only if you want the containerized API
to read from GCS.
