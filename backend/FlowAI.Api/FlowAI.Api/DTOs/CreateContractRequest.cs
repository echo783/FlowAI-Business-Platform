namespace FlowAI.Api.DTOs;

public class CreateContractRequest
{
    public string ContractNo { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}