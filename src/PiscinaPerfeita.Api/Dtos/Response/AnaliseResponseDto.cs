using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Request;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class AnaliseResponseDto
    {
        public Guid Id { get; set; }

        public Guid PiscinaId { get; set; }

        public DateTimeOffset DataAnalise { get; set; }

        public decimal? Ph { get; set; }

        public decimal? CloroLivre { get; set; }

        public decimal? Alcalinidade { get; set; }

        public List<decimal>? Temperatura { get; set; }

        public string? Observacoes { get; set; }

        public virtual PiscinaRequestDto Piscina { get; set; } = null!;
    }
}

