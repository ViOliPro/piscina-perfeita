using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Helpers.Conversoes;
using PiscinaPerfeita.Api.Helpers.Estoque;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Analises;
using PiscinaPerfeita.Api.Repository.AplicacoesProduto;
using PiscinaPerfeita.Api.Repository.Depositos;
using PiscinaPerfeita.Api.Repository.Estoques;
using PiscinaPerfeita.Api.Repository.Piscinas;
using PiscinaPerfeita.Api.Repository.Produtos;

namespace PiscinaPerfeita.Api.Service.AplicacoesProduto
{
    public class AplicacaoProdutoService : IAplicacaoProdutoService
    {
        private readonly IAplicacaoProdutoRepository _aplicacaoRepository;
        private readonly IEstoqueRepository _estoqueRepository;
        private readonly IPiscinaRepository _piscinaRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IDepositoRepository _depositoRepository;
        private readonly IAnaliseRepository _analiseRepository;
        private readonly IAuthenticatedUser _user;

        public AplicacaoProdutoService(
            IAplicacaoProdutoRepository aplicacaoRepository,
            IEstoqueRepository estoqueRepository,
            IPiscinaRepository piscinaRepository,
            IProdutoRepository produtoRepository,
            IDepositoRepository depositoRepository,
            IAnaliseRepository analiseRepository,
            IAuthenticatedUser user
        )
        {
            _aplicacaoRepository =
                aplicacaoRepository ?? throw new ArgumentNullException(nameof(aplicacaoRepository));
            _estoqueRepository =
                estoqueRepository ?? throw new ArgumentNullException(nameof(estoqueRepository));
            _piscinaRepository =
                piscinaRepository ?? throw new ArgumentNullException(nameof(piscinaRepository));
            _produtoRepository =
                produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
            _depositoRepository =
                depositoRepository ?? throw new ArgumentNullException(nameof(depositoRepository));
            _analiseRepository =
                analiseRepository ?? throw new ArgumentNullException(nameof(analiseRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public async Task<List<AplicacaoProdutoResponseDto>> Show()
        {
            return await _aplicacaoRepository.Show();
        }

        public async Task<AplicacaoProdutoResponseDto> GetById(Guid id)
        {
            var aplicacao = await _aplicacaoRepository.GetById(id);
            if (aplicacao == null)
                throw new KeyNotFoundException($"Aplicação com id {id} não encontrada.");

            return aplicacao;
        }

        // Registra a aplicação de um produto numa piscina e — automaticamente,
        // na mesma operação — gera a MovimentacaoEstoque (Tipo=Aplicacao) e
        // dá baixa no Estoque do Depósito de onde o produto saiu.
        public async Task<AplicacaoProdutoResponseDto> Create(AplicacaoProdutoRequestDto dto)
        {
            var usuarioId = dto.UsuarioId ?? _user.GetUserId();

            var piscina = await _piscinaRepository.GetById(dto.PiscinaId);
            if (piscina == null)
                throw new KeyNotFoundException($"Piscina com id {dto.PiscinaId} não encontrada.");

            var produto = await _produtoRepository.GetById(dto.ProdutoId);
            if (produto == null)
                throw new KeyNotFoundException($"Produto com id {dto.ProdutoId} não encontrado.");

            var deposito = await _depositoRepository.GetById(dto.DepositoId);
            if (deposito == null)
                throw new KeyNotFoundException(
                    $"Depósito com id {dto.DepositoId} não encontrado."
                );

            if (dto.AnaliseId.HasValue)
            {
                var analise = await _analiseRepository.GetById(dto.AnaliseId.Value);
                if (analise == null)
                    throw new KeyNotFoundException(
                        $"Análise com id {dto.AnaliseId} não encontrada."
                    );
            }

            var estoqueDb = await _estoqueRepository.GetEntidadeByProdutoEDeposito(
                dto.ProdutoId,
                dto.DepositoId
            );
            if (estoqueDb == null)
                throw new InvalidOperationException(
                    $"Não há estoque de '{produto.Nome}' cadastrado no depósito '{deposito.Nome}' — cadastre o estoque antes de registrar uma aplicação."
                );

            var quantidadeConvertida = ConversorUnidade.ConverterParaUnidadeBase(
                dto.Quantidade,
                dto.UnidadeLancamento,
                produto.UnidadeMedida
            );

            var novaQuantidadeEstoque = CalculadoraEstoque.CalcularNovaQuantidade(
                estoqueDb.QuantidadeAtual ?? 0,
                quantidadeConvertida,
                Tipo.Aplicacao,
                produto.Nome,
                deposito.Nome
            );

            var unidadeLancamento = string.IsNullOrWhiteSpace(dto.UnidadeLancamento)
                ? produto.UnidadeMedida
                : dto.UnidadeLancamento;

            var movimentacao = new MovimentacaoEstoque
            {
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                DepositoId = dto.DepositoId,
                UsuarioId = usuarioId,
                TipoMovimentacao = Tipo.Aplicacao,
                Quantidade = dto.Quantidade,
                UnidadeLancamento = unidadeLancamento,
            };

            var aplicacao = new AplicacaoProduto
            {
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                DepositoId = dto.DepositoId,
                UsuarioId = usuarioId,
                AnaliseId = dto.AnaliseId,
                Quantidade = dto.Quantidade,
                UnidadeLancamento = unidadeLancamento,
                DataAplicacao = dto.DataAplicacao?.ToUniversalTime() ?? DateTimeOffset.UtcNow,
                Observacoes = dto.Observacoes,
            };

            await _aplicacaoRepository.Create(
                aplicacao,
                movimentacao,
                estoqueDb.Id,
                novaQuantidadeEstoque
            );

            return new AplicacaoProdutoResponseDto
            {
                Id = aplicacao.Id,
                Piscina = new NomeIdDto(dto.PiscinaId, piscina.Nome),
                Produto = new NomeIdDto(dto.ProdutoId, produto.Nome),
                Deposito = new NomeIdDto(dto.DepositoId, deposito.Nome),
                Usuario = new NomeIdDto(usuarioId, null),
                AnaliseId = aplicacao.AnaliseId,
                MovimentacaoEstoqueId = movimentacao.Id,
                Quantidade = aplicacao.Quantidade,
                UnidadeLancamento = aplicacao.UnidadeLancamento,
                DataAplicacao = aplicacao.DataAplicacao,
                Observacoes = aplicacao.Observacoes,
            };
        }
    }
}
