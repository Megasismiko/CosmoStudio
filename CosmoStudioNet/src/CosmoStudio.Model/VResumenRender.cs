using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Model;

[Keyless]
public partial class VResumenRender
{
    public long IdTarea { get; set; }

    public long IdProyecto { get; set; }

    [StringLength(200)]
    public string Titulo { get; set; } = null!;

    [StringLength(400)]
    public string Tema { get; set; } = null!;

    [StringLength(20)]
    public string Estado { get; set; } = null!;

    public int DuracionMinutos { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    [StringLength(500)]
    public string? RutaVideoSalida { get; set; }
}
