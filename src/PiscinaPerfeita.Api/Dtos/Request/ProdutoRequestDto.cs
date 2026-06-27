using System;
using System.Collections.Generic;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Dtos.Request;

public partial class ProdutoRequestDto
{
    public Guid Id { get; set; }

    public char Nome { get; set; }

    public char UnidadeMedida { get; set; }

    public virtual ICollection<EstoqueRequestDto> Estoques { get; set; } = new List<EstoqueRequestDto>();

    public virtual ICollection<MovimentacoesEstoqueRequestDto> MovimentacoesEstoques { get; set; } = new List<MovimentacoesEstoqueRequestDto>();
}
