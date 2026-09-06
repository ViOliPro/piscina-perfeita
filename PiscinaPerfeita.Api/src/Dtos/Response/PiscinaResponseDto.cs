using System.Text.Json.Serialization;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response;

/// <summary>
/// DTO leve de Piscina — usado em listagens e selects.
/// NÃO inclui coleções de análises, estoques ou movimentações
/// (essas vivem em endpoints analíticos sob demanda).
/// </summary>
public class PiscinaResponseDto
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = null!;

    public decimal? VolumeLitros { get; set; }

    public decimal? ProfundidadeMedia { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public NomeIdDto? UsuarioPiscina { get; set; }
}
