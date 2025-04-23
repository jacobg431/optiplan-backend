using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

public partial class OptiplanContext : DbContext
{
    public OptiplanContext()
    {
    }

    public OptiplanContext(DbContextOptions<OptiplanContext> options) : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Dependency> Dependencies { get; set; }

    public virtual DbSet<WorkOrder> WorkOrders { get; set; }

    public virtual DbSet<WorkOrderToDependency> WorkOrderToDependencies { get; set; }

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
