using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request;

public partial class PiscinaRequestDto
{
    public Guid UsuarioId { get; set; }

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
