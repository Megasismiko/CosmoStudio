using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmoStudio.Model;

[Index("IdGuion", "NumeroVersion", Name = "UQ_GuionVersion_Numero", IsUnique = true)]
public partial class GuionVersion
{
    [Key]
    public long Id { get; set; }

    public long IdGuion { get; set; }

    public int NumeroVersion { get; set; }

    public long? OutlineRecursoId { get; set; }

    public long? ScriptRecursoId { get; set; }

    [StringLength(500)]
    public string? Notas { get; set; }

    public DateTime FechaCreacion { get; set; }

    [InverseProperty(nameof(GuionAudio.IdGuionVersionNavigation))]
    public virtual ICollection<GuionAudio> GuionAudios { get; set; } = [];

    [InverseProperty(nameof(GuionImagen.IdGuionVersionNavigation))]
    public virtual ICollection<GuionImagen> GuionImagenes { get; set; } = [];

    [InverseProperty(nameof(Guion.CurrentVersion))]
    public virtual ICollection<Guion> Guiones { get; set; } = [];

    [ForeignKey("IdGuion")]
    [InverseProperty(nameof(Guion.GuionVersiones))]
    public virtual Guion IdGuionNavigation { get; set; } = null!;

    [ForeignKey("OutlineRecursoId")]
    [InverseProperty(nameof(Recurso.GuionVersioneOutlineRecursos))]
    public virtual Recurso? OutlineRecurso { get; set; }

    [ForeignKey("ScriptRecursoId")]
    [InverseProperty(nameof(Recurso.GuionVersioneScriptRecursos))]
    public virtual Recurso? ScriptRecurso { get; set; }
}
