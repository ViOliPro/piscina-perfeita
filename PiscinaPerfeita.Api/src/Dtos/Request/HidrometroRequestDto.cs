using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request;

public class HidrometroRequestDto
{
    public decimal LeituraAtual { get; set; }

    [Required(ErrorMessage = "A data e hora da leitura são obrigatórias.")]
    public DateTimeOffset DataLeitura { get; set; }

    [StringLength(500, ErrorMessage = "As observações podem ter no máximo 500 caracteres.")]
    public string? Observacoes { get; set; }
}
