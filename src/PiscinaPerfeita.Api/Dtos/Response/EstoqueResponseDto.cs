using PiscinaPerfeita.Api.Models;
using System;
using System.Collections.Generic;
using PiscinaPerfeita.Api.Dtos.Request;

namespace PiscinaPerfeita.Api.Dtos.Response;

public partial class EstoqueResponseDto
{
    public Guid PiscinaId { get; set; }

    public Guid ProdutoId { get; set; }

    public decimal? QuantidadeAtual { get; set; }

}
