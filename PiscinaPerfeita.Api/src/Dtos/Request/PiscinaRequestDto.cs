using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request;

public partial class PiscinaRequestDto
{
    [Required(ErrorMessage = "O campo UsuarioId é obrigatório.")]
    public Guid UsuarioId { get; set; }

    public Guid LocalId { get; set; }

    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    [MaxLength(100)]
    public string Nome { get; set; } = null!;

    [Required(ErrorMessage = "O campo volume é obrigatório.")]
    [DisplayName("Volume")]
    public decimal? VolumeLitros { get; set; }

    [Required(ErrorMessage = "O campo Profundidade Média é obrigatório.")]
    [DisplayName("Profundidade")]
    public decimal? ProfundidadeMedia { get; set; }
}
