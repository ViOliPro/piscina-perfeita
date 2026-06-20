using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Repository.MovimentacoesEstoque;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Service.MovimentacoesEstoque
{
    public class MovimentacaoService : IMovimentacaoService
    {
        private readonly IMovimentacaoRepository _movimentacaoRepository;

        public MovimentacaoService(IMovimentacaoRepository movimentacaoRepository)
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
                TipoMovimentacao = (Dtos.Response.Tipo)u.TipoMovimentacao,
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
                TipoMovimentacao = (Dtos.Response.Tipo)u.TipoMovimentacao,
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
            var movimentacao = new MovimentacaoEstoque
            {
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                TipoMovimentacao = (Models.Tipo)dto.TipoMovimentacao,
                Quantidade = dto.Quantidade,
                Valor = dto.Valor
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

            var movimentacaoUpdated = new MovimentacaoEstoque
            {
                Id = id,
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                TipoMovimentacao = (Models.Tipo)dto.TipoMovimentacao,
                Quantidade = dto.Quantidade,
                Valor = dto.Valor,
            };

            await _movimentacaoRepository.Update(id, movimentacaoUpdated);

            return new MovimentacaoEstoqueResponseDto
            {
                Id = id,
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                TipoMovimentacao = (Dtos.Response.Tipo)dto.TipoMovimentacao,
                Quantidade = dto.Quantidade,
                Valor = dto.Valor
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
