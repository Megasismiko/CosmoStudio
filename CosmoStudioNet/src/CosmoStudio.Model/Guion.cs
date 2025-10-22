using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmoStudio.Model;

[Index("IdProyecto", Name = "UQ_Guiones_Proyecto", IsUnique = true)]
public partial class Guion
{
    [Key]
    public long Id { get; set; }

    public long IdProyecto { get; set; }

    public long? CurrentVersionId { get; set; }

    public DateTime FechaCreacion { get; set; }

    [ForeignKey("CurrentVersionId")]
    [InverseProperty(nameof(GuionVersion.Guiones))]
    public virtual GuionVersion? CurrentVersion { get; set; }

    [InverseProperty(nameof(GuionVersion.IdGuionNavigation))]
    public virtual ICollection<GuionVersion> GuionVersiones { get; set; } = [];

    [ForeignKey("IdProyecto")]
    [InverseProperty(nameof(Proyecto.Guion))]
    public virtual Proyecto IdProyectoNavigation { get; set; } = null!;

    [InverseProperty(nameof(Recurso.IdGuionNavigation))]
    public virtual ICollection<Recurso> Recursos { get; set; } = [];
}
