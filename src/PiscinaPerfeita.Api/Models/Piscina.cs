using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models;

public partial class Piscina
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    public string Nome { get; set; } = null!;

    public decimal? VolumeLitros { get; set; }

    public decimal? ProfundidadeMedia { get; set; }

    [Column(TypeName = "date")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<AnaliseRequestDto> Analises { get; set; } = new List<AnaliseRequestDto>();

    public virtual ICollection<EstoqueResponseDto> Estoques { get; set; } = new List<EstoqueResponseDto>();

    public virtual ICollection<MovimentacoesEstoqueRequestDto> MovimentacoesEstoques { get; set; } = new List<MovimentacoesEstoqueRequestDto>();

    public virtual Usuario1 Usuario { get; set; } = null!;

    public virtual ICollection<Usuario1> Usuario1s { get; set; } = new List<Usuario1>();
}
