using FlowAI.Api.DTOs;
using FlowAI.Api.Models;
using FlowAI.Api.Options;
using Microsoft.Extensions.Options;
using System.Text;

namespace FlowAI.Api.Services;

public class AgentService
{
    private readonly ContractService _contractService;
    private readonly WorkOrderService _workOrderService;
    private readonly SettlementService _settlementService;
    private readonly DashboardService _dashboardService;
    private readonly AgentOptions _options;
    private readonly RuleBasedAgentTextGenerationService _ruleBasedTextGenerationService;
    private readonly OllamaAgentTextGenerationService _ollamaTextGenerationService;

    public AgentService(
        ContractService contractService,
        WorkOrderService workOrderService,
        SettlementService settlementService,
        DashboardService dashboardService,
        IOptions<AgentOptions> options,
        RuleBasedAgentTextGenerationService ruleBasedTextGenerationService,
        OllamaAgentTextGenerationService ollamaTextGenerationService)
    {
        _contractService = contractService;
        _workOrderService = workOrderService;
        _settlementService = settlementService;
        _dashboardService = dashboardService;
        _options = options.Value;
        _ruleBasedTextGenerationService = ruleBasedTextGenerationService;
        _ollamaTextGenerationService = ollamaTextGenerationService;
    }

    public List<Contract> GetPendingApprovalContracts()
    {
        return _contractService.GetAll()
            .Where(x =>
                x.Status == BusinessStatus.ContractApprovalPending ||
                x.Status == BusinessStatus.ContractApprovalRequested ||
                // TODO: Replace ContractRegistered with only ApprovalRequested/Pending after approval request workflow is added.
                x.Status == BusinessStatus.ContractRegistered)
            .ToList();
    }

    public List<WorkOrder> GetTodayWorkOrders()
    {
        var today = DateTime.Today;

        return _workOrderService.GetAll()
            .Where(x =>
                x.Status == BusinessStatus.WorkInProgress &&
                (!x.StartedAt.HasValue || x.StartedAt.Value.Date == today))
            .ToList();
    }

    public List<WorkOrder> GetDelayedWorkOrders()
    {
        var today = DateTime.Today;

        return _workOrderService.GetAll()
            .Where(x =>
                // TODO: Replace this temporary rule with PlannedEndDate based delay detection when the model has PlannedEndDate.
                (x.Status == BusinessStatus.WorkCreated || x.Status == BusinessStatus.WorkInProgress) &&
                x.CreatedAt.Date < today)
            .ToList();
    }

    public List<Settlement> GetOnHoldSettlements()
    {
        return _settlementService.GetAll()
            .Where(x => x.Status == BusinessStatus.SettlementHeld)
            .ToList();
    }

    public CustomerStatusSummaryResponse GetCustomerStatusSummary(string customerName)
    {
        var contracts = _contractService.GetAll()
            .Where(x => string.Equals(x.CustomerName, customerName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var contractIds = contracts.Select(x => x.Id).ToHashSet();
        var workOrders = _workOrderService.GetAll()
            .Where(x => contractIds.Contains(x.ContractId))
            .ToList();

        var workOrderIds = workOrders.Select(x => x.Id).ToHashSet();
        var settlements = _settlementService.GetAll()
            .Where(x => workOrderIds.Contains(x.WorkOrderId))
            .ToList();

        var response = new CustomerStatusSummaryResponse
        {
            CustomerName = customerName,
            ContractCount = contracts.Count,
            ApprovedContractCount = contracts.Count(x =>
                x.Status == BusinessStatus.ContractApproved ||
                x.Status == BusinessStatus.ContractConvertedToWork),
            WorkOrderCount = workOrders.Count,
            WorkInProgressCount = workOrders.Count(x => x.Status == BusinessStatus.WorkInProgress),
            WorkCompletedCount = workOrders.Count(x => x.Status == BusinessStatus.WorkCompleted),
            SettlementCount = settlements.Count,
            SettlementApprovedCount = settlements.Count(x => x.Status == BusinessStatus.SettlementApproved),
            SettlementHeldCount = settlements.Count(x => x.Status == BusinessStatus.SettlementHeld)
        };

        response.Summary =
            $"{customerName} 고객사의 계약은 {response.ContractCount}건, 작업은 {response.WorkOrderCount}건, 정산은 {response.SettlementCount}건입니다. " +
            $"승인 완료 계약 {response.ApprovedContractCount}건, 완료 작업 {response.WorkCompletedCount}건, " +
            $"승인 완료 정산 {response.SettlementApprovedCount}건, 보류 정산 {response.SettlementHeldCount}건입니다.";

        return response;
    }

    public async Task<AgentQueryResponse> QueryAsync(
        AgentQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var question = request.Question ?? string.Empty;
        var source = GetSource();
        var facts = BuildFacts();
        var prompt = BuildPrompt(question, facts);
        var answer = IsOllamaMode()
            ? await _ollamaTextGenerationService.GenerateAsync(prompt, cancellationToken)
            : await _ruleBasedTextGenerationService.GenerateAsync(
                $"{prompt}{Environment.NewLine}{Environment.NewLine}RULE_BASED_ANSWER:{Environment.NewLine}{BuildAnswer(question, facts)}",
                cancellationToken);

        return new AgentQueryResponse
        {
            Question = question,
            Answer = answer,
            Source = source,
            GeneratedAt = DateTime.Now,
            Facts = facts
        };
    }

    private bool IsOllamaMode()
    {
        return string.Equals(_options.Mode, "Ollama", StringComparison.OrdinalIgnoreCase);
    }

    private string GetSource()
    {
        return IsOllamaMode() ? "ollama" : "rule-based";
    }

    private AgentQueryFactsResponse BuildFacts()
    {
        var summary = _dashboardService.GetSummary();

        return new AgentQueryFactsResponse
        {
            PendingApprovalContracts = GetPendingApprovalContracts().Count,
            TodayWorkOrders = GetTodayWorkOrders().Count,
            DelayedWorkOrders = GetDelayedWorkOrders().Count,
            OnHoldSettlements = GetOnHoldSettlements().Count,
            TotalSettlements = summary.TotalSettlements,
            RequestedSettlements = summary.SettlementRequested,
            ReviewingSettlements = summary.SettlementReviewing,
            ApprovedSettlements = summary.SettlementApproved,
            RejectedSettlements = summary.SettlementRejected
        };
    }

    private string BuildPrompt(string question, AgentQueryFactsResponse facts)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("너는 한국어 업무시스템 상태 요약 담당자다.");
        prompt.AppendLine("반드시 아래 지침을 지킨다.");
        prompt.AppendLine("- 반드시 한국어만 사용한다.");
        prompt.AppendLine("- 영어, 중국어, 베트남어, 혼합 문자를 절대 사용하지 않는다.");
        prompt.AppendLine("- 제공된 데이터만 근거로 답변한다.");
        prompt.AppendLine("- 없는 상태를 추측하지 않는다.");
        prompt.AppendLine("- 상태명은 아래 한글 용어만 사용한다.");
        prompt.AppendLine("- 짧고 실무적인 보고 문장으로 작성한다.");
        prompt.AppendLine("- [확정 데이터]의 숫자를 절대 변경하지 마라.");
        prompt.AppendLine("- 숫자를 계산하거나 추측하지 마라.");
        prompt.AppendLine("- 제공되지 않은 건수는 언급하지 마라.");
        prompt.AppendLine("- 정산 보류는 반드시 \"정산 보류\" 항목의 숫자만 사용한다.");
        prompt.AppendLine("- 전체 정산 건수와 정산 보류 건수를 혼동하지 마라.");
        prompt.AppendLine();
        prompt.AppendLine("상태명 한글 용어 사전:");
        prompt.AppendLine("- Pending Approval = 승인 대기");
        prompt.AppendLine("- Approved = 승인 완료");
        prompt.AppendLine("- Rejected = 반려");
        prompt.AppendLine("- WorkOrder = 작업");
        prompt.AppendLine("- InProgress = 진행 중");
        prompt.AppendLine("- Completed = 완료");
        prompt.AppendLine("- Settlement = 정산");
        prompt.AppendLine("- Requested = 정산 요청");
        prompt.AppendLine("- Reviewing = 검토 중");
        prompt.AppendLine("- Held = 보류");
        prompt.AppendLine("- OnHold = 보류");
        prompt.AppendLine("- Delayed = 지연");
        prompt.AppendLine();
        prompt.AppendLine("출력 형식:");
        prompt.AppendLine("[업무 현황 요약]");
        prompt.AppendLine("현재 승인 대기 계약은 N건이며, 오늘 진행 중인 작업은 N건입니다.");
        prompt.AppendLine("정산 보류 건은 N건으로 확인되어 담당자 검토가 필요합니다.");
        prompt.AppendLine();
        prompt.AppendLine($"사용자 질문: {question}");
        prompt.AppendLine();
        prompt.AppendLine("[확정 데이터]");
        prompt.AppendLine($"- 승인 대기 계약: {facts.PendingApprovalContracts}건");
        prompt.AppendLine($"- 오늘 진행 중 작업: {facts.TodayWorkOrders}건");
        prompt.AppendLine($"- 지연 작업: {facts.DelayedWorkOrders}건");
        prompt.AppendLine($"- 정산 보류: {facts.OnHoldSettlements}건");
        prompt.AppendLine($"- 전체 정산: {facts.TotalSettlements}건");
        prompt.AppendLine($"- 정산 요청: {facts.RequestedSettlements}건");
        prompt.AppendLine($"- 검토 중: {facts.ReviewingSettlements}건");
        prompt.AppendLine($"- 정산 승인 완료: {facts.ApprovedSettlements}건");
        prompt.AppendLine($"- 정산 반려: {facts.RejectedSettlements}건");
        prompt.AppendLine();
        prompt.AppendLine("주의: 답변의 모든 숫자는 [확정 데이터]의 값만 그대로 사용한다.");

        return prompt.ToString();
    }

    private string BuildAnswer(string question, AgentQueryFactsResponse facts)
    {
        if (ContainsAny(question, "정산 보류", "보류"))
        {
            return $"[업무 현황 요약]{Environment.NewLine}현재 정산 보류 건은 {facts.OnHoldSettlements}건입니다.";
        }

        if (ContainsAny(question, "진행 중", "작업"))
        {
            return $"[업무 현황 요약]{Environment.NewLine}현재 진행 중인 작업은 {facts.TodayWorkOrders}건입니다.";
        }

        if (ContainsAny(question, "승인 대기", "계약"))
        {
            return $"[업무 현황 요약]{Environment.NewLine}현재 승인 대기 계약은 {facts.PendingApprovalContracts}건입니다.";
        }

        if (ContainsAny(question, "요약", "현황"))
        {
            return
                $"[업무 현황 요약]{Environment.NewLine}" +
                $"현재 승인 대기 계약은 {facts.PendingApprovalContracts}건이며, 오늘 진행 중인 작업은 {facts.TodayWorkOrders}건입니다.{Environment.NewLine}" +
                $"정산 보류 건은 {facts.OnHoldSettlements}건, 검토 중 정산은 {facts.ReviewingSettlements}건, 반려 정산은 {facts.RejectedSettlements}건입니다.";
        }

        return $"[업무 현황 요약]{Environment.NewLine}현재는 계약, 작업, 정산, 대시보드 현황 조회를 지원합니다.";
    }

    private static bool ContainsAny(string value, params string[] keywords)
    {
        return keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
