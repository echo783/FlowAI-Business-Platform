using FlowAI.Domain.Entities;

namespace FlowAI.Application.Common;

public interface IFlowAiRepository
{
    IQueryable<Contract> Contracts { get; }
    IQueryable<ApprovalRequest> ApprovalRequests { get; }
    IQueryable<WorkOrder> WorkOrders { get; }
    IQueryable<Settlement> Settlements { get; }
    IQueryable<StatusHistory> StatusHistories { get; }
    void Add<T>(T entity) where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
