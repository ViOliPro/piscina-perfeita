using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public partial class MovimentacaoEstoqueRequestDto
    {
        [Required(ErrorMessage = "O ID da piscina é obrigatório.")]
        public Guid PiscinaId { get; set; }

        [Required(ErrorMessage = "O ID do produto é obrigatório.")]
        public Guid ProdutoId { get; set; }

        [Required(ErrorMessage = "O tipo de movimentação é obrigatório.")]
        [EnumDataType(typeof(Tipo), ErrorMessage = "O tipo de movimentação deve ser 0 (Entrada) ou 1 (Saída).")]
        public Tipo TipoMovimentacao { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória.")]
        [Range(0.01, 999999.99, ErrorMessage = "A quantidade deve ser maior do que zero.")]
        public decimal? Quantidade { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.00, 999999.99, ErrorMessage = "O valor não pode ser negativo.")]
        public decimal Valor { get; set; }
    }

    public enum Tipo
    {
        Entrada = 0,
        Saida = 1
    }
}
