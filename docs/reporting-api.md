# Dashboard and reporting backend

All reporting routes require an authenticated JWT and the `Reporting` policy.
Only `Admin` and `Manager` satisfy that policy. The Application service repeats
the permission check so the authorization rule remains enforced outside HTTP.

## Endpoints

| Method and route | Purpose | Query parameters |
| --- | --- | --- |
| `GET /api/reports/dashboard` | Current ticket widgets, chart series, agent metrics, average resolution, and SLA summary | `fromUtc`, `toUtc` |
| `GET /api/reports/agent-performance` | Chart-ready metrics for every active support agent | `fromUtc`, `toUtc` |
| `GET /api/reports/monthly` | Zero-filled monthly created/resolved/closed/cancelled series | `months` (1-60) |
| `GET /api/reports/sla` | Overall and per-priority SLA analytics | `fromUtc`, `toUtc` |
| `GET /api/reports/employee-activity` | Paginated user activity report | `fromUtc`, `toUtc`, `role`, `pageNumber`, `pageSize` (1-100) |
| `GET /api/reports/export` | Download a PDF or XLSX report | `type`, `format`, plus the applicable filters above |

Export `type` values are `Dashboard`, `MonthlyTickets`, `AgentPerformance`,
`Sla`, and `EmployeeActivity`. Export `format` values are `Pdf` and `Excel`.

When no reporting period is supplied, the configured default ending at the
current UTC time is used. Current dashboard counts and category/priority
distributions are point-in-time values; agent, resolution, and SLA metrics use
the returned reporting period.

## Metric definitions

- Resolution duration is the elapsed time from ticket creation to its first
  transition to `Resolved` or direct transition to `Closed`.
- Agent performance counts currently assigned active/pending tickets and
  tickets resolved by the current assignee during the selected period.
- SLA evaluation uses tickets created during the selected period. A resolved
  ticket is compliant when its resolution duration is at or below the target
  for its priority.
- An unresolved ticket is at risk after the configured percentage of its SLA
  target and breached after the complete target has elapsed.
- Employee activity combines ticket creation/resolution, comments, and
  successful/failed audited API actions.

## Configuration

The `Reporting` section in `HelpDesk.Api/appsettings.json` controls the default
period, SLA thresholds, and PDF font files. The font paths must point to readable
TrueType files in the deployment environment.

## Database migration

The `AddReportingLifecycleMetrics` migration adds and backfills
`ResolvedAtUtc` and `ClosedAtUtc`. Apply it from the `backend` directory:

```bash
dotnet ef database update \
  --project HelpDesk.Infrastructure/HelpDesk.Infrastructure.csproj \
  --startup-project HelpDesk.Api/HelpDesk.Api.csproj
```
