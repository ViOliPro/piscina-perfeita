namespace PiscinaPerfeita.Api.Helpers.Conversoes
{
    // Converte quantidades entre a unidade em que um lançamento foi feito
    // (ex.: "mL" numa aplicação) e a unidade "base" em que o Produto tem
    // seu estoque controlado (ex.: "L" no cadastro do produto).
    //
    // Exemplo real do negócio: produto "Algicida" cadastrado em L, mas o
    // usuário aplica 500 mL na piscina → convertemos 500 mL para 0.5 L
    // antes de dar baixa no Estoque (que está sempre na unidade base do
    // Produto, nunca na unidade do lançamento).
    public static class ConversorUnidade
    {
        // Fator de conversão de cada unidade para a "unidade-âncora" da sua
        // família (kg para massa, L para volume). Unidades de contagem
        // (un/cx/sc) não têm conversão prevista — precisam ser idênticas.
        private static readonly Dictionary<string, (string Familia, decimal FatorParaAncora)> Fatores =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["kg"] = ("massa", 1m),
                ["g"] = ("massa", 0.001m),
                ["mg"] = ("massa", 0.000001m),
                ["l"] = ("volume", 1m),
                ["ml"] = ("volume", 0.001m),
                ["un"] = ("unidade", 1m),
                ["cx"] = ("unidade", 1m),
                ["sc"] = ("unidade", 1m),
            };

        /// <summary>
        /// Converte <paramref name="quantidade"/>, informada na unidade
        /// <paramref name="unidadeOrigem"/>, para a unidade
        /// <paramref name="unidadeBase"/> (normalmente a UnidadeMedida
        /// cadastrada no Produto).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando as unidades são de famílias incompatíveis (ex.:
        /// tentar converter "L" para "kg") ou desconhecidas.
        /// </exception>
        public static decimal ConverterParaUnidadeBase(
            decimal quantidade,
            string? unidadeOrigem,
            string unidadeBase
        )
        {
            // Sem unidade de lançamento informada: assume que já está na
            // unidade base do produto, sem conversão.
            if (string.IsNullOrWhiteSpace(unidadeOrigem))
                return quantidade;

            var origem = unidadeOrigem.Trim();
            var baseNormalizada = unidadeBase.Trim();

            if (string.Equals(origem, baseNormalizada, StringComparison.OrdinalIgnoreCase))
                return quantidade;

            if (
                !Fatores.TryGetValue(origem, out var infoOrigem)
                || !Fatores.TryGetValue(baseNormalizada, out var infoBase)
            )
                throw new InvalidOperationException(
                    $"Unidade '{origem}' ou '{baseNormalizada}' não reconhecida para conversão."
                );

            if (infoOrigem.Familia != infoBase.Familia)
                throw new InvalidOperationException(
                    $"Não é possível converter '{origem}' para '{baseNormalizada}' — unidades de grandezas diferentes."
                );

            // quantidade * fator(origem→âncora) / fator(base→âncora)
            return quantidade * infoOrigem.FatorParaAncora / infoBase.FatorParaAncora;
        }
    }
}
