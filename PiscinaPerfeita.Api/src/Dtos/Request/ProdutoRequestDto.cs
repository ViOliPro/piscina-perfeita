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
    public string? Fabricante { get; set; } = string.Empty;

    public string? Marca { get; set; } = string.Empty;

    public string? Observacoes { get; set; } = string.Empty;

    public string? Categoria { get; set; } = string.Empty;
}
