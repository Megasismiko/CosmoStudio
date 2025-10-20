using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Model;

[Table("TareasRender")]
[Index("IdProyecto", "Estado", Name = "IX_TareasRender_Proyecto_Estado")]
public partial class TareaRender
{
    [Key]
    public long Id { get; set; }

    public long IdProyecto { get; set; }

    [StringLength(20)]
    public string Estado { get; set; } = null!;

    public int DuracionMinutos { get; set; }

    [StringLength(500)]
    public string? RutaVideoSalida { get; set; }

    [Column("RutasSalidaJSON")]
    public string? RutasSalidaJson { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public DateTime FechaCreacion { get; set; }

    [ForeignKey("IdProyecto")]
    [InverseProperty(nameof(Proyecto.TareasRender))]
    public virtual Proyecto IdProyectoNavigation { get; set; } = null!;

    [InverseProperty(nameof(Log.IdTareaRenderNavigation))]
    public virtual ICollection<Log> Logs { get; set; } = [];
}
