using EventTracker.Api.Models;

namespace EventTracker.Api.Services;

/// <summary>
/// Storage abstraction. The in-memory implementation can be swapped for
/// EF Core / SQL Server without touching the endpoints.
/// </summary>
public interface IEventStore
{
    IReadOnlyList<EventItem> GetEvents();
    EventItem? GetEvent(Guid id);
    EventItem AddEvent(string title, DateTime date, string location);

    EventItem? UpdateEvent(Guid id, string title, DateTime date, string location);

    bool DeleteEvent(Guid id);

    IReadOnlyList<Participant> GetParticipants(Guid eventId);
    Participant? Register(Guid eventId, string name, string email);
    Participant? CheckIn(Guid eventId, Guid participantId);
}
