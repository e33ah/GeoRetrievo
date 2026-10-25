# GeoRetrievo UI

The web client for GeoRetrievo is built with React, TypeScript, and Vite. It
provides country search, region filtering, sorting, and a responsive catalogue
view. For the full-stack local setup, start with the repository
[README](../README.md).

## Requirements

- Node.js 22 and npm
- .NET 10 SDK to run the local API

## Develop

Start the API as described in the repository
[README](../README.md#run-locally). In another terminal, install dependencies
and start the UI:

```sh
cd GeoRetrievo.UI
npm ci
npm run dev
```

Open [http://localhost:5173](http://localhost:5173). The API is expected at
`http://localhost:5294`.

### Configure the development API target

The browser always calls same-origin `/api` routes. During development, Vite
proxies those requests to the local API. To change the target, create
`.env.development.local` in this directory:

```dotenv
VITE_API_BASE_URL=http://localhost:5294
```

Files ending in `.local` are ignored by Git. Production builds do not contain
the API URL; Cloud Run supplies `Api__BaseUrl` to the .NET UI proxy at runtime.

## Scripts

Run these commands from `GeoRetrievo.UI`:

| Command | Purpose |
| --- | --- |
| `npm run dev` | Start the Vite development server with hot reload. |
| `npm run test:run` | Run the UI test suite once. |
| `npm run lint` | Check the TypeScript and React source with ESLint. |
| `npm run build` | Type-check and create the production build in `dist/`. |
| `npm run preview` | Serve the production build locally. |

## Build and run the container

The UI image builds the React assets with Node.js, then publishes them with the
ASP.NET Core UI proxy. It is designed to run on Cloud Run, where the proxy uses
the attached UI service account to obtain an ID token for the private API.

```powershell
docker build `
  -f .\GeoRetrievo.Proxy\Dockerfile `
  -t georetrievo-ui:local `
  .
```

The container requires `Api__BaseUrl` and Cloud Run metadata credentials at
runtime; local UI development should use Vite's proxy instead of running this
container directly.

## API integration

The browser calls these same-origin routes; the .NET proxy forwards them to
the private API with a Cloud Run ID token:

- `GET /api/countries` — retrieve the country catalogue.
- `GET /api/countries/{code}` — retrieve a country by code.
- `GET /health` — check API health.

Request and error handling lives in `src/services`; React state and retry
behavior live in `src/hooks`. In development, API request diagnostics are
written to the browser console. These logs are not production monitoring.
