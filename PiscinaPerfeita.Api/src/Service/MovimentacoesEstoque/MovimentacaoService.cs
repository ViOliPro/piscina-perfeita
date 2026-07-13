using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Helpers.Conversoes;
using PiscinaPerfeita.Api.Helpers.Estoque;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Depositos;
using PiscinaPerfeita.Api.Repository.Estoques;
using PiscinaPerfeita.Api.Repository.MovimentacoesEstoque;
using PiscinaPerfeita.Api.Repository.Piscinas;
using PiscinaPerfeita.Api.Repository.Produtos;
using PiscinaPerfeita.Api.Repository.Usuarios;

namespace PiscinaPerfeita.Api.Service.MovimentacoesEstoque
{
    public class MovimentacaoService : IMovimentacaoService
    {
        private readonly IMovimentacaoRepository _movimentacaoRepository;
        private readonly IEstoqueRepository _estoqueRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPiscinaRepository _piscinaRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IDepositoRepository _depositoRepository;
        private readonly IAuthenticatedUser _user;

        public MovimentacaoService(
            IMovimentacaoRepository movimentacaoRepository,
            IEstoqueRepository estoqueRepository,
            IAuthenticatedUser user,
            IUsuarioRepository usuarioRepository,
            IPiscinaRepository piscinaRepository,
            IProdutoRepository produtoRepository,
            IDepositoRepository depositoRepository
        )
        {
            _movimentacaoRepository =
                movimentacaoRepository
                ?? throw new ArgumentNullException(nameof(movimentacaoRepository));
            _estoqueRepository =
                estoqueRepository ?? throw new ArgumentNullException(nameof(estoqueRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _usuarioRepository =
                usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _piscinaRepository =
                piscinaRepository ?? throw new ArgumentNullException(nameof(piscinaRepository));
            _produtoRepository =
                produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
            _depositoRepository =
                depositoRepository ?? throw new ArgumentNullException(nameof(depositoRepository));
        }

        public async Task<List<MovimentacaoEstoqueResponseDto>> Show()
        {
            return await _movimentacaoRepository.Show();
        }

        public async Task<MovimentacaoEstoqueResponseDto?> GetById(Guid id)
        {
            var mov = await _movimentacaoRepository.GetById(id);

            if (mov == null)
                throw new KeyNotFoundException($"Movimentação com id {id} não encontrada.");

            return mov;
        }

        // Cria uma movimentação de estoque e — este é o ponto central da
        // feature — aplica o efeito dela no saldo do Estoque na mesma
        // operação atômica (ver IMovimentacaoRepository.CreateComAtualizacaoEstoque).
        public async Task<MovimentacaoEstoqueResponseDto> Create(MovimentacaoEstoqueRequestDto dto)
        {
            if (dto.TipoMovimentacao == Tipo.AjusteInventario)
                throw new InvalidOperationException(
                    "Movimentações do tipo Ajuste de Inventário são geradas automaticamente pela contagem física (POST /api/movimentacoes/contagem-inventario), não podem ser criadas diretamente."
                );

            if (
                CalculadoraEstoque.TiposQueExigemPiscina.Contains(dto.TipoMovimentacao)
                && dto.PiscinaId == null
            )
                throw new ArgumentException(
                    $"O ID da piscina é obrigatório para movimentações do tipo {dto.TipoMovimentacao}."
                );

            var userId = _user.GetUserId();
            var user = await _usuarioRepository.GetById(userId);
            if (user == null)
                throw new KeyNotFoundException($"Usuário com id {userId} não encontrado.");

            PiscinaResponseDto? piscina = null;
            if (dto.PiscinaId.HasValue)
            {
                piscina = await _piscinaRepository.GetById(dto.PiscinaId.Value);
                if (piscina == null)
                    throw new KeyNotFoundException(
                        $"Piscina com id {dto.PiscinaId} não encontrada."
                    );
            }

            var produto = await _produtoRepository.GetById(dto.ProdutoId);
            if (produto == null)
                throw new KeyNotFoundException($"Produto com id {dto.ProdutoId} não encontrado.");

            var deposito = await _depositoRepository.GetById(dto.DepositoId);
            if (deposito == null)
                throw new KeyNotFoundException(
                    $"Depósito com id {dto.DepositoId} não encontrado."
                );

            var quantidadeConvertida = ConversorUnidade.ConverterParaUnidadeBase(
                dto.Quantidade ?? 0,
                dto.UnidadeLancamento,
                produto.UnidadeMedida
            );

            var movimentacao = new MovimentacaoEstoque
            {
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                DepositoId = dto.DepositoId,
                UsuarioId = userId,
                TipoMovimentacao = dto.TipoMovimentacao,
                Quantidade = dto.Quantidade,
                UnidadeLancamento = dto.UnidadeLancamento,
            };

            var estoqueDb = await ObterOuCriarEstoque(
                dto.ProdutoId,
                dto.DepositoId,
                dto.TipoMovimentacao,
                produto.Nome,
                deposito.Nome,
                userId
            );

            var novaQuantidade = CalculadoraEstoque.CalcularNovaQuantidade(
                estoqueDb.QuantidadeAtual ?? 0,
                quantidadeConvertida,
                dto.TipoMovimentacao,
                produto.Nome,
                deposito.Nome
            );

            await _movimentacaoRepository.CreateComAtualizacaoEstoque(
                movimentacao,
                estoqueDb.Id,
                novaQuantidade
            );

            return new MovimentacaoEstoqueResponseDto
            {
                Id = movimentacao.Id,
                Piscina = piscina != null ? new NomeIdDto(dto.PiscinaId!.Value, piscina.Nome) : null,
                Produto = new NomeIdDto(movimentacao.ProdutoId, produto.Nome),
                Deposito = new NomeIdDto(movimentacao.DepositoId, deposito.Nome),
                Usuario = new NomeIdDto(movimentacao.UsuarioId, user.Nome),
                TipoMovimentacao = movimentacao.TipoMovimentacao,
                Quantidade = movimentacao.Quantidade,
                UnidadeLancamento = movimentacao.UnidadeLancamento,
                DataMovimentacao = movimentacao.DataMovimentacao,
            };
        }

        // NOTA IMPORTANTE: Update() aqui só corrige campos "de registro"
        // (ex.: corrigir a piscina errada em uma movimentação antiga) — ele
        // NÃO recalcula o efeito no Estoque. Editar a Quantidade ou o Tipo
        // de uma movimentação já criada NÃO ajusta o saldo retroativamente.
        // Isso é proposital: movimentações de estoque funcionam como um
        // livro-razão (ledger) — para corrigir uma quantidade errada, crie
        // uma nova movimentação compensatória (ou use a contagem de
        // inventário), não edite a original. Um PUT "burro" que recalculasse
        // o delta é fácil de fazer errado (ex.: em edições concorrentes) e
        // foge do padrão de auditoria que o restante do sistema já segue.
        public async Task<MovimentacaoEstoqueResponseDto> Update(
            Guid id,
            MovimentacaoEstoqueRequestDto dto
        )
        {
            var movimentacaoDb = await _movimentacaoRepository.GetById(id);
            if (movimentacaoDb == null)
            {
                throw new KeyNotFoundException($"Movimentação com id {id} não encontrada.");
            }

            if (dto.TipoMovimentacao == Tipo.AjusteInventario)
                throw new InvalidOperationException(
                    "Movimentações do tipo Ajuste de Inventário não podem ser editadas."
                );

            var movimentacaoUpdated = new MovimentacaoEstoque
            {
                Id = id,
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                DepositoId = dto.DepositoId,
                TipoMovimentacao = dto.TipoMovimentacao,
                Quantidade = dto.Quantidade,
                UnidadeLancamento = dto.UnidadeLancamento,
            };

            await _movimentacaoRepository.Update(id, movimentacaoUpdated);

            return new MovimentacaoEstoqueResponseDto
            {
                Id = id,
                Piscina = dto.PiscinaId.HasValue ? new NomeIdDto(dto.PiscinaId.Value, null) : null,
                Produto = new NomeIdDto(movimentacaoUpdated.ProdutoId, null),
                Deposito = new NomeIdDto(movimentacaoUpdated.DepositoId, null),
                TipoMovimentacao = movimentacaoUpdated.TipoMovimentacao,
                Quantidade = movimentacaoUpdated.Quantidade,
                UnidadeLancamento = movimentacaoUpdated.UnidadeLancamento,
            };
        }

        public async Task Delete(Guid id)
        {
            var movimentacaoDb = await _movimentacaoRepository.GetById(id);
            if (movimentacaoDb == null)
            {
                throw new KeyNotFoundException($"Movimentação com id {id} não encontrada.");
            }

            await _movimentacaoRepository.Delete(id);
        }

        // ------------------------------------------------------------
        // Feature: Contagem de Inventário / Ajuste
        //
        // Objetivo: mitigar perdas ou desvios de produtos. O usuário informa
        // a contagem física de vários produtos de um Depósito de uma vez; a
        // API compara com o saldo lógico e gera uma MovimentacaoEstoque do
        // tipo AjusteInventario para cada produto com diferença (contagens
        // que batem não geram movimentação, para não sujar o histórico).
        // ------------------------------------------------------------
        public async Task<List<ContagemInventarioResultadoDto>> RegistrarContagemInventario(
            ContagemInventarioRequestDto dto
        )
        {
            var deposito = await _depositoRepository.GetById(dto.DepositoId);
            if (deposito == null)
                throw new KeyNotFoundException(
                    $"Depósito com id {dto.DepositoId} não encontrado."
                );

            var usuarioId = dto.UsuarioId ?? _user.GetUserId();
            var resultados = new List<ContagemInventarioResultadoDto>();

            foreach (var item in dto.Itens)
            {
                var produto = await _produtoRepository.GetById(item.ProdutoId);
                if (produto == null)
                    throw new KeyNotFoundException(
                        $"Produto com id {item.ProdutoId} não encontrado."
                    );

                var estoqueDb = await _estoqueRepository.GetEntidadeByProdutoEDeposito(
                    item.ProdutoId,
                    dto.DepositoId
                );
                var quantidadeAnterior = estoqueDb?.QuantidadeAtual ?? 0;
                var diferenca = item.QuantidadeContada - quantidadeAnterior;

                if (diferenca == 0)
                {
                    resultados.Add(
                        new ContagemInventarioResultadoDto
                        {
                            ProdutoId = item.ProdutoId,
                            ProdutoNome = produto.Nome,
                            QuantidadeAnterior = quantidadeAnterior,
                            QuantidadeContada = item.QuantidadeContada,
                            Diferenca = 0,
                        }
                    );
                    continue;
                }

                if (estoqueDb == null)
                {
                    // Produto nunca teve estoque cadastrado neste depósito:
                    // cria já com a quantidade contada, sem gerar uma
                    // movimentação de ajuste (não havia saldo lógico prévio
                    // para "divergir").
                    var novoEstoque = new Estoque
                    {
                        ProdutoId = item.ProdutoId,
                        DepositoId = dto.DepositoId,
                        UsuarioId = usuarioId,
                        QuantidadeAtual = item.QuantidadeContada,
                        QuantidadeMinima = 5,
                    };
                    await _estoqueRepository.Create(novoEstoque);

                    resultados.Add(
                        new ContagemInventarioResultadoDto
                        {
                            ProdutoId = item.ProdutoId,
                            ProdutoNome = produto.Nome,
                            QuantidadeAnterior = 0,
                            QuantidadeContada = item.QuantidadeContada,
                            Diferenca = diferenca,
                            MovimentacaoEstoqueId = null,
                        }
                    );
                    continue;
                }

                var movimentacao = new MovimentacaoEstoque
                {
                    PiscinaId = null,
                    ProdutoId = item.ProdutoId,
                    DepositoId = dto.DepositoId,
                    UsuarioId = usuarioId,
                    TipoMovimentacao = Tipo.AjusteInventario,
                    // Para AjusteInventario, Quantidade guarda a diferença
                    // ASSINADA (pode ser negativa) — é o valor que, somado
                    // ao saldo anterior, resulta na quantidade contada.
                    Quantidade = diferenca,
                    UnidadeLancamento = produto.UnidadeMedida,
                };

                await _movimentacaoRepository.CreateComAtualizacaoEstoque(
                    movimentacao,
                    estoqueDb.Id,
                    item.QuantidadeContada
                );

                resultados.Add(
                    new ContagemInventarioResultadoDto
                    {
                        ProdutoId = item.ProdutoId,
                        ProdutoNome = produto.Nome,
                        QuantidadeAnterior = quantidadeAnterior,
                        QuantidadeContada = item.QuantidadeContada,
                        Diferenca = diferenca,
                        MovimentacaoEstoqueId = movimentacao.Id,
                    }
                );
            }

            return resultados;
        }

        // Busca o Estoque do produto no depósito; se não existir e o tipo
        // for de entrada, cria automaticamente com saldo zero (primeira
        // compra/entrada deste produto neste depósito). Para tipos de
        // saída, exige que o estoque já exista.
        private async Task<Estoque> ObterOuCriarEstoque(
            Guid produtoId,
            Guid depositoId,
            Tipo tipo,
            string nomeProduto,
            string nomeDeposito,
            Guid usuarioId
        )
        {
            var estoqueDb = await _estoqueRepository.GetEntidadeByProdutoEDeposito(
                produtoId,
                depositoId
            );

            if (estoqueDb != null)
                return estoqueDb;

            if (!CalculadoraEstoque.TiposDeEntrada.Contains(tipo))
                throw new InvalidOperationException(
                    $"Não há estoque de '{nomeProduto}' cadastrado no depósito '{nomeDeposito}' — não é possível registrar uma saída."
                );

            var novoEstoque = new Estoque
            {
                ProdutoId = produtoId,
                DepositoId = depositoId,
                UsuarioId = usuarioId,
                QuantidadeAtual = 0,
                QuantidadeMinima = 5,
            };
            await _estoqueRepository.Create(novoEstoque);
            return novoEstoque;
        }
    }
}
