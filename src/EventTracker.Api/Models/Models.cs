namespace EventTracker.Api.Models;

public record EventItem(Guid Id, string Title, DateTime Date, string Location);

public record Participant(Guid Id, Guid EventId, string Name, string Email, bool CheckedIn, DateTime? CheckedInAt);

public record CreateEventRequest(string? Title, DateTime? Date, string? Location);

public record RegisterParticipantRequest(string? Name, string? Email);

public record EventReport(
    Guid EventId,
    string Title,
    int TotalRegistered,
    int CheckedIn,
    int NoShows,
    double AttendanceRate);
