using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class LocalRequestDto
    {
        public string Nome { get; set; } = null!;
        public string? Descricao { get; set; }
        public string? Telefone { get; set; }
        public string? Observacoes { get; set; }
        public string? Endereco { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Pais { get; set; }
        [RegularExpression(@"^\d{5}-\d{3}$", ErrorMessage = "CEP must be in the format 12345-678")]
        public string? Cep { get; set; }
    }
}
