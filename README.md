# Event Tracker API

A small **ASP.NET Core 8 (minimal API)** service for tracking event registrations and attendance: create events, register participants, check them in, and pull an attendance report as JSON or CSV.

Inspired by a real participation-tracking platform I built professionally, rebuilt here from scratch as a clean, open example.

![CI](../../actions/workflows/ci.yml/badge.svg)

## Features

- Events: create, list, get, delete
- Participants: register (with validation) and check in
- Attendance report: totals, check-ins, no-shows, attendance rate
- CSV export, with spreadsheet formula-injection protection
- Validation errors returned as standard RFC 9110 problem details
- Storage behind an `IEventStore` interface, so the in-memory store can be swapped for EF Core / SQL Server
- CORS enabled for an Angular dev server on `http://localhost:4200`

## Run it

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/EventTracker.Api
```

Then try the requests in [`src/EventTracker.Api/requests.http`](src/EventTracker.Api/requests.http) (VS Code REST Client or Visual Studio), or use curl:

```bash
curl -X POST http://localhost:5000/api/events \
  -H "Content-Type: application/json" \
  -d '{"title":"Angular Meetup","date":"2026-11-15T10:00:00","location":"Bengaluru"}'
```

> The port is printed in the console when the app starts. Adjust `@host` in `requests.http` if yours differs.

## Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/health` | Health check |
| GET | `/api/events` | List events |
| POST | `/api/events` | Create an event |
| GET | `/api/events/{id}` | Get one event |
| DELETE | `/api/events/{id}` | Delete an event and its participants |
| GET | `/api/events/{id}/participants` | List participants |
| POST | `/api/events/{id}/participants` | Register a participant |
| POST | `/api/events/{id}/participants/{pid}/check-in` | Check a participant in |
| GET | `/api/events/{id}/report` | Attendance report (JSON) |
| GET | `/api/events/{id}/report.csv` | Attendance report (CSV) |

## Project structure

```
src/EventTracker.Api
├── Endpoints/   # route definitions and validation
├── Models/      # records and request/response DTOs
├── Services/    # IEventStore + in-memory implementation
└── Program.cs   # composition root
```

## Roadmap

- [ ] EF Core + SQL Server / SQLite persistence
- [ ] xUnit tests with `WebApplicationFactory`
- [ ] JWT authentication and role-based access
- [ ] Swagger / OpenAPI docs
- [ ] Angular dashboard consuming this API
