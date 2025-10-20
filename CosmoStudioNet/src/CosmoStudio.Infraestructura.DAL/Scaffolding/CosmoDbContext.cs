
using Microsoft.EntityFrameworkCore;
using CosmoStudio.Model;

namespace CosmoStudio.Infraestructura.DAL.Scaffolding;

public partial class CosmoDbContext : DbContext
{
    public CosmoDbContext()
    {
    }

    public CosmoDbContext(DbContextOptions<CosmoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Guion> Guiones { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Proyecto> Proyectos { get; set; }

    public virtual DbSet<Recurso> Recursos { get; set; }

    public virtual DbSet<TareaRender> TareasRenders { get; set; }

    public virtual DbSet<VResumenRender> VResumenRenders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Guion>(entity =>
        {
            entity.Property(e => e.Version).HasDefaultValue(1);

            entity.HasOne(g => g.IdProyectoNavigation)
            .WithOne(p => p.Guion)
            .HasForeignKey<Guion>(g => g.IdProyecto)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Guiones_Proyectos");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.IdTareaRenderNavigation).WithMany(p => p.Logs).HasConstraintName("FK_Logs_TareasRender");
        });

        modelBuilder.Entity<Proyecto>(entity =>
        {
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Origen).HasDefaultValue("Manual");
        });

        modelBuilder.Entity<Recurso>(entity =>
        {
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.IdProyectoNavigation).WithMany(p => p.Recursos).HasConstraintName("FK_Recursos_Proyectos");
        });

        modelBuilder.Entity<TareaRender>(entity =>
        {
            entity.Property(e => e.DuracionMinutos).HasDefaultValue(60);
            entity.Property(e => e.Estado).HasDefaultValue("EnCola");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.IdProyectoNavigation).WithMany(p => p.TareasRender).HasConstraintName("FK_TareasRender_Proyectos");
        });

        modelBuilder.Entity<VResumenRender>(entity =>
        {
            entity.ToView("v_ResumenRender");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
