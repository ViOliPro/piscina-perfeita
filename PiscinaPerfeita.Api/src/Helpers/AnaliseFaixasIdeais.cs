namespace PiscinaPerfeita.Api.Helpers
{
    public record FaixaIdeal(decimal Min, decimal Max);

    // Fonte única de verdade das faixas ideais de qualidade da água.
    // Antes essas faixas viviam hardcoded em ANALISE_FAIXAS
    // (PiscinaPerfeita.Front/src/config/index.js) — movidas pra cá pra
    // evitar o risco de front e back divergirem silenciosamente se algum
    // dia só um dos dois for atualizado. O endpoint
    // GET /api/analises/qualidade-agua devolve esses valores prontos pro
    // front desenhar a faixa sombreada no gráfico, sem precisar manter
    // uma cópia própria.
    //
    // Atenção: os 4 componentes abaixo ainda usam ANALISE_FAIXAS local no
    // front (não migrados nesta tarefa, escopo era só o novo endpoint):
    // Analises.jsx, UltimasAnalisesCard.jsx, QualidadeAguaCard.jsx,
    // PhScale.jsx.
    public static class AnaliseFaixasIdeais
    {
        public static readonly FaixaIdeal Ph = new(7.2m, 7.8m);
        public static readonly FaixaIdeal CloroLivre = new(1.0m, 3.0m);
        public static readonly FaixaIdeal Alcalinidade = new(80m, 120m);
        public static readonly FaixaIdeal Temperatura = new(26m, 30m);
    }
}
