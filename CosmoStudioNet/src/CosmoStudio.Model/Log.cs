using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Model;

[Index("IdTareaRender", "FechaCreacion", Name = "IX_Logs_Tarea_Fecha")]
public partial class Log
{
    [Key]
    public long Id { get; set; }

    public long IdTareaRender { get; set; }

    [StringLength(10)]
    public string Nivel { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    [ForeignKey("IdTareaRender")]
    [InverseProperty(nameof(TareaRender.Logs))]
    public virtual TareaRender IdTareaRenderNavigation { get; set; } = null!;
}
