using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class FaixaIdealDto
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }

    public class PeriodoDto
    {
        public DateTimeOffset Inicio { get; set; }
        public DateTimeOffset Fim { get; set; }
    }

    // Status de um parâmetro frente à faixa ideal, calculado sempre a
    // partir da leitura mais recente do período — "SemDados" quando a
    // análise mais recente não preencheu esse campo (ex.: registrou só pH
    // e cloro, deixando alcalinidade/temperatura em branco).
    public enum StatusParametro
    {
        Ideal,
        Abaixo,
        Acima,
        SemDados,
    }

    public class ParametroResumoDto
    {
        public decimal? Valor { get; set; }
        public StatusParametro Status { get; set; }
    }

    public class ResumoQualidadeAguaDto
    {
        public DateTimeOffset? UltimaAnalise { get; set; }
        public ParametroResumoDto Ph { get; set; } = new();
        public ParametroResumoDto CloroLivre { get; set; } = new();
        public ParametroResumoDto Alcalinidade { get; set; } = new();
        public ParametroResumoDto Temperatura { get; set; } = new();

        // Frase pronta pra exibir fechada, antes do usuário expandir o
        // gráfico (ver conversa sobre UX mobile — resumo sempre visível,
        // gráfico completo só atrás do clique).
        public string TextoResumo { get; set; } = string.Empty;
    }

    public class PontoQualidadeAguaDto
    {
        public DateTimeOffset Data { get; set; }
        public decimal? Ph { get; set; }
        public decimal? CloroLivre { get; set; }
        public decimal? Alcalinidade { get; set; }
        public decimal? Temperatura { get; set; }
    }

    public class FaixasIdeaisDto
    {
        public FaixaIdealDto Ph { get; set; } = new();
        public FaixaIdealDto CloroLivre { get; set; } = new();
        public FaixaIdealDto Alcalinidade { get; set; } = new();
        public FaixaIdealDto Temperatura { get; set; } = new();
    }

    public class QualidadeAguaResponseDto
    {
        public NomeIdDto Piscina { get; set; } = null!;
        public PeriodoDto Periodo { get; set; } = new();
        public FaixasIdeaisDto FaixasIdeais { get; set; } = new();
        public ResumoQualidadeAguaDto Resumo { get; set; } = new();

        // Ordem ascendente por data (esquerda→direita no gráfico de linha)
        // — o repositório devolve em ordem decrescente (mais recente
        // primeiro), o Service inverte antes de montar esta lista.
        public List<PontoQualidadeAguaDto> Pontos { get; set; } = [];
    }
}
