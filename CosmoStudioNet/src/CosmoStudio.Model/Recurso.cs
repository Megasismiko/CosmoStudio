using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmoStudio.Model;

[Index("IdGuion", "Tipo", "FechaCreacion", Name = "IX_Recursos_Guion_Tipo_Fecha", IsDescending = new[] { false, false, true })]
[Index("IdProyecto", "Tipo", "FechaCreacion", Name = "IX_Recursos_Proyecto_Tipo_Fecha", IsDescending = new[] { false, false, true })]
public partial class Recurso
{
    [Key]
    public long Id { get; set; }

    public long IdProyecto { get; set; }

    public long? IdGuion { get; set; }

    [StringLength(20)]
    public string Tipo { get; set; } = null!;

    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [StringLength(500)]
    public string StoragePath { get; set; } = null!;

    [StringLength(100)]
    public string? Mime { get; set; }

    public long? SizeBytes { get; set; }

    [MaxLength(32)]
    public byte[]? Checksum { get; set; }

    [Column("MetaJSON")]
    public string? MetaJson { get; set; }

    public string? TextoIndexado { get; set; }

    public DateTime FechaCreacion { get; set; }

    [InverseProperty(nameof(GuionAudio.IdAudioRecursoNavigation))]
    public virtual ICollection<GuionAudio> GuionAudios { get; set; } = [];

    [InverseProperty(nameof(GuionImagen.IdImagenRecursoNavigation))]    
    public virtual ICollection<GuionImagen> GuionImagenes { get; set; } = [];
        
    [InverseProperty(nameof(GuionVersion.OutlineRecurso))]
    public virtual ICollection<GuionVersion> GuionVersioneOutlineRecursos { get; set; } = [];

    [InverseProperty(nameof(GuionVersion.ScriptRecurso))]
    public virtual ICollection<GuionVersion> GuionVersioneScriptRecursos { get; set; } = [];

    [ForeignKey("IdGuion")]
    [InverseProperty(nameof(Guion.Recursos))]
    public virtual Guion? IdGuionNavigation { get; set; }

    [ForeignKey("IdProyecto")]
    [InverseProperty(nameof(Proyecto.Recursos))]
    public virtual Proyecto IdProyectoNavigation { get; set; } = null!;
}
