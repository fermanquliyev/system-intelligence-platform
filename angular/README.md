# ErrorIntel — Angular UI

Single-instance web UI for **ErrorIntel**: dashboard, applications, incidents, and settings. It talks to the ABP host API; there is **no tenant switcher, tenant headers, or per-tenant routing** — one deployment maps to one backend.

## Requirements

- [Node.js 18+](https://nodejs.org/)
- npm (or yarn)

## Install

From this folder:

```bash
npm install
```

If the solution uses ABP client libraries from the repo root:

```bash
abp install-libs
```

(run from the repository root when needed)

## Connect to the API

The UI reads API and OAuth settings from:

- **Development:** `src/environments/environment.ts`
- **Production:** merged with `dynamic-env.json` via `remoteEnv` in `src/environments/environment.prod.ts`

Set:

- `apis.default.url` — base URL of the ErrorIntel host (e.g. `http://localhost:44397` or your Docker-published port)
- `oAuthConfig.issuer` — usually the same origin as the API
- `oAuthConfig.redirectUri` — Angular origin (e.g. `http://localhost:4200`)
- `oAuthConfig.clientId` — must match the OpenIddict client on the server

**No extra isolation setup:** the backend is a single instance. Optional ABP isolation headers (`__tenant`, etc.) are removed by `ClearIsolationHeadersInterceptor` so stale client state cannot affect requests.

## Run (development)

Start the **host API** first, then:

```bash
npm start
```

or:

```bash
ng serve
```

Open `http://localhost:4200/`. Sign in with a user defined on your host (see the main repository README).

## Build

```bash
ng build
```

Production:

```bash
ng build --configuration production
```

Output is under `dist/`.

## Tests

```bash
ng test
```

## Proxies

Regenerate TypeScript API clients after backend changes:

```bash
abp generate-proxy -t ng
```

(from the solution root, with paths configured for this project)

## More documentation

- [ABP Angular UI](https://abp.io/docs/latest/framework/ui/angular/overview)
- [Angular CLI](https://angular.dev/tools/cli)
