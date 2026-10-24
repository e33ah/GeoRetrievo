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

### Configure the API URL

Vite reads `VITE_API_BASE_URL` from mode-specific environment files:

| File | Used by | Default |
| --- | --- | --- |
| `.env.development` | `npm run dev` | `http://localhost:5294` |
| `.env.production` | `npm run build` | Deployed API URL |

For a local-only development override, create
`.env.development.local` in this directory:

```dotenv
VITE_API_BASE_URL=http://localhost:5294
```

Files ending in `.local` are ignored by Git. Production builds bake the API
URL into the static assets; set `VITE_API_BASE_URL` when building the
container if the default deployment URL is not appropriate. The API must
allow the UI's origin through its CORS configuration.

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

From the repository root, pass the API URL at build time:

```powershell
docker build `
  --build-arg VITE_API_BASE_URL=https://your-api.example.com `
  -t georetrievo-ui:local `
  .\GeoRetrievo.UI
docker run --rm -p 8080:8080 georetrievo-ui:local
```

Open [http://localhost:8080](http://localhost:8080). The image builds the
static client with Node.js and serves it using Nginx on port `8080`, with
fallback routing for client-side paths. Configure the API to allow the
deployed UI origin before using the client against that API.

## API integration

The UI uses these API routes:

- `GET /countries` — retrieve the country catalogue.
- `GET /countries/{code}` — retrieve a country by code.
- `GET /health` — check API health.

Request and error handling lives in `src/services`; React state and retry
behavior live in `src/hooks`. In development, API request diagnostics are
written to the browser console. These logs are not production monitoring.
