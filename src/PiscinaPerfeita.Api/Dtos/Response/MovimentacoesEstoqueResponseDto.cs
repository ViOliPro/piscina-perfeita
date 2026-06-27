
using System;
using System.Collections.Generic;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response;

public partial class MovimentacoesEstoqueResponseDto
{
    public Guid Id { get; set; }

    public Guid PiscinaId { get; set; }

    public Guid ProdutoId { get; set; }

    public char TipoMovimentacao { get; set; }

    public decimal? Quantidade { get; set; }

    public decimal? Valor { get; set; }

    public DateTimeOffset? DataMovimentacao { get; set; }

    public virtual Piscina Piscina { get; set; } = null!;

    public virtual Produto Produto { get; set; } = null!;
}
