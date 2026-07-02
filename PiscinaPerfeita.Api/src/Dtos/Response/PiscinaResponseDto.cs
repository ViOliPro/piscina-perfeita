using System.Text.Json.Serialization;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response;

public class PiscinaResponseDto
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = null!;

    public decimal? VolumeLitros { get; set; }

    public decimal? ProfundidadeMedia { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    // Retorna o Id e Nome do usuário associado à piscina
    public NomeIdDto? UsuarioPiscina { get; set; }



    public virtual ICollection<AnalisePiscinaResponseDto> AnalisePiscina { get; set; } = new List<AnalisePiscinaResponseDto>();

    public virtual ICollection<MovimentacaoEstoquePiscinaResponsetDto> MovimentacoesEstoques { get; set; } = new List<MovimentacaoEstoquePiscinaResponsetDto>();

    public virtual ICollection<NomeIdDto> Estoques { get; set; } = new List<NomeIdDto>();




}

public class AnalisePiscinaResponseDto
{
    public Guid Id { get; set; }
    public DateTimeOffset DataAnalise { get; set; }
}

public class MovimentacaoEstoquePiscinaResponsetDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public DateTimeOffset DataMovimentacao { get; set; }
}