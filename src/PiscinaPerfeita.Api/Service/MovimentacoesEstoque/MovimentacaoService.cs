using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Dtos.Request;

namespace PiscinaPerfeita.Api.Service.MovimentacoesEstoque
{
    public class MovimentacaoService : IMovimentacaoService
    {
        private readonly IMovimentacaoService _movimentacaoRepository;

        public MovimentacaoService(IMovimentacaoService movimentacaoRepository)
        {
            _movimentacaoRepository = movimentacaoRepository ?? throw new ArgumentNullException(nameof(movimentacaoRepository));
        }


        // Implementação dos métodos do serviço
        // Metodo Show: Retorna uma lista de todos os estoques, incluindo as informações relacionadas de piscina e produto.
        public async Task<List<MovimentacaoEstoqueResponseDto>> Show()
        {
            var mov = await _movimentacaoRepository.Show();
            return mov.Select(u => new MovimentacaoEstoqueResponseDto
            {
                Id = u.Id,
                PiscinaId = u.PiscinaId,
                ProdutoId = u.ProdutoId,
                TipoMovimentacao = u.TipoMovimentacao,
                Quantidade = u.Quantidade,
                Valor = u.Valor,
                DataMovimentacao = u.DataMovimentacao,
                Piscina = u.Piscina,
                Produto = u.Produto
            }).ToList();
        }


        // Metodo GetById: Retorna um estoque específico com base no ID, incluindo as informações relacionadas de piscina e produto.
        public async Task<MovimentacaoEstoqueResponseDto> GetById(Guid id)
        {
            var u = await _movimentacaoRepository.GetById(id);
            return new MovimentacaoEstoqueResponseDto
            {
                Id = u.Id,
                PiscinaId = u.PiscinaId,
                ProdutoId = u.ProdutoId,
                TipoMovimentacao = u.TipoMovimentacao,
                Quantidade = u.Quantidade,
                Valor = u.Valor,
                DataMovimentacao = u.DataMovimentacao,
                Piscina = u.Piscina,
                Produto = u.Produto
            };
        }


        // Metodo Create: Cria um novo estoque com base nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<MovimentacaoEstoqueResponseDto> Create(MovimentacaoEstoqueRequestDto dto)
        {
            var movimentacaoId = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
            var movimentacao = new MovimentacaoEstoqueRequestDto
            {
                Id = dto.Id,
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                TipoMovimentacao = dto.TipoMovimentacao,
                Quantidade = dto.Quantidade,
                Valor = dto.Valor,
                DataMovimentacao = dto.DataMovimentacao,
                Piscina = dto.Piscina,
                Produto = dto.Produto
            };

            await _movimentacaoRepository.Create(movimentacao);


            return new MovimentacaoEstoqueResponseDto
            {
                Id = movimentacao.Id,
                PiscinaId = movimentacao.PiscinaId,
                ProdutoId = movimentacao.ProdutoId,
                Quantidade = movimentacao.Quantidade,
                Valor = movimentacao.Valor,
                DataMovimentacao = movimentacao.DataMovimentacao
            };
        }


        // Metodo Update: Atualiza um estoque existente com base no ID e nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<MovimentacaoEstoqueResponseDto> Update(Guid id, MovimentacaoEstoqueRequestDto dto)
        {
            var movimentacaoDb = await _movimentacaoRepository.GetById(id);
            if (movimentacaoDb == null)
            {
                throw new KeyNotFoundException($"Movimentação com id {id} não encontrada.");
            }

            var movimentacaoUpdated = new MovimentacaoEstoqueRequestDto
            {
                Id = id,
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                TipoMovimentacao = dto.TipoMovimentacao,
                Quantidade = dto.Quantidade,
                Valor = dto.Valor,
                DataMovimentacao = dto.DataMovimentacao
            };

            await _movimentacaoRepository.Update(id, movimentacaoUpdated);

            return new MovimentacaoEstoqueResponseDto
            {
                Id = id,
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                TipoMovimentacao = dto.TipoMovimentacao,
                Quantidade = dto.Quantidade,
                Valor = dto.Valor,
                DataMovimentacao = dto.DataMovimentacao
            };
        }


        // Metodo Delete: Exclui um estoque existente com base no ID.
        public async Task Delete(Guid id)
        {
            var movimentacaoDb = await _movimentacaoRepository.GetById(id);
            if (movimentacaoDb == null)
            {
                throw new KeyNotFoundException($"Movimentação com id {id} não encontrada.");
            }

            await _movimentacaoRepository.Delete(id);

        }
    }
}
