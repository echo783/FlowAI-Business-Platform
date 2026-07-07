namespace FlowAI.Api.DTOs;

public class CreateWorkOrderRequest
{
    public int ContractId { get; set; }

    public string WorkNo { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
