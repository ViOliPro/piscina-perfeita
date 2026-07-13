using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    // Feature de contagem física ("Ajuste de Inventário"): o usuário informa
    // quanto contou fisicamente de cada produto em um Depósito, e a API
    // calcula sozinha a diferença em relação ao estoque lógico, gerando uma
    // MovimentacaoEstoque do tipo AjusteInventario para cada divergência.
    public class ContagemInventarioRequestDto
    {
        [Required(ErrorMessage = "O ID do depósito é obrigatório.")]
        public Guid DepositoId { get; set; }

        public Guid? UsuarioId { get; set; }

        [Required(ErrorMessage = "Informe ao menos um item de contagem.")]
        [MinLength(1, ErrorMessage = "Informe ao menos um item de contagem.")]
        public List<ContagemInventarioItemRequestDto> Itens { get; set; } = new();
    }

    public class ContagemInventarioItemRequestDto
    {
        [Required(ErrorMessage = "O ID do produto é obrigatório.")]
        public Guid ProdutoId { get; set; }

        // Quantidade contada fisicamente, já na unidade base do produto
        // (a mesma do Estoque) — sem conversão nesta feature, para evitar
        // ambiguidade na hora da contagem física.
        [Range(0, 999999.99, ErrorMessage = "A quantidade contada não pode ser negativa.")]
        public decimal QuantidadeContada { get; set; }
    }
}
