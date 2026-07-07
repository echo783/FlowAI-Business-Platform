using FlowAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowAI.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task EnsureSeededAsync(FlowAiDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsInMemory())
        {
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            return;
        }

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
