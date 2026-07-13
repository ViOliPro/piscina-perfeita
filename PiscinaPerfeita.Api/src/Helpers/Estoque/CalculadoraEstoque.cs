using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Helpers.Estoque
{
    // Regra central de "quanto o Estoque deve mudar" para cada Tipo de
    // movimentação. Compartilhada entre MovimentacaoService e
    // AplicacaoProdutoService para não duplicar (e arriscar dessincronizar)
    // essa lógica em dois lugares.
    public static class CalculadoraEstoque
    {
        public static readonly HashSet<Tipo> TiposDeEntrada = new() { Tipo.Entrada, Tipo.Compra };

        public static readonly HashSet<Tipo> TiposDeSaida = new()
        {
            Tipo.Saida,
            Tipo.Aplicacao,
            Tipo.Perda,
            Tipo.Descarte,
        };

        // Tipos de movimentação que exigem uma Piscina associada — os
        // demais (Compra/Perda/Descarte/AjusteInventario) são movimentações
        // de depósito, nem sempre ligadas a uma piscina específica.
        public static readonly HashSet<Tipo> TiposQueExigemPiscina = new()
        {
            Tipo.Entrada,
            Tipo.Saida,
            Tipo.Aplicacao,
        };

        /// <summary>
        /// Calcula o novo saldo do Estoque após aplicar uma movimentação.
        /// </summary>
        /// <param name="quantidadeMovimentada">
        /// Já convertida para a unidade base do produto. Para
        /// AjusteInventario, é a DIFERENÇA assinada (pode ser negativa);
        /// para os demais tipos, é sempre a magnitude (positiva).
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando uma saída deixaria o saldo negativo.
        /// </exception>
        public static decimal CalcularNovaQuantidade(
            decimal quantidadeAtual,
            decimal quantidadeMovimentada,
            Tipo tipo,
            string nomeProduto,
            string nomeDeposito
        )
        {
            if (tipo == Tipo.AjusteInventario || TiposDeEntrada.Contains(tipo))
                return quantidadeAtual + quantidadeMovimentada;

            if (TiposDeSaida.Contains(tipo))
            {
                var resultado = quantidadeAtual - quantidadeMovimentada;
                if (resultado < 0)
                    throw new InvalidOperationException(
                        $"Estoque insuficiente de '{nomeProduto}' no depósito '{nomeDeposito}': "
                            + $"saldo atual {quantidadeAtual}, tentando remover {quantidadeMovimentada}."
                    );
                return resultado;
            }

            throw new InvalidOperationException($"Tipo de movimentação '{tipo}' não reconhecido.");
        }
    }
}
