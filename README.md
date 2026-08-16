# IT Help Desk

A full-stack help desk application for submitting, assigning, tracking, and resolving IT support tickets.

## Features

- Role-based access for employees, agents, and administrators
- Ticket workflow with assignment, escalation, history, internal notes, and attachments
- Real-time notifications and ticket conversations via SignalR
- Dashboards, activity logs, and operational reports
- User, role, category, and system-settings administration

## Tech Stack

- Frontend: React, TypeScript, Vite, Tailwind CSS, and TanStack Query
- Backend: ASP.NET Core (.NET 10), Entity Framework Core, ASP.NET Identity, and SignalR
- Database: SQL Server

## Requirements

- .NET 10 SDK
- Node.js 20 or later and npm
- SQL Server (LocalDB, SQL Server Express, or a full SQL Server instance)

## Architecture

The backend follows a layered structure:

- `HelpDesk.Api` — REST API, SignalR hub, middleware, and application startup
- `HelpDesk.Application` — use cases, validation, and application interfaces
- `HelpDesk.Domain` — core business rules and entities
- `HelpDesk.Infrastructure` — SQL Server persistence, Identity, JWT, email, file storage, and reporting

The React frontend is organized by feature, including authentication, tickets, reporting, administration, users, and notifications.

## Getting Started

### Backend

1. Configure the application with user secrets or an environment-specific `appsettings` file. At minimum, provide a SQL Server connection string, JWT settings, and the seeded administrator account:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=HelpDesk;Trusted_Connection=True;TrustServerCertificate=True"
     },
     "Jwt": {
       "Issuer": "HelpDesk",
       "Audience": "HelpDeskClient",
       "Key": "use-a-secure-secret-with-at-least-32-bytes",
       "ExpiryMinutes": 60,
       "RefreshTokenExpiryDays": 7
     },
     "Admin": {
       "Email": "admin@example.com",
       "Password": "ChangeThisPassword1!",
       "FirstName": "System",
       "LastName": "Administrator"
     }
   }
   ```

   Do not commit passwords, JWT keys, or connection strings to the repository. Configure SMTP and attachment storage as needed for password recovery and file uploads.

2. Apply the included Entity Framework Core migrations:

   ```bash
   cd backend/HelpDesk.Api
   dotnet ef database update --project ../HelpDesk.Infrastructure
   ```

3. Run the API:

   ```bash
   cd backend/HelpDesk.Api
   dotnet run
   ```

### Frontend

1. Create `frontend/.env` and point the client to the running API:

   ```env
   VITE_API_URL=https://localhost:7146
   ```

2. Install dependencies and start the development server:

   ```bash
   cd frontend
   npm install
   npm run dev
   ```

The frontend runs at `http://localhost:5173` by default.

## Available Commands

| Area | Command | Purpose |
| --- | --- | --- |
| Frontend | `npm run dev` | Start the Vite development server |
| Frontend | `npm run build` | Type-check and create a production build |
| Frontend | `npm run lint` | Run the frontend linter |
| Backend | `dotnet run --project backend/HelpDesk.Api` | Start the API |
| Backend | `dotnet test backend/HelpDesk.slnx` | Run backend tests |

## Configuration

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server database connection |
| `Jwt` | Access-token issuer, audience, signing key, and expiry settings |
| `Admin` | Initial administrator account seeded at startup |
| `Smtp` | Email delivery and password-reset settings |
| `Attachments` | Local upload storage, size limit, and allowed extensions |
| `Reporting` | SLA targets and PDF font locations |
| `VITE_API_URL` | API base URL used by the frontend |

## Project Structure

```text
backend/   ASP.NET Core API and application layers
frontend/  React single-page application
```

## License

This project is intended for educational or internal use. Add a license file before distributing it publicly.
