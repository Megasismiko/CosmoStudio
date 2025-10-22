using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Model;

[Index("IdGuionVersion", Name = "IX_GI_Version")]
[Index("IdGuionVersion", "Orden", Name = "UQ_GI_Version_Orden", IsUnique = true)]
public partial class GuionImagen
{
    [Key]
    public long Id { get; set; }

    public long IdGuionVersion { get; set; }

    public long IdImagenRecurso { get; set; }

    public int Orden { get; set; }

    public int? StartMs { get; set; }

    public int? EndMs { get; set; }

    [StringLength(500)]
    public string? TextoSuperpuesto { get; set; }

    [Column("MetaJSON")]
    public string? MetaJson { get; set; }

    public DateTime FechaCreacion { get; set; }

    [ForeignKey("IdGuionVersion")]
    [InverseProperty(nameof(GuionVersion.GuionImagenes))]
    public virtual GuionVersion IdGuionVersionNavigation { get; set; } = null!;

    [ForeignKey("IdImagenRecurso")]
    [InverseProperty(nameof(Recurso.GuionImagenes))]
    public virtual Recurso IdImagenRecursoNavigation { get; set; } = null!;
}
