using System;
using System.Collections.Generic;
using PiscinaPerfeita.Api.Dtos.Request;

namespace PiscinaPerfeita.Api.Dtos.Response;

public partial class ProdutoResponseDto
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string UnidadeMedida { get; set; } = string.Empty;

    public virtual ICollection<EstoqueResponseDto> Estoques { get; set; } = new List<EstoqueResponseDto>();

    public virtual ICollection<MovimentacaoEstoqueRequestDto> MovimentacoesEstoques { get; set; } = new List<MovimentacaoEstoqueRequestDto>();
}
