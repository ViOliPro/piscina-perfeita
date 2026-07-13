using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class AplicacaoProdutoRequestDto
    {
        [Required(ErrorMessage = "O ID da piscina é obrigatório.")]
        public Guid PiscinaId { get; set; }

        [Required(ErrorMessage = "O ID do produto é obrigatório.")]
        public Guid ProdutoId { get; set; }

        [Required(ErrorMessage = "O ID do depósito é obrigatório.")]
        public Guid DepositoId { get; set; }

        public Guid? UsuarioId { get; set; }

        // Análise que motivou esta aplicação (opcional).
        public Guid? AnaliseId { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória.")]
        [Range(0.0001, 999999.99, ErrorMessage = "A quantidade deve ser maior do que zero.")]
        public decimal Quantidade { get; set; }

        // Unidade em que Quantidade foi informada (ex.: "mL"). Se omitido,
        // assume a UnidadeMedida cadastrada no Produto.
        public string? UnidadeLancamento { get; set; }

        public DateTimeOffset? DataAplicacao { get; set; }

        [StringLength(500, ErrorMessage = "As observações não podem passar de 500 caracteres.")]
        public string? Observacoes { get; set; }
    }
}
