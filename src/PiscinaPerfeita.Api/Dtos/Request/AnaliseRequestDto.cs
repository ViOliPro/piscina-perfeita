using PiscinaPerfeita.Api.Models;


namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class AnaliseRequestDto
    {
        public Guid Id { get; set; }

        public Guid PiscinaId { get; set; }

        public DateTimeOffset DataAnalise { get; set; }

        public decimal? Ph { get; set; }

        public decimal? CloroLivre { get; set; }

        public decimal? Alcalinidade { get; set; }

        public List<decimal>? Temperatura { get; set; }

        public string? Observacoes { get; set; }

        public virtual Piscina Piscina { get; set; } = null!;
    }
}

