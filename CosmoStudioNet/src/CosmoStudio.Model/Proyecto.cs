using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmoStudio.Model;

[Index("FechaCreacion", Name = "IX_Proyectos_FechaCreacion", AllDescending = true)]
public partial class Proyecto
{
    [Key]
    public long Id { get; set; }

    [StringLength(200)]
    public string Titulo { get; set; } = null!;

    [StringLength(400)]
    public string Tema { get; set; } = null!;

    [StringLength(20)]
    public string Origen { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    [InverseProperty(nameof(Guion.IdProyectoNavigation))]
    public virtual Guion? Guion { get; set; }

    [InverseProperty(nameof(Recurso.IdProyectoNavigation))]
    public virtual ICollection<Recurso> Recursos { get; set; } = [];

    [InverseProperty(nameof(TareaRender.IdProyectoNavigation))]
    public virtual ICollection<TareaRender> TareasRender { get; set; } = [];
}
