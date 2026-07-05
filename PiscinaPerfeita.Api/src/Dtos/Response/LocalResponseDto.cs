
namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class LocalResponseDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = null!;
        public string? Descricao { get; set; }
        public string? Telefone { get; set; }
        public string? Observacoes { get; set; }
        public string? Endereco { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Pais { get; set; }
        public string? Cep { get; set; }

    }
}
