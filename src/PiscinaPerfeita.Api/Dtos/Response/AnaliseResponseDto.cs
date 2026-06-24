using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class AnaliseResponseDto
    {
        public Guid Id { get; set; }

        public PiscinaOrigem? Piscina { get; set; } = new PiscinaOrigem();

        public DateTimeOffset DataAnalise { get; set; }

        public decimal? Ph { get; set; }

        public decimal? CloroLivre { get; set; }

        public decimal? Alcalinidade { get; set; }

        public decimal? Temperatura { get; set; }

        public string? Observacoes { get; set; }
    }

    public class PiscinaOrigem
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = null!;
    }
}

