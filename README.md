# Event Tracker API

![CI](https://github.com/DarshaNaik08/event-tracker-api/actions/workflows/ci.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![C#](https://img.shields.io/badge/C%23-ASP.NET%20Core-239120)

A small **ASP.NET Core 8 minimal API** for tracking event registrations and attendance. Create events, register participants, check them in on the day, and download an attendance report as JSON or CSV.

It is a clean, from-scratch rebuild of the kind of participation-tracking service I built professionally, written to demonstrate API design, validation and a swappable storage layer.

## Features

- **Events:** create, list, get, update and delete
- **Participants:** register with validation, list, and check in
- **Attendance report:** total registered, checked in, no-shows and attendance rate
- **CSV export:** download the attendance list, with protection against spreadsheet formula injection
- **Validation:** bad requests return a standard `400` problem-details response listing every error
- **Swagger UI:** interactive docs and a built-in way to try every endpoint
- **Swappable storage:** everything goes through an `IEventStore` interface, so the in-memory store can be replaced by a database without touching the endpoints
- **CORS** enabled for an Angular dev server at `http://localhost:4200`
- **CI:** GitHub Actions builds the project on every push and pull request

## Tech stack

| | |
|---|---|
| Framework | ASP.NET Core 8, minimal APIs |
| Language | C# 12 |
| Docs | Swagger UI (Swashbuckle.AspNetCore) |
| Storage | In-memory (`ConcurrentDictionary`), swappable via `IEventStore` |
| CI | GitHub Actions |

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Optional: Visual Studio 2022 or VS Code

### Run it

```bash
git clone https://github.com/DarshaNaik08/event-tracker-api.git
cd event-tracker-api
dotnet run --project src/EventTracker.Api
```

The console prints the address the app is listening on (look for `Now listening on:`). Open it with `/swagger` on the end, for example:

```
http://localhost:5000/swagger
```

The port can differ on your machine. In Visual Studio, open `EventTracker.sln` and press **F5** or **Ctrl+F5**.

> Data is held in memory, so it resets every time the app restarts.

### Try it with Swagger

1. Open `/swagger`.
2. Expand `POST /api/events`, click **Try it out**, and execute the sample body.
3. Copy the `id` from the response and use it in the participant and report endpoints.

### Try it with curl

```bash
# Create an event
curl -X POST http://localhost:5000/api/events \
  -H "Content-Type: application/json" \
  -d '{"title":"Angular Meetup","date":"2026-11-15T10:00:00","location":"Bengaluru"}'

# Register a participant (replace {eventId})
curl -X POST http://localhost:5000/api/events/{eventId}/participants \
  -H "Content-Type: application/json" \
  -d '{"name":"Asha","email":"asha@example.com"}'

# Get the attendance report
curl http://localhost:5000/api/events/{eventId}/report
```

A ready-made set of requests is also in [`src/EventTracker.Api/requests.http`](src/EventTracker.Api/requests.http) for Visual Studio and the VS Code REST Client extension.

## API reference

| Method | Route | Description | Success |
|---|---|---|---|
| GET | `/health` | Health check | 200 |
| GET | `/api/events` | List events, ordered by date | 200 |
| POST | `/api/events` | Create an event | 201 |
| GET | `/api/events/{id}` | Get one event | 200 |
| PUT | `/api/events/{id}` | Update an event | 200 |
| DELETE | `/api/events/{id}` | Delete an event and its participants | 204 |
| GET | `/api/events/{id}/participants` | List participants | 200 |
| POST | `/api/events/{id}/participants` | Register a participant | 201 |
| POST | `/api/events/{id}/participants/{participantId}/check-in` | Check a participant in | 200 |
| GET | `/api/events/{id}/report` | Attendance report (JSON) | 200 |
| GET | `/api/events/{id}/report.csv` | Attendance report (CSV download) | 200 |

Unknown ids return `404`. Invalid bodies return `400`.

### Example: attendance report

`GET /api/events/{id}/report`

```json
{
  "eventId": "46ef1dc3-68f7-4026-baad-60d7a9e97399",
  "title": "Angular Meetup",
  "totalRegistered": 2,
  "checkedIn": 1,
  "noShows": 1,
  "attendanceRate": 50
}
```

### Example: validation error

`POST /api/events` with an empty body returns `400`:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "title": ["Title is required."],
    "date": ["Date is required."],
    "location": ["Location is required."]
  }
}
```

### Example: CSV export

```csv
Name,Email,CheckedIn,CheckedInAtUtc
"Asha","asha@example.com",True,2026-10-05T10:26:15.5386855Z
"Ravi","ravi@example.com",False,
```

Values starting with `=`, `+`, `-` or `@` are prefixed with `'` so spreadsheet apps do not run them as formulas.

## Project structure

```
EventTracker/
├── .github/workflows/ci.yml        # build on push and pull request
├── src/EventTracker.Api/
│   ├── Endpoints/
│   │   └── EventEndpoints.cs       # routes, validation, status codes
│   ├── Models/
│   │   └── Models.cs               # records for entities, requests and responses
│   ├── Services/
│   │   ├── IEventStore.cs          # storage contract
│   │   └── InMemoryEventStore.cs   # in-memory implementation
│   ├── Program.cs                  # service registration and middleware pipeline
│   └── requests.http               # sample requests
└── EventTracker.sln
```

## How it works

```
Request → middleware (error handler, CORS, Swagger)
        → EventEndpoints (route match + validation)
        → IEventStore (InMemoryEventStore)
        → JSON response
```

- **Endpoints** handle HTTP only: routes, validation and status codes.
- **`IEventStore`** is the storage contract. Endpoints depend on the interface, not on a concrete class, and it is registered as a singleton in `Program.cs`.
- **Records** (`EventItem`, `Participant` and the request/response types) are immutable, so updates replace the stored record instead of mutating it.
- **`ConcurrentDictionary`** keeps the in-memory store safe when many requests arrive at once.

## Roadmap

- [x] CRUD for events, participant registration, check-in and reports
- [x] CSV export
- [x] Swagger UI
- [ ] EF Core persistence with SQL Server, with the store registered as scoped
- [ ] xUnit integration tests using `WebApplicationFactory`
- [ ] JWT authentication and role-based access
- [ ] Prevent duplicate registrations (same email per event)
- [ ] Angular dashboard that consumes this API
- [ ] Upgrade to .NET 10 with built-in minimal API validation

## Author

**Darsha Naik**, Full Stack Developer (Angular + Node.js + .NET), Bengaluru.
GitHub: [@DarshaNaik08](https://github.com/DarshaNaik08)
