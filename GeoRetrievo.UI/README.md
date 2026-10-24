# GeoRetrievo UI — Developer Guide

The client-side web application for the **GeoRetrievo** global country intelligence platform. Built with **React 19**, **TypeScript**, and **Vite**, connecting to the **GeoRetrievo.Api** (.NET minimal API) service.

---

## ⚡ Quickstart (Under 2 Minutes)

```bash
# 1. From the solution root, start the .NET backend API (Terminal 1)
dotnet run --project GeoRetrievo.Api

# 2. In a second terminal, enter the UI project (Terminal 2)
cd GeoRetrievo.UI

# 3. Install dependencies
npm install

# 4. (Optional) Create a local .env override
cp .env.example .env

# 5. Start the Vite dev server
npm run dev
```

Open [http://localhost:5173](http://localhost:5173) in your browser.

---

## 📋 Prerequisites

Before setting up the project, ensure you have the following installed on your machine:

| Tool | Required Version | Purpose | Check Command |
| :--- | :--- | :--- | :--- |
| **Node.js** | `v18.0.0` or higher (`v20+` recommended) | JavaScript runtime for Vite and build tooling | `node -v` |
| **npm** | `v9.0.0` or higher | Package manager | `npm -v` |
| **.NET SDK** | `10.0` (or `8.0+`) | Runs the `GeoRetrievo.Api` backend service | `dotnet --version` |
| **Modern Browser** | Chrome, Edge, Firefox, or Safari | DirectWrite / WebKit browser with modern CSS support | — |

---

## 🔧 Environment Configuration

The application uses Vite environment variables prefixed with `VITE_`.

Vite loads the API URL from a mode-specific file:

| File | Used by | API URL |
| :--- | :--- | :--- |
| `.env.development` | `npm run dev` | `http://localhost:5294` |
| `.env.production` | `npm run build` | `https://georetrievo-api-g64urgyysq-uc.a.run.app` |

To override the API URL locally, copy `.env.example` to `.env`:
   ```bash
   cp .env.example .env
   ```

Configuration variables:

| Variable | Value | Description |
| :--- | :--- | :--- |
| `VITE_API_BASE_URL` | Mode-specific | The HTTP root of the .NET backend API (`GeoRetrievo.Api`). The production value is embedded in the built UI. |

> [!NOTE]
> The backend API has a CORS policy configured in `Program.cs` that explicitly permits requests from `http://localhost:5173`. If you run Vite on a different port, update the CORS policy in the backend accordingly.

### UI diagnostics

Failed country-list requests are written to the browser console as structured error events. In local development (`import.meta.env.DEV`), the UI also logs request start, success (with country count), and failure events, including the retry attempt number. Events include a timestamp, event name, and safe diagnostic context; raw exception messages, request URLs, and country data are intentionally omitted. User-facing errors remain separate from diagnostic details. Browser-console logging is not a production monitoring service, so connect an error-monitoring provider if production error collection is needed.

---

## 💻 Developer Scripts & Workflow

All commands are run from the `GeoRetrievo.UI` directory:

| Script | Command | Description |
| :--- | :--- | :--- |
| **Development** | `npm run dev` | Launches the local Vite development server with Hot Module Replacement (HMR) at `http://localhost:5173`. |
| **Type Check & Build** | `npm run build` | Runs the TypeScript compiler in build mode (`tsc -b`), then bundles optimized production assets into `dist/`. |
| **Linting** | `npm run lint` | Runs ESLint across all `.ts` and `.tsx` source files using the flat config in `eslint.config.js`. |
| **Preview Build** | `npm run preview` | Spins up a local web server to inspect the compiled output in `dist/`. |

### Build and run the UI container

From the repository root, build the production image and run it locally:

```powershell
docker build `
  --build-arg VITE_API_BASE_URL=https://georetrievo-api-g64urgyysq-uc.a.run.app `
  -t georetrievo-ui:local `
  .\GeoRetrievo.UI
docker run --rm -p 8080:8080 georetrievo-ui:local
```

Open [http://localhost:8080](http://localhost:8080). The image uses a
multi-stage build: Node.js compiles the Vite app, then Nginx serves the static
files on port `8080` with SPA route fallback. `VITE_API_BASE_URL` is baked into
the frontend at build time; pass the deployed API URL when building for Cloud
Run. Add the deployed UI origin to the API's `cors_allowed_origins` Terraform
variable before making browser requests to the API.

---

## 🏗️ Architecture & Component Design

The codebase follows a modular, decoupled architecture separating UI presentation, custom React hooks, API services, and utility helpers:

```
src/
├── components/               # Modular UI presentation components
│   ├── HeroHeader.tsx        # Title, badge, and live global statistics overview
│   ├── ControlPanel.tsx      # Search bar, keyboard shortcut hint, sort dropdown, region chips
│   ├── CountryCard.tsx       # Individual country card (flag, code, region tag, specs dl)
│   ├── EmptyState.tsx        # Zero-state placeholder when search matches 0 countries
│   ├── ErrorState.tsx        # API connection failure alert with retry button
│   └── LoadingSkeleton.tsx   # Shimmer pulse placeholder cards during fetch
├── hooks/                    # Reusable React state & effect logic
│   ├── useCountries.ts       # AbortController-managed API fetching, state, and retry handler
│   └── useSearchShortcut.ts  # Global window listener for '/' focus and 'Escape' clear
├── services/                 # External communication layer
│   └── countriesApi.ts       # Type-safe fetch client for backend endpoints
├── types/                    # Domain data models
│   └── Country.ts            # Country interface contract
├── utils/                    # Pure helper functions
│   └── formatters.ts         # ISO-to-flag emoji generator, population formatters, badge styles
├── App.css                   # Component-level styles, animations, grid, and glassmorphism
├── index.css                 # Global CSS variables, reset, and viewport-locked background
├── App.tsx                   # Main orchestrator (filters, memoized sorting, layout)
└── main.tsx                  # React DOM root entry point
```

### Data Flow

```
[Backend API: http://localhost:5294/countries]
                       │
                       ▼
            [services/countriesApi.ts] (Fetch + AbortSignal)
                       │
                       ▼
             [hooks/useCountries.ts] (loading, error, retry, countries)
                       │
                       ▼
                  [App.tsx] (Search query + Region filter + Sort options)
                       │
         ┌─────────────┴─────────────┐
         ▼                           ▼
[HeroHeader / ControlPanel]    [CountryCard grid / Skeleton / EmptyState]
```

---

## 🎨 Design System & Styling Conventions

The application uses modern native CSS3 with CSS Custom Properties and zero heavy CSS frameworks:

- **Global Tokens (`src/index.css`)**:
  - Font Stack: `Plus Jakarta Sans`, `Inter`, system fallbacks
  - Brand Palette: `--accent-blue: #00D1FF`, `--accent-green: #00C96B`, `--accent-primary: #2563eb`
  - Radii: `--radius-md: 12px`, `--radius-lg: 16px`, `--radius-xl: 20px`, `--radius-full: 9999px`
- **Viewport-Locked Background**:
  - The background gradient and cartographic mesh (`globe-mesh.svg`) are anchored via `body::before` and `body::after` with `position: fixed; inset: 0; pointer-events: none;`.
  - **Why?** This prevents background gradients from compressing or shifting colors when search filtering collapses the document height from 24 cards down to 1 card.
- **Glassmorphism**:
  - Floating controls and stats pills use `backdrop-filter: blur(16px)` and translucent surfaces (`rgba(255, 255, 255, 0.9)`), refracting the ambient background underneath.

---

## 🔌 Backend API Contract

The UI expects the following REST endpoints from `GeoRetrievo.Api`:

### `GET /countries`
Returns an array of all available country records:
```json
[
  {
    "name": "Japan",
    "code": "JP",
    "capital": "Tokyo",
    "region": "Asia",
    "population": 126476461,
    "currency": "JPY",
    "callingCode": "+81"
  }
]
```

### `GET /health`
Returns service health status:
```json
{
  "status": "healthy"
}
```

---

## 🔍 Troubleshooting & Common Issues

### 1. "Could not connect to the GeoRetrievo API"
- **Cause**: The .NET API backend is not running, or is listening on a different port.
- **Solution**: Run `dotnet run --project GeoRetrievo.Api` from the root solution folder and verify it output `Now listening on: http://localhost:5294`. Then click **"Retry Connection"** in the UI.

### 2. CORS Blocked in Browser Console
- **Cause**: The frontend is running on a port other than `http://localhost:5173`.
- **Solution**: Either ensure Vite starts on `5173` (`npm run dev`), or update the CORS policy in `GeoRetrievo.Api/Program.cs`:
  ```csharp
  builder.Services.AddCors(options => {
      options.AddPolicy("ReactDev", policy => {
          policy.WithOrigins("http://localhost:5173", "http://localhost:<YOUR_PORT>")
                .AllowAnyHeader()
                .AllowAnyMethod();
      });
  });
  ```

### 3. TypeScript Build Errors on Node / React Versions
- Run `npm run build` locally. The project uses React 19 types (`@types/react: ^19.2.18`) with strict module resolution. Ensure your IDE's TypeScript version is set to use the workspace version (`~6.0.2` or TypeScript 5.8+).

---

## 🧪 Testing & Verification

To run a complete sanity check on the full stack before committing:

```bash
# Test the backend API & service logic
dotnet run --project GeoRetrievo.Test

# Test the UI linter
cd GeoRetrievo.UI
npm run lint

# Test the UI TypeScript build bundle
npm run build
```
