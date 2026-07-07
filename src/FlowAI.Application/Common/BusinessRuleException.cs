namespace FlowAI.Application.Common;

public sealed class BusinessRuleException(string message) : InvalidOperationException(message);
