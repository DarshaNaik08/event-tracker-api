using System.Collections.Concurrent;
using EventTracker.Api.Models;

namespace EventTracker.Api.Services;

public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentDictionary<Guid, EventItem> _events = new();
    private readonly ConcurrentDictionary<Guid, Participant> _participants = new();

    public IReadOnlyList<EventItem> GetEvents() =>
        _events.Values.OrderBy(e => e.Date).ToList();

    public EventItem? GetEvent(Guid id) =>
        _events.TryGetValue(id, out var e) ? e : null;

    public EventItem AddEvent(string title, DateTime date, string location)
    {
        var item = new EventItem(Guid.NewGuid(), title, date, location);
        _events[item.Id] = item;
        return item;
    }

    public bool DeleteEvent(Guid id)
    {
        if (!_events.TryRemove(id, out _)) return false;
        foreach (var p in _participants.Values.Where(p => p.EventId == id))
            _participants.TryRemove(p.Id, out _);
        return true;
    }

    public IReadOnlyList<Participant> GetParticipants(Guid eventId) =>
        _participants.Values.Where(p => p.EventId == eventId).OrderBy(p => p.Name).ToList();

    public Participant? Register(Guid eventId, string name, string email)
    {
        if (!_events.ContainsKey(eventId)) return null;
        var p = new Participant(Guid.NewGuid(), eventId, name, email, false, null);
        _participants[p.Id] = p;
        return p;
    }

    public Participant? CheckIn(Guid eventId, Guid participantId)
    {
        if (!_participants.TryGetValue(participantId, out var p) || p.EventId != eventId)
            return null;
        if (p.CheckedIn) return p;
        var updated = p with { CheckedIn = true, CheckedInAt = DateTime.UtcNow };
        _participants[p.Id] = updated;
        return updated;
    }
}
