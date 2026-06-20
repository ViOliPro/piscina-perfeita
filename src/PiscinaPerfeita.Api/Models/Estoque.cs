using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models;

[Table("Estoque")]
public partial class Estoque
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid PiscinaId { get; set; }

    public Guid ProdutoId { get; set; }

    public decimal? QuantidadeAtual { get; set; }

    public virtual Piscina Piscina { get; set; } = null!;

    public virtual Produto Produto { get; set; } = null!;
}
