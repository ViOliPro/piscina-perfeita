using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request;

public partial class ProdutoRequestDto
{
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    public string Nome { get; set; } = null!;

    [Required(ErrorMessage = "O campo unidade de medida é obrigatório.")]
    [DisplayName("Unidade de Medida")]
    public string UnidadeMedida { get; set; } = string.Empty;
}
