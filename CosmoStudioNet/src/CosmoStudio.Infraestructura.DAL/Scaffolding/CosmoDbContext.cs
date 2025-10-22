using Microsoft.EntityFrameworkCore;
using CosmoStudio.Model;

namespace CosmoStudio.Infraestructura.DAL.Scaffolding;

public class CosmoDbContext : DbContext
{
    public CosmoDbContext(DbContextOptions<CosmoDbContext> options) : base(options) { }

    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<Guion> Guiones => Set<Guion>();
    public DbSet<GuionVersion> GuionVersiones => Set<GuionVersion>();
    public DbSet<Recurso> Recursos => Set<Recurso>();
    public DbSet<GuionImagen> GuionImagenes => Set<GuionImagen>();
    public DbSet<GuionAudio> GuionAudios => Set<GuionAudio>();
    public DbSet<TareaRender> TareasRender => Set<TareaRender>();
    public DbSet<Log> Logs => Set<Log>();
    public DbSet<VResumenRender> VResumenRender => Set<VResumenRender>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");

        // === Proyectos ===
        modelBuilder.Entity<Proyecto>(entity =>
        {
            entity.ToTable("Proyectos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Tema).IsRequired().HasMaxLength(400);
            entity.Property(e => e.Origen).IsRequired().HasMaxLength(20);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");

            // 1:1 Proyecto → Guion (CASCADE)
            entity.HasOne(p => p.Guion)
                  .WithOne(g => g.IdProyectoNavigation)
                  .HasForeignKey<Guion>(g => g.IdProyecto)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Guiones_Proyectos");
        });

        // === Guiones ===
        modelBuilder.Entity<Guion>(entity =>
        {
            entity.ToTable("Guiones");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IdProyecto).IsUnique();
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");

            // FK explícita a Proyecto (ya configurada arriba, lo dejamos también aquí por claridad)
            entity.HasOne(g => g.IdProyectoNavigation)
                  .WithOne(p => p.Guion)
                  .HasForeignKey<Guion>(g => g.IdProyecto)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Guiones_Proyectos");

            // Guion (0..1) → (1) GuionVersion (CurrentVersion)  [FK: CurrentVersionId]  (NO CASCADE)
            entity.HasOne(g => g.CurrentVersion)
                  .WithMany(v => v.Guiones)         // si NO tienes colección en GuionVersion, usa: .WithMany()
                  .HasForeignKey(g => g.CurrentVersionId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Guiones_CurrentVersion");
        });

        // === Recursos ===
        modelBuilder.Entity<Recurso>(entity =>
        {
            entity.ToTable("Recursos");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(20);
            entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Mime).HasMaxLength(100);
            entity.Property(e => e.SizeBytes);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");

            // Recurso → Proyecto (CASCADE)
            entity.HasOne(r => r.IdProyectoNavigation)
                  .WithMany(p => p.Recursos) // si no tienes colección en Proyecto, puedes usar .WithMany()
                  .HasForeignKey(r => r.IdProyecto)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Recursos_Proyectos");

            // Recurso → Guion (NO CASCADE / Restrict) para evitar multi-cascade
            entity.HasOne(r => r.IdGuionNavigation)
                  .WithMany(g => g.Recursos) // si no hay colección en Guion, usa .WithMany()
                  .HasForeignKey(r => r.IdGuion)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Recursos_Guiones");
        });

        // === GuionVersiones ===
        modelBuilder.Entity<GuionVersion>(entity =>
        {
            entity.ToTable("GuionVersiones");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.IdGuion, e.NumeroVersion }).IsUnique();
            entity.Property(e => e.NumeroVersion).IsRequired();
            entity.Property(e => e.Notas).HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");

            // GuionVersion (N) → (1) Guion  [FK: IdGuion]  (CASCADE)
            entity.HasOne(v => v.IdGuionNavigation)
                  .WithMany(g => g.GuionVersiones)
                  .HasForeignKey(v => v.IdGuion)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_GV_Guion");

            // GuionVersion → OutlineRecurso (NO CASCADE / Restrict)
            entity.HasOne(v => v.OutlineRecurso)
                  .WithMany(r => r.GuionVersioneOutlineRecursos) // si no tienes colección en Recurso, usa .WithMany()
                  .HasForeignKey(v => v.OutlineRecursoId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_GV_Outline");

            // GuionVersion → ScriptRecurso (NO CASCADE / Restrict)
            entity.HasOne(v => v.ScriptRecurso)
                  .WithMany(r => r.GuionVersioneScriptRecursos) // si no tienes colección en Recurso, usa .WithMany()
                  .HasForeignKey(v => v.ScriptRecursoId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_GV_Script");
        });

        // === GuionImagenes ===
        modelBuilder.Entity<GuionImagen>(entity =>
        {
            entity.ToTable("GuionImagenes");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.IdGuionVersion, e.Orden }).IsUnique();

            entity.Property(e => e.TextoSuperpuesto).HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");

            // GuionImagen → GuionVersion (CASCADE)
            entity.HasOne(i => i.IdGuionVersionNavigation)
                  .WithMany(v => v.GuionImagenes)   // si no la tienes, usa .WithMany()
                  .HasForeignKey(i => i.IdGuionVersion)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_GI_GuionVersion");

            // GuionImagen → Recurso (Imagen) (NO CASCADE / Restrict)
            entity.HasOne(i => i.IdImagenRecursoNavigation)
                  .WithMany(r => r.GuionImagenes)   // si no la tienes, usa .WithMany()
                  .HasForeignKey(i => i.IdImagenRecurso)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_GI_Imagen");
        });

        // === GuionAudios ===
        modelBuilder.Entity<GuionAudio>(entity =>
        {
            entity.ToTable("GuionAudios");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.IdGuionVersion, e.Orden }).IsUnique();

            entity.Property(e => e.VolumenDb).HasColumnType("decimal(6,2)");
            entity.Property(e => e.Panoramica).HasColumnType("decimal(4,2)");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");

            // GuionAudio → GuionVersion (CASCADE)
            entity.HasOne(a => a.IdGuionVersionNavigation)
                  .WithMany(v => v.GuionAudios)     // si no la tienes, usa .WithMany()
                  .HasForeignKey(a => a.IdGuionVersion)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_GA_GuionVersion");

            // GuionAudio → Recurso (Audio/Voz) (NO CASCADE / Restrict)
            entity.HasOne(a => a.IdAudioRecursoNavigation)
                  .WithMany(r => r.GuionAudios)     // si no la tienes, usa .WithMany()
                  .HasForeignKey(a => a.IdAudioRecurso)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_GA_Audio");
        });

        // === TareasRender ===
        modelBuilder.Entity<TareaRender>(entity =>
        {
            entity.ToTable("TareasRender");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Estado).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DuracionMinutos);
            entity.Property(e => e.RutaVideoSalida).HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime2");
            entity.Property(e => e.FechaFin).HasColumnType("datetime2");

            // TareaRender → Proyecto (CASCADE)
            entity.HasOne(t => t.IdProyectoNavigation)
                  .WithMany(p => p.TareasRender)
                  .HasForeignKey(t => t.IdProyecto)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_TareasRender_Proyectos");
        });

        // === Logs ===
        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("Logs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nivel).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Mensaje).IsRequired();
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");

            // Log → TareaRender (CASCADE)
            entity.HasOne(l => l.IdTareaRenderNavigation)
                  .WithMany(t => t.Logs)
                  .HasForeignKey(l => l.IdTareaRender)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Logs_TareasRender");
        });

        // === Vista (Keyless) ===
        modelBuilder.Entity<VResumenRender>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("v_ResumenRender");
        });
    }
}
