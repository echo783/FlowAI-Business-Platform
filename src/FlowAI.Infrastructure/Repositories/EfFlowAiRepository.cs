using FlowAI.Application.Common;
using FlowAI.Domain.Entities;
using FlowAI.Infrastructure.Data;

namespace FlowAI.Infrastructure.Repositories;

public sealed class EfFlowAiRepository(FlowAiDbContext dbContext) : IFlowAiRepository
{
    public IQueryable<Contract> Contracts => dbContext.Contracts;
    public IQueryable<ApprovalRequest> ApprovalRequests => dbContext.ApprovalRequests;
    public IQueryable<WorkOrder> WorkOrders => dbContext.WorkOrders;
    public IQueryable<Settlement> Settlements => dbContext.Settlements;
    public IQueryable<StatusHistory> StatusHistories => dbContext.StatusHistories;

    public void Add<T>(T entity) where T : class => dbContext.Set<T>().Add(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
