using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Analises;
using PiscinaPerfeita.Api.Repository.Piscinas;
using PiscinaPerfeita.Api.Repository.Usuarios;

namespace PiscinaPerfeita.Api.Service.Analises
{
    public class AnaliseService : IAnaliseService
    {
        private readonly IAnaliseRepository _analiseRepository;
        private readonly IAuthenticatedUser _user;
        private readonly IUsuarioRepository _userRepository;
        private readonly IPiscinaRepository _piscinaRepository;

        public AnaliseService(
            IAnaliseRepository analisesRepository,
            IAuthenticatedUser user,
            IUsuarioRepository userRepository,
            IPiscinaRepository piscinaRepository
        )
        {
            _analiseRepository =
                analisesRepository ?? throw new ArgumentNullException(nameof(analisesRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _userRepository =
                userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _piscinaRepository =
                piscinaRepository ?? throw new ArgumentNullException(nameof(piscinaRepository));
        }

        // Implementação dos métodos do serviço
        // Metodo Show: Retorna uma lista de todos os estoques, incluindo as informações relacionadas de piscina e produto.
        public async Task<List<AnaliseResponseDto>> Show()
        {
            return await _analiseRepository.Show();
        }

        // Metodo GetById: Retorna um estoque específico com base no ID, incluindo as informações relacionadas de piscina e produto.
        public async Task<AnaliseResponseDto> GetById(Guid id)
        {
            var analiseDb = await _analiseRepository.GetById(id);

            return analiseDb == null
                ? throw new KeyNotFoundException(
                    $"Não possivel localizar uma analise com o id {id} informado"
                )
                : analiseDb;
        }

        // Metodo Create: Cria um novo Analise com base nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<AnaliseResponseDto> Create(AnaliseRequestDto dto)
        {
            var piscinaDb = await _piscinaRepository.GetById(dto.PiscinaId);
            if (piscinaDb == null)
                throw new KeyNotFoundException("Problemas ao registrar, piscina não localizado");

            var userDb = await _userRepository.GetById(_user.GetUserId());
            if (userDb == null)
                throw new KeyNotFoundException(
                    "Problemas ao registrar, usuario ID analise não localizado"
                );

            var analise = new Analise
            {
                PiscinaId = dto.PiscinaId,
                UsuarioId = dto.UsuarioId ?? _user.GetUserId(),
                Ph = dto.Ph ?? null,
                CloroLivre = dto.CloroLivre ?? null,
                Alcalinidade = dto.Alcalinidade ?? null,
                Temperatura = dto.Temperatura ?? null,
                Observacoes = dto.Observacoes,
                DataAnalise = dto.DataAnalise?.ToUniversalTime() ?? DateTimeOffset.UtcNow,
            };

            await _analiseRepository.Create(analise);

            return new AnaliseResponseDto
            {
                Id = analise.Id,
                DataAnalise = analise.DataAnalise,
                Ph = analise.Ph,
                CloroLivre = analise.CloroLivre,
                Alcalinidade = analise.Alcalinidade,
                Temperatura = analise.Temperatura,
                Observacoes = analise.Observacoes,
                Piscina = new NomeIdDto(analise.PiscinaId, piscinaDb.Nome),
                Usuario = new NomeIdDto(analise.UsuarioId, userDb.Nome),
            };
        }

        // Metodo Update: Atualiza um estoque existente com base no ID e nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<AnaliseResponseDto> Update(Guid id, AnaliseRequestDto dto)
        {
            var analisesDb = await _analiseRepository.GetById(id);
            if (analisesDb == null)
            {
                throw new KeyNotFoundException($"Analise com id {id} não encontrado.");
            }

            var analisesUpdated = new Analise
            {
                Id = id,
                PiscinaId = dto.PiscinaId,
                Ph = dto.Ph,
                CloroLivre = dto.CloroLivre,
                Alcalinidade = dto.Alcalinidade,
                Temperatura = dto.Temperatura,
                Observacoes = dto.Observacoes,
            };

            await _analiseRepository.Update(id, analisesUpdated);

            return new AnaliseResponseDto
            {
                Id = analisesUpdated.Id,
                DataAnalise = analisesUpdated.DataAnalise,
                Ph = analisesUpdated.Ph,
                CloroLivre = analisesUpdated.CloroLivre,
                Alcalinidade = analisesUpdated.Alcalinidade,
                Temperatura = analisesUpdated.Temperatura,
                Observacoes = analisesUpdated.Observacoes,
                Piscina = new NomeIdDto(analisesUpdated.PiscinaId, null),
                Usuario = new NomeIdDto(analisesUpdated.UsuarioId, null),
            };
        }

        // Metodo Delete: Exclui um estoque existente com base no ID.
        public async Task Delete(Guid id)
        {
            var analisesDb = await _analiseRepository.GetById(id);
            if (analisesDb == null)
            {
                throw new KeyNotFoundException($"Estoque com id {id} não encontrado.");
            }

            await _analiseRepository.Delete(id);
        }

        public async Task<QualidadeAguaResponseDto> ObterQualidadeAgua(
            Guid piscinaId,
            DateTimeOffset? inicio,
            DateTimeOffset? fim
        )
        {
            var piscinaDb = await _piscinaRepository.GetById(piscinaId);
            if (piscinaDb == null)
                throw new KeyNotFoundException("Piscina não encontrada.");

            var fimReal = fim ?? DateTimeOffset.UtcNow;
            var inicioReal = inicio ?? fimReal.AddDays(-30);

            // Show() já vem em ordem decrescente (mais recente primeiro) —
            // conveniente pra achar a última análise sem ordenar de novo.
            var analises = await _analiseRepository.Show(inicioReal, fimReal, piscinaId);

            var ultima = analises.FirstOrDefault();

            var resumo = new ResumoQualidadeAguaDto
            {
                UltimaAnalise = ultima?.DataAnalise,
                Ph = MontarParametroResumo(ultima?.Ph, AnaliseFaixasIdeais.Ph),
                CloroLivre = MontarParametroResumo(
                    ultima?.CloroLivre,
                    AnaliseFaixasIdeais.CloroLivre
                ),
                Alcalinidade = MontarParametroResumo(
                    ultima?.Alcalinidade,
                    AnaliseFaixasIdeais.Alcalinidade
                ),
                Temperatura = MontarParametroResumo(
                    ultima?.Temperatura,
                    AnaliseFaixasIdeais.Temperatura
                ),
            };
            resumo.TextoResumo = MontarTextoResumo(resumo);

            return new QualidadeAguaResponseDto
            {
                Piscina = new NomeIdDto(piscinaId, piscinaDb.Nome),
                Periodo = new PeriodoDto { Inicio = inicioReal, Fim = fimReal },
                FaixasIdeais = new FaixasIdeaisDto
                {
                    Ph = new FaixaIdealDto
                    {
                        Min = AnaliseFaixasIdeais.Ph.Min,
                        Max = AnaliseFaixasIdeais.Ph.Max,
                    },
                    CloroLivre = new FaixaIdealDto
                    {
                        Min = AnaliseFaixasIdeais.CloroLivre.Min,
                        Max = AnaliseFaixasIdeais.CloroLivre.Max,
                    },
                    Alcalinidade = new FaixaIdealDto
                    {
                        Min = AnaliseFaixasIdeais.Alcalinidade.Min,
                        Max = AnaliseFaixasIdeais.Alcalinidade.Max,
                    },
                    Temperatura = new FaixaIdealDto
                    {
                        Min = AnaliseFaixasIdeais.Temperatura.Min,
                        Max = AnaliseFaixasIdeais.Temperatura.Max,
                    },
                },
                Resumo = resumo,
                // analises vem decrescente (Show()); o gráfico de linha
                // precisa de ordem ascendente (esquerda = mais antigo).
                Pontos = analises
                    .OrderBy(a => a.DataAnalise)
                    .Select(a => new PontoQualidadeAguaDto
                    {
                        Data = a.DataAnalise,
                        Ph = a.Ph,
                        CloroLivre = a.CloroLivre,
                        Alcalinidade = a.Alcalinidade,
                        Temperatura = a.Temperatura,
                    })
                    .ToList(),
            };
        }

        private static ParametroResumoDto MontarParametroResumo(decimal? valor, FaixaIdeal faixa)
        {
            if (valor is null)
                return new ParametroResumoDto { Valor = null, Status = StatusParametro.SemDados };

            var status =
                valor < faixa.Min ? StatusParametro.Abaixo
                : valor > faixa.Max ? StatusParametro.Acima
                : StatusParametro.Ideal;

            return new ParametroResumoDto { Valor = valor, Status = status };
        }

        // Prioridade de destaque quando mais de um parâmetro está fora da
        // faixa: cloro primeiro (afeta desinfecção/segurança da água mais
        // diretamente), depois pH, alcalinidade e temperatura.
        private static string MontarTextoResumo(ResumoQualidadeAguaDto resumo)
        {
            if (resumo.UltimaAnalise is null)
                return "Nenhuma análise registrada no período.";

            (
                string Nome,
                ParametroResumoDto Parametro,
                string Unidade,
                FaixaIdeal Faixa
            )[] parametros =
            [
                ("Cloro", resumo.CloroLivre, "ppm", AnaliseFaixasIdeais.CloroLivre),
                ("pH", resumo.Ph, "", AnaliseFaixasIdeais.Ph),
                ("Alcalinidade", resumo.Alcalinidade, "ppm", AnaliseFaixasIdeais.Alcalinidade),
                ("Temperatura", resumo.Temperatura, "°C", AnaliseFaixasIdeais.Temperatura),
            ];

            var foraDaFaixa = parametros.FirstOrDefault(p =>
                p.Parametro.Status is StatusParametro.Abaixo or StatusParametro.Acima
            );

            if (foraDaFaixa != default)
            {
                var direcao =
                    foraDaFaixa.Parametro.Status == StatusParametro.Abaixo ? "abaixo" : "acima";
                return $"{foraDaFaixa.Nome} {direcao} do ideal na última medição "
                    + $"({foraDaFaixa.Parametro.Valor}{foraDaFaixa.Unidade}, ideal "
                    + $"{foraDaFaixa.Faixa.Min}–{foraDaFaixa.Faixa.Max}{foraDaFaixa.Unidade})";
            }

            if (parametros.All(p => p.Parametro.Status == StatusParametro.SemDados))
                return "Última análise não registrou nenhum parâmetro numérico.";

            return "Todos os parâmetros dentro da faixa ideal na última medição.";
        }
    }
}
