namespace PiscinaPerfeita.Api.Models
{
    // Dto para retornar apenas o Id e Nome de um objeto
    public record NomeIdDto(Guid Id, string? Nome);

    // Dto retorno Enum Estoque TipoMovimentacao
    //
    // IMPORTANTE: Entrada=0 e Saida=1 são os valores originais já gravados
    // no banco — não podem mudar de número, ou movimentações antigas
    // passam a ser lidas com o tipo errado. Os tipos novos sempre entram
    // com números novos no final da lista, nunca inseridos no meio.
    //
    // Convenção de sinal ao atualizar o Estoque (ver MovimentacaoRepository):
    // Entrada/Compra                 → soma ao QuantidadeAtual
    // Saida/Aplicacao/Perda/Descarte → subtrai do QuantidadeAtual
    // AjusteInventario               → soma a diferença (pode ser positiva
    //                                   ou negativa), calculada automaticamente
    //                                   pela feature de contagem de inventário
    public enum Tipo
    {
        Entrada = 0,
        Saida = 1,
        Compra = 2,
        Aplicacao = 3,
        Perda = 4,
        Descarte = 5,
        AjusteInventario = 6,
    }

    public enum Role
    {
        SuperAdmin = 0,
        Usuario = 1,
    }
}
