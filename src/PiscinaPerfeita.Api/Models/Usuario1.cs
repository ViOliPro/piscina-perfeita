using System;
using System.Collections.Generic;

namespace PiscinaPerfeita.Api.Models;

public partial class Usuario1
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string SenhaHash { get; set; } = null!;

    public TimeOnly? CreatedAt { get; set; }

    public Guid PiscinaId { get; set; }

    public virtual Piscina Piscina { get; set; } = null!;

    public virtual ICollection<Piscina> Piscinas { get; set; } = new List<Piscina>();
}
