
using System;
using System.Collections.Generic;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{

    public partial class MovimentacaoEstoqueResponseDto
    {
        public Guid Id { get; set; }

        public Guid PiscinaId { get; set; }

        public Guid ProdutoId { get; set; }

        public Tipo TipoMovimentacao { get; set; }

        public decimal? Quantidade { get; set; }

        public string Valor { get; set; } = string.Empty;

        public DateTimeOffset? DataMovimentacao { get; set; }

    }
    public enum Tipo
    {
        Entrada,
        Saida
    }
}
