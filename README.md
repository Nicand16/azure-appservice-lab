# Azure App Service Lab

Small ASP.NET Core app for practicing Azure App Service operations.

## What this app lets you practice

- Deployment to Azure App Service
- Configuration / App Settings
- Log Stream and App Service Logs
- Metrics: requests, response time, failed requests
- Application Insights: requests, failures, exceptions
- Health check endpoint
- Deployment slots: production vs staging
- Slot swap validation

## Endpoints

| Endpoint | Purpose |
|---|---|
| `/` | Simple HTML home page with environment info |
| `/api/health` | Health check response |
| `/api/config-check` | Shows non-secret config values |
| `/api/log-demo` | Generates info and warning logs |
| `/api/error` | Generates intentional HTTP 500 exception |
| `/api/slow?delay=3000` | Simulates a slow request |
| `/api/status/404` | Returns a chosen HTTP status code |
| `/api/headers` | Shows safe request headers |

## Run locally

```bash
dotnet restore
dotnet run
```

Then open the local URL shown in the terminal.

## Suggested Azure App Settings

Add these in Azure App Service > Configuration > Application settings:

| Name | Value |
|---|---|
| `APP_VERSION` | `v1-production` |
| `CUSTOM_MESSAGE` | `Hello from production` |
| `FEATURE_FLAG` | `false` |

For a staging slot, use different values:

| Name | Value |
|---|---|
| `APP_VERSION` | `v2-staging` |
| `CUSTOM_MESSAGE` | `Hello from staging slot` |
| `FEATURE_FLAG` | `true` |

Do not put secrets in this practice app.
