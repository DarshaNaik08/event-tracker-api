using System.Globalization;
using System.Net.Mail;
using System.Text;
using EventTracker.Api.Models;
using EventTracker.Api.Services;

namespace EventTracker.Api.Endpoints;

public static class EventEndpoints
{
    public static void MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events");

        group.MapGet("/", (IEventStore store) => Results.Ok(store.GetEvents()));

        group.MapGet("/{id:guid}", (Guid id, IEventStore store) =>
            store.GetEvent(id) is { } e ? Results.Ok(e) : Results.NotFound());

        group.MapPost("/", (CreateEventRequest req, IEventStore store) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(req.Title)) errors["title"] = new[] { "Title is required." };
            if (req.Date is null) errors["date"] = new[] { "Date is required." };
            if (string.IsNullOrWhiteSpace(req.Location)) errors["location"] = new[] { "Location is required." };
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var created = store.AddEvent(req.Title!.Trim(), req.Date!.Value, req.Location!.Trim());
            return Results.Created($"/api/events/{created.Id}", created);
        });

        group.MapDelete("/{id:guid}", (Guid id, IEventStore store) =>
            store.DeleteEvent(id) ? Results.NoContent() : Results.NotFound());

        group.MapGet("/{id:guid}/participants", (Guid id, IEventStore store) =>
            store.GetEvent(id) is null ? Results.NotFound() : Results.Ok(store.GetParticipants(id)));

        group.MapPost("/{id:guid}/participants", (Guid id, RegisterParticipantRequest req, IEventStore store) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(req.Name)) errors["name"] = new[] { "Name is required." };
            if (!IsValidEmail(req.Email)) errors["email"] = new[] { "A valid email is required." };
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var p = store.Register(id, req.Name!.Trim(), req.Email!.Trim());
            return p is null
                ? Results.NotFound()
                : Results.Created($"/api/events/{id}/participants/{p.Id}", p);
        });

        group.MapPost("/{id:guid}/participants/{participantId:guid}/check-in",
            (Guid id, Guid participantId, IEventStore store) =>
                store.CheckIn(id, participantId) is { } p ? Results.Ok(p) : Results.NotFound());

        group.MapGet("/{id:guid}/report", (Guid id, IEventStore store) =>
            BuildReport(id, store) is { } r ? Results.Ok(r) : Results.NotFound());

        group.MapGet("/{id:guid}/report.csv", (Guid id, IEventStore store) =>
        {
            if (BuildReport(id, store) is null) return Results.NotFound();

            var sb = new StringBuilder("Name,Email,CheckedIn,CheckedInAtUtc\n");
            foreach (var p in store.GetParticipants(id))
                sb.AppendLine(string.Join(',',
                    Csv(p.Name), Csv(p.Email), p.CheckedIn,
                    p.CheckedInAt?.ToString("o", CultureInfo.InvariantCulture) ?? ""));

            return Results.File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "attendance.csv");
        });
    }

    private static EventReport? BuildReport(Guid id, IEventStore store)
    {
        if (store.GetEvent(id) is not { } e) return null;
        var participants = store.GetParticipants(id);
        var total = participants.Count;
        var checkedIn = participants.Count(p => p.CheckedIn);
        var rate = total == 0 ? 0 : Math.Round(100.0 * checkedIn / total, 1);
        return new EventReport(e.Id, e.Title, total, checkedIn, total - checkedIn, rate);
    }

    private static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email)
        && MailAddress.TryCreate(email, out var addr)
        && addr.Address == email.Trim();

    // Quote CSV fields and neutralise spreadsheet formula injection.
    private static string Csv(string value)
    {
        if (value.Length > 0 && "=+-@".Contains(value[0])) value = "'" + value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
