using FlowAI.Application.Common;
using FlowAI.Infrastructure.Data;
using FlowAI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFlowAiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue("Database:UseInMemory", false);
        if (useInMemory)
        {
            services.AddDbContext<FlowAiDbContext>(options => options.UseInMemoryDatabase("FlowAI-Demo"));
        }
        else
        {
            services.AddDbContext<FlowAiDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }

        services.AddScoped<IFlowAiRepository, EfFlowAiRepository>();
        return services;
    }
}
