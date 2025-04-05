using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Optiplan.DatabaseResources;

public static class OptiplanContextExtensions
{
    // Add the OptiplanContext class to a collection of dependencies
    // This will register the class as a service that can be utilized throughout the app via dependency injection
    public static IServiceCollection AddOptiplanContext(
        this IServiceCollection services,
        string relativePath = "..",
        string database = "Optiplan.db"
    )
    {
        string path = Path.Combine(relativePath, database);
        path = Path.GetFullPath(path);
        OptiplanContextLogger.WriteLine($"Database path: {path}");
        if (!File.Exists(path))
        {
            // Important to throw, otherwise database provider will create empty db file
            throw new FileNotFoundException(message: $"{path} not found.", fileName: path);
        }
        
        services.AddDbContext<OptiplanContext>(optionsBuilder => {
            optionsBuilder.UseSqlite($"Data Source={path}");
            optionsBuilder.LogTo(OptiplanContextLogger.WriteLine, 
                [Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuting]
            );
        },
        contextLifetime: ServiceLifetime.Transient,
        optionsLifetime: ServiceLifetime.Transient            
        );

        return services;
    }
}