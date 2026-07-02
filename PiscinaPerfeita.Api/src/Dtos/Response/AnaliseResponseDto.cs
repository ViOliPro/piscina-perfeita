using PiscinaPerfeita.Api.Models;
using System.Text.Json.Serialization;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class AnaliseResponseDto
    {
        public Guid Id { get; set; }

        public DateTimeOffset DataAnalise { get; set; }

        public decimal? Ph { get; set; }

        public decimal? CloroLivre { get; set; }

        public decimal? Alcalinidade { get; set; }

        public decimal? Temperatura { get; set; }

        public string? Observacoes { get; set; }

        // Usuario que realizou a análise
        public NomeIdDto? Usuario { get; set; }

        // Piscina analisada
        public NomeIdDto? Piscina { get; set; }
    }

}

