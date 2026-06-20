using System;
using System.Collections.Generic;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Models;

public partial class Produto
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string UnidadeMedida { get; set; } = string.Empty;

    public virtual ICollection<EstoqueResponseDto> Estoques { get; set; } = new List<EstoqueResponseDto>();

    public virtual ICollection<MovimentacoesEstoqueRequestDto> MovimentacoesEstoques { get; set; } = new List<MovimentacoesEstoqueRequestDto>();
}
