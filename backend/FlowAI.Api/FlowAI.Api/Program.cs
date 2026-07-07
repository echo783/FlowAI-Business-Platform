using FlowAI.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<StatusHistoryService>();
builder.Services.AddSingleton<ContractService>();
builder.Services.AddSingleton<WorkOrderService>();
builder.Services.AddSingleton<SettlementService>();
builder.Services.AddSingleton<DashboardService>();
builder.Services.AddSingleton<AgentService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
