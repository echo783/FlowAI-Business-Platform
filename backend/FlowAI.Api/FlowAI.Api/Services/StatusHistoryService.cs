using FlowAI.Api.Models;

namespace FlowAI.Api.Services;

public class StatusHistoryService
{
    private readonly List<StatusHistory> _histories = new();

    private int _historySeq = 1;

    public StatusHistory AddHistory(
        string entityType,
        int entityId,
        BusinessStatus fromStatus,
        BusinessStatus toStatus,
        string? memo,
        string changedBy = "system")
    {
        var history = new StatusHistory
        {
            Id = _historySeq++,
            EntityType = entityType,
            EntityId = entityId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedBy = changedBy,
            Memo = memo,
            ChangedAt = DateTime.Now
        };

        _histories.Add(history);

        return history;
    }

    public List<StatusHistory> GetAll()
    {
        return _histories
            .OrderByDescending(x => x.ChangedAt)
            .ToList();
    }
}
