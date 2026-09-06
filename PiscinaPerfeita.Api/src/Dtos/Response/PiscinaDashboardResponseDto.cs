using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response;

public class PiscinaDashboardResponseDto
{
    public PiscinaResumoDto Piscina { get; set; } = null!;

    public PeriodoDto Periodo { get; set; } = null!;

    public ContagensDto Contagens { get; set; } = null!;

    public List<AnaliseResumoDto> UltimasAnalises { get; set; } = [];

    public List<MovimentacaoResumoDto> UltimasMovimentacoes { get; set; } = [];

    public List<ProdutoUsoResumoDto> ProdutosUtilizados { get; set; } = [];
}

public class PiscinaResumoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public decimal? VolumeLitros { get; set; }
    public decimal? ProfundidadeMedia { get; set; }
    public NomeIdDto? UsuarioPiscina { get; set; }
}

public class ContagensDto
{
    public int Analises { get; set; }
    public int Movimentacoes { get; set; }
    public int Aplicacoes { get; set; }
}

public class AnaliseResumoDto
{
    public Guid Id { get; set; }
    public DateTimeOffset DataAnalise { get; set; }
    public decimal? Ph { get; set; }
    public decimal? CloroLivre { get; set; }
    public decimal? Alcalinidade { get; set; }
    public decimal? Temperatura { get; set; }
}

public class MovimentacaoResumoDto
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = null!;
    public DateTimeOffset DataMovimentacao { get; set; }
    public string? ProdutoNome { get; set; }
}

public class ProdutoUsoResumoDto
{
    public Guid ProdutoId { get; set; }
    public string Nome { get; set; } = null!;
    public decimal Quantidade { get; set; }
    public string Unidade { get; set; } = null!;
    public int Ocorrencias { get; set; }
}
