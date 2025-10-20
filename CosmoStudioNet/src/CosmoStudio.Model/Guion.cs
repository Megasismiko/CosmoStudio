using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Model;

[Index("IdProyecto", Name = "UQ_Guiones_Proyecto", IsUnique = true)]
public partial class Guion
{
    [Key]
    public long Id { get; set; }

    public long IdProyecto { get; set; }

    [StringLength(500)]
    public string RutaOutline { get; set; } = null!;

    [StringLength(500)]
    public string RutaCompleto { get; set; } = null!;

    public int Version { get; set; }

    [ForeignKey("IdProyecto")]
    [InverseProperty(nameof(Proyecto.Guion))]
    public virtual Proyecto IdProyectoNavigation { get; set; } = null!;
}
