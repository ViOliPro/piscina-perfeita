using System;
using System.Collections.Generic;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Dtos.Request;

public partial class PiscinaRequestDto
{
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    public string Nome { get; set; } = null!;

    public decimal? VolumeLitros { get; set; }

    public decimal? ProfundidadeMedia { get; set; }

    public TimeOnly? CreatedAt { get; set; }

    public virtual ICollection<AnaliseRequestDto> Analises { get; set; } = new List<AnaliseRequestDto>();

    public virtual ICollection<EstoqueResponseDto> Estoques { get; set; } = new List<EstoqueResponseDto>();

    public virtual ICollection<MovimentacoesEstoqueRequestDto> MovimentacoesEstoques { get; set; } = new List<MovimentacoesEstoqueRequestDto>();
}
