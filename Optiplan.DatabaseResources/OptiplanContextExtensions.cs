using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Optiplan.DatabaseResources;

public static class OptiplanContextExtensions
{
    // Add the OptiplanContext class to a collection of dependencies
    // This will register the class as a service that can be utilized throughout the app via dependency injection
    public static IServiceCollection AddOptiplanContext(
        this IServiceCollection services,
        string database = "Optiplan.db"
    )
    {
        string path = ResolveFilePath(database);
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

    public static string ResolveFilePath(string filePath)
    {
        string relativePath = "..";
        if (Environment.CurrentDirectory.EndsWith("net8.0"))
        {
            return Path.Combine(relativePath, relativePath, relativePath, relativePath, filePath);
        }
        return Path.Combine(relativePath, filePath);
    }
}