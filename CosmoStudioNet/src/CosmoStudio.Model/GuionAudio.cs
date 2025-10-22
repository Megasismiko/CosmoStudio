using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Model;

[Index("IdGuionVersion", Name = "IX_GA_Version")]
[Index("IdGuionVersion", "Orden", Name = "UQ_GA_Version_Orden", IsUnique = true)]
public partial class GuionAudio
{
    [Key]
    public long Id { get; set; }

    public long IdGuionVersion { get; set; }

    public long IdAudioRecurso { get; set; }

    public int Orden { get; set; }

    public int? StartMs { get; set; }

    public int? EndMs { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? VolumenDb { get; set; }

    [Column(TypeName = "decimal(4, 2)")]
    public decimal? Panoramica { get; set; }

    [Column("MetaJSON")]
    public string? MetaJson { get; set; }

    public DateTime FechaCreacion { get; set; }

    [ForeignKey("IdAudioRecurso")]
    [InverseProperty(nameof(Recurso.GuionAudios))]
    public virtual Recurso IdAudioRecursoNavigation { get; set; } = null!;

    [ForeignKey("IdGuionVersion")]
    [InverseProperty(nameof(GuionVersion.GuionAudios))]
    public virtual GuionVersion IdGuionVersionNavigation { get; set; } = null!;
}
