using FlowAI.Domain.Entities;
using FlowAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlowAI.Infrastructure.Data;

public sealed class FlowAiDbContext(DbContextOptions<FlowAiDbContext> options) : DbContext(options)
{
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Settlement> Settlements => Set<Settlement>();
    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>().Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        modelBuilder.Entity<ApprovalRequest>().Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        modelBuilder.Entity<WorkOrder>().Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        modelBuilder.Entity<Settlement>().Property(x => x.Status).HasConversion<string>().HasMaxLength(40);

        modelBuilder.Entity<Contract>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Settlement>().Property(x => x.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<Contract>().HasData(
            new Contract { Id = 1, ContractNo = "CT-2026-001", Title = "Long-term logistics operations", CustomerName = "Sejin Logistics", Amount = 12000000, StartDate = new DateOnly(2026, 7, 1), EndDate = new DateOnly(2026, 12, 31), ManagerName = "Kim Mina", Status = ContractStatus.ApprovalRequested },
            new Contract { Id = 2, ContractNo = "CT-2026-002", Title = "Monthly maintenance contract", CustomerName = "Hanjin Industries", Amount = 4800000, StartDate = new DateOnly(2026, 7, 1), EndDate = new DateOnly(2026, 9, 30), ManagerName = "Lee Joon", Status = ContractStatus.ApprovalRequested },
            new Contract { Id = 3, ContractNo = "CT-2026-003", Title = "Equipment inspection contract", CustomerName = "Dongwoo Construction", Amount = 7500000, StartDate = new DateOnly(2026, 6, 15), EndDate = new DateOnly(2026, 8, 31), ManagerName = "Park Hana", Status = ContractStatus.ApprovalRequested },
            new Contract { Id = 4, ContractNo = "CT-2026-004", Title = "Settlement support contract", CustomerName = "Eco Systems", Amount = 5200000, StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 7, 31), ManagerName = "Choi Daeun", Status = ContractStatus.ConvertedToWork },
            new Contract { Id = 5, ContractNo = "CT-2026-005", Title = "Business process improvement", CustomerName = "Mirae Accounting Service", Amount = 9800000, StartDate = new DateOnly(2026, 5, 1), EndDate = new DateOnly(2026, 8, 15), ManagerName = "Jung Taeho", Status = ContractStatus.ConvertedToWork });

        modelBuilder.Entity<WorkOrder>().HasData(
            new WorkOrder { Id = 1, ContractId = 4, WorkNo = "WO-20260701-0001", Title = "Settlement support execution", AssignedTo = "Choi Daeun", Status = WorkOrderStatus.InProgress, PlannedStartDate = new DateOnly(2026, 7, 1), PlannedEndDate = new DateOnly(2026, 7, 31), ActualStartDate = new DateOnly(2026, 7, 1) },
            new WorkOrder { Id = 2, ContractId = 5, WorkNo = "WO-20260701-0002", Title = "Process analysis", AssignedTo = "Jung Taeho", Status = WorkOrderStatus.InProgress, PlannedStartDate = new DateOnly(2026, 7, 1), PlannedEndDate = new DateOnly(2026, 7, 20), ActualStartDate = new DateOnly(2026, 7, 1) },
            new WorkOrder { Id = 3, ContractId = 4, WorkNo = "WO-20260702-0003", Title = "Data reconciliation", AssignedTo = "Kim Yuna", Status = WorkOrderStatus.InProgress, PlannedStartDate = new DateOnly(2026, 7, 2), PlannedEndDate = new DateOnly(2026, 7, 7), ActualStartDate = new DateOnly(2026, 7, 2) },
            new WorkOrder { Id = 4, ContractId = 5, WorkNo = "WO-20260620-0004", Title = "Legacy workflow review", AssignedTo = "Han Sora", Status = WorkOrderStatus.InProgress, PlannedStartDate = new DateOnly(2026, 6, 20), PlannedEndDate = new DateOnly(2026, 7, 5), ActualStartDate = new DateOnly(2026, 6, 20) },
            new WorkOrder { Id = 5, ContractId = 4, WorkNo = "WO-20260625-0005", Title = "Monthly close assistance", AssignedTo = "Oh Minseok", Status = WorkOrderStatus.InProgress, PlannedStartDate = new DateOnly(2026, 6, 25), PlannedEndDate = new DateOnly(2026, 7, 10), ActualStartDate = new DateOnly(2026, 6, 25) },
            new WorkOrder { Id = 6, ContractId = 5, WorkNo = "WO-20260601-0006", Title = "Initial process mapping", AssignedTo = "Jung Taeho", Status = WorkOrderStatus.Completed, PlannedStartDate = new DateOnly(2026, 6, 1), PlannedEndDate = new DateOnly(2026, 6, 15), ActualStartDate = new DateOnly(2026, 6, 1), ActualEndDate = new DateOnly(2026, 6, 14) },
            new WorkOrder { Id = 7, ContractId = 4, WorkNo = "WO-20260610-0007", Title = "Invoice validation", AssignedTo = "Choi Daeun", Status = WorkOrderStatus.Completed, PlannedStartDate = new DateOnly(2026, 6, 10), PlannedEndDate = new DateOnly(2026, 6, 20), ActualStartDate = new DateOnly(2026, 6, 10), ActualEndDate = new DateOnly(2026, 6, 20) });

        modelBuilder.Entity<Settlement>().HasData(
            new Settlement { Id = 1, ContractId = 5, WorkOrderId = 6, SettlementNo = "ST-20260614-0006", Amount = 2500000, Status = SettlementStatus.Requested },
            new Settlement { Id = 2, ContractId = 4, WorkOrderId = 7, SettlementNo = "ST-20260620-0007", Amount = 1800000, Status = SettlementStatus.Requested },
            new Settlement { Id = 3, ContractId = 4, WorkOrderId = 7, SettlementNo = "ST-20260621-0007", Amount = 900000, Status = SettlementStatus.Requested },
            new Settlement { Id = 4, ContractId = 5, WorkOrderId = 6, SettlementNo = "ST-20260622-0006", Amount = 1200000, Status = SettlementStatus.Requested },
            new Settlement { Id = 5, ContractId = 4, WorkOrderId = 7, SettlementNo = "ST-20260623-0007", Amount = 700000, Status = SettlementStatus.OnHold, HoldReason = "Amount mismatch" },
            new Settlement { Id = 6, ContractId = 5, WorkOrderId = 6, SettlementNo = "ST-20260624-0006", Amount = 650000, Status = SettlementStatus.OnHold, HoldReason = "Work completion evidence missing" });
    }
}
