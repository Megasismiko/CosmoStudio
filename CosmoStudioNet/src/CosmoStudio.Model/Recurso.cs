using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmoStudio.Model
{
    public enum TipoRecurso
    {
        Otro = 0,
        Voz = 1,
        Musica = 2,
        Imagen = 3
    }

    [Index("IdProyecto", "Tipo", Name = "IX_Recursos_Proyecto_Tipo")]
    public partial class Recurso
    {
        [Key]
        public long Id { get; set; }

        public long IdProyecto { get; set; }

        [StringLength(20)]
        public string Tipo { get; set; } = null!;

        [NotMapped]
        public TipoRecurso TipoEnum
        {
            get => Enum.TryParse<TipoRecurso>(Tipo, out var val) ? val : TipoRecurso.Otro;
            set => Tipo = value.ToString();
        }

        [StringLength(500)]
        public string Ruta { get; set; } = null!;

        [Column("MetaJSON")]
        public string? MetaJson { get; set; }

        public DateTime FechaCreacion { get; set; }

        [ForeignKey("IdProyecto")]
        [InverseProperty(nameof(Proyecto.Recursos))]
        public virtual Proyecto IdProyectoNavigation { get; set; } = null!;
    }

}
