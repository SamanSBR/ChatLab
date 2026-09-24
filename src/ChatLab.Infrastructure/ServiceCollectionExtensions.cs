using ChatLab.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatLab.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChatLabSqlite(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ChatLabDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IResearchSessionRepository, SqliteResearchSessionRepository>();
        return services;
    }
}
