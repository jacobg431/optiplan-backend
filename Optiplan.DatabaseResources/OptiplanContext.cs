using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

public partial class OptiplanContext : DbContext
{
    public OptiplanContext()
    {
    }

    public OptiplanContext(DbContextOptions<OptiplanContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Dependency> Dependencies { get; set; }

    public virtual DbSet<WorkOrder> WorkOrders { get; set; }

    public virtual DbSet<WorkOrderToDependency> WorkOrderToDependencies { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string database = "Optiplan.db";
        string relativePath = "..";
        string currentDir = Environment.CurrentDirectory;
        string path = string.Empty;

        if (currentDir.EndsWith("net8.0"))
        {
            path = Path.Combine(relativePath, relativePath, relativePath, relativePath, database);
        }
        else
        {
            path = Path.Combine(relativePath, database);
        }

        path = Path.GetFullPath(path);
        OptiplanContextLogger.WriteLine($"Database path: {path}");
        if (!File.Exists(path))
        {
            // Important to throw, otherwise database provider will create empty db file
            throw new FileNotFoundException(message: $"{path} not found.", fileName: path);
        }
        optionsBuilder.UseSqlite($"Data Source={path}");
        optionsBuilder.LogTo(OptiplanContextLogger.WriteLine, 
            [Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuting]
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Dependency>(entity =>
        {
            entity.HasOne(d => d.Category).WithMany(p => p.Dependencies).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<WorkOrderToDependency>(entity =>
        {
            entity.HasOne(d => d.Dependency).WithMany(p => p.WorkOrderToDependencies).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.WorkOrderToDependencies).OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
