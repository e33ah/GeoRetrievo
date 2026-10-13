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

