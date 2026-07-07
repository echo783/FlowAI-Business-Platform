using FlowAI.Api.Middleware;
using FlowAI.Api.Options;
using FlowAI.Api.Services;
using Microsoft.OpenApi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKey"));
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FlowAI Business Platform API",
        Version = "v1",
        Description = "ERP workflow portfolio API prepared for Power Platform Custom Connector and Copilot Studio mock integration."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Enter the FlowAI API key. Example: local-dev-key",
        Name = "X-FlowAI-Api-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKey"
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("ApiKey", null, "ApiKey"),
            new List<string>()
        }
    });
});

builder.Services.AddSingleton<StatusHistoryService>();
builder.Services.AddSingleton<ContractService>();
builder.Services.AddSingleton<WorkOrderService>();
builder.Services.AddSingleton<SettlementService>();
builder.Services.AddSingleton<DashboardService>();
builder.Services.AddSingleton<AgentService>();
builder.Services.AddSingleton<ApprovalRequestService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowAI Business Platform API v1");
    });
}


app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
