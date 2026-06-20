using System;
using System.Collections.Generic;

namespace PiscinaPerfeita.Api.Dtos.Request;

public partial class EstoqueRequestDto
{
    public Guid Id { get; set; }

    public Guid PiscinaId { get; set; }

    public Guid ProdutoId { get; set; }

    public decimal? QuantidadeAtual { get; set; }

    public virtual PiscinaRequestDto Piscina { get; set; } = null!;

    public virtual ProdutoRequestDto Produto { get; set; } = null!;
}
