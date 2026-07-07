using FlowAI.Domain.Entities;

namespace FlowAI.Application.Common;

public sealed class StatusHistoryWriter(IFlowAiRepository repository)
{
    public void Add<TStatus>(string entityType, int entityId, TStatus fromStatus, TStatus toStatus, string changedBy, string? reason = null)
        where TStatus : struct, Enum
    {
        repository.Add(new StatusHistory
        {
            EntityType = entityType,
            EntityId = entityId,
            FromStatus = fromStatus.ToString(),
            ToStatus = toStatus.ToString(),
            ChangedBy = changedBy,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = reason
        });
    }
}
