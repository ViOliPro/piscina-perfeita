using System.ComponentModel.DataAnnotations;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    // ATENÇÃO: este arquivo tinha um "enum Tipo" próprio, duplicado do
    // PiscinaPerfeita.Api.Models.Tipo (mesmos 2 valores originais, por
    // coincidência). O Service fazia cast numérico entre os dois — ao
    // adicionar novos tipos isso quebraria silenciosamente se os dois
    // enums saíssem de sincronia. Consolidado para usar um único enum.
    public partial class MovimentacaoEstoqueRequestDto
    {
        // Obrigatório para Entrada/Saida/Aplicacao; opcional para
        // Compra/Perda/Descarte/AjusteInventario (movimentações de
        // depósito, nem sempre ligadas a uma piscina específica) — a
        // validação condicional por Tipo acontece no MovimentacaoService.
        public Guid? PiscinaId { get; set; }

        [Required(ErrorMessage = "O ID do produto é obrigatório.")]
        public Guid ProdutoId { get; set; }

        [Required(ErrorMessage = "O ID do depósito é obrigatório.")]
        public Guid DepositoId { get; set; }

        [Required(ErrorMessage = "O tipo de movimentação é obrigatório.")]
        [EnumDataType(typeof(Tipo), ErrorMessage = "Tipo de movimentação inválido.")]
        public Tipo TipoMovimentacao { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória.")]
        [Range(0.01, 999999.99, ErrorMessage = "A quantidade deve ser maior do que zero.")]
        public decimal? Quantidade { get; set; }

        // Unidade em que "Quantidade" foi informada (ex.: "mL", "g").
        // Pode ser diferente da UnidadeMedida "base" do produto (ex.:
        // produto cadastrado em L, lançamento em mL) — a API converte
        // automaticamente antes de aplicar no Estoque. Se omitido, assume
        // a mesma unidade base do produto (sem conversão).
        public string? UnidadeLancamento { get; set; }
    }
}
