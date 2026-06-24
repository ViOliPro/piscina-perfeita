using System;
using System.Collections.Generic;
using PiscinaPerfeita.Api.Dtos.Request;

namespace PiscinaPerfeita.Api.Dtos.Response;

public class PiscinaResponseDto
{
    public Guid Id { get; set; }


    public string Nome { get; set; } = null!;

    public decimal? VolumeLitros { get; set; }

    public decimal? ProfundidadeMedia { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public UsuarioPiscinaResponseDto? UsuarioPiscina { get; set; } = new UsuarioPiscinaResponseDto();

    public virtual ICollection<AnaliseRequestDto> Analises { get; set; } = new List<AnaliseRequestDto>();

    public virtual ICollection<EstoqueResponseDto> Estoques { get; set; } = new List<EstoqueResponseDto>();

    public virtual ICollection<MovimentacaoEstoqueRequestDto> MovimentacoesEstoques { get; set; } = new List<MovimentacaoEstoqueRequestDto>();
}

public class UsuarioPiscinaResponseDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
}