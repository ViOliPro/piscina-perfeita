using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.MovimentacoesEstoque;
using PiscinaPerfeita.Api.Repository.Piscinas;
using PiscinaPerfeita.Api.Repository.Produtos;
using PiscinaPerfeita.Api.Repository.Usuarios;

namespace PiscinaPerfeita.Api.Service.MovimentacoesEstoque
{
    public class MovimentacaoService : IMovimentacaoService
    {
        private readonly IMovimentacaoRepository _movimentacaoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPiscinaRepository _piscinaRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IAuthenticatedUser _user;

        public MovimentacaoService(
            IMovimentacaoRepository movimentacaoRepository,
            IAuthenticatedUser user,
            IUsuarioRepository usuarioRepository,
            IPiscinaRepository piscinaRepository,
            IProdutoRepository produtoRepository
        )
        {
            _movimentacaoRepository =
                movimentacaoRepository
                ?? throw new ArgumentNullException(nameof(movimentacaoRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _usuarioRepository =
                usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _piscinaRepository =
                piscinaRepository ?? throw new ArgumentNullException(nameof(piscinaRepository));
            _produtoRepository =
                produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
        }

        // Implementação dos métodos do serviço
        // Metodo Show: Retorna uma lista de todos os estoques, incluindo as informações relacionadas de piscina e produto.
        public async Task<List<MovimentacaoEstoqueResponseDto>> Show()
        {
            return await _movimentacaoRepository.Show();
        }

        // Metodo GetById: Retorna um estoque específico com base no ID, incluindo as informações relacionadas de piscina e produto.
        public async Task<MovimentacaoEstoqueResponseDto?> GetById(Guid id)
        {
            var mov = await _movimentacaoRepository.GetById(id);

            if (mov == null)
            {
                throw new KeyNotFoundException($"Movimentação com id {id} não encontrada.");
            }

            return mov;
        }

        // Metodo Create: Cria um novo estoque com base nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<MovimentacaoEstoqueResponseDto> Create(MovimentacaoEstoqueRequestDto dto)
        {
            var user = await _usuarioRepository.GetById(_user.GetUserId());
            if (user == null)
                throw new KeyNotFoundException(
                    $"Usuário com id {_user.GetUserId()} não encontrado."
                );

            var piscina = await _piscinaRepository.GetById(dto.PiscinaId);
            if (piscina == null)
                throw new KeyNotFoundException($"Piscina com id {dto.PiscinaId} não encontrada.");

            var produto = await _produtoRepository.GetById(dto.ProdutoId);
            if (produto == null)
                throw new KeyNotFoundException($"Produto com id {dto.ProdutoId} não encontrado.");

            var movimentacao = new MovimentacaoEstoque
            {
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                UsuarioId = _user.GetUserId(),
                TipoMovimentacao = (Models.Tipo)dto.TipoMovimentacao,
                Quantidade = dto.Quantidade,
            };

            await _movimentacaoRepository.Create(movimentacao);

            return new MovimentacaoEstoqueResponseDto
            {
                Id = movimentacao.Id,
                Piscina = new NomeIdDto(movimentacao.PiscinaId, piscina.Nome),
                Produto = new NomeIdDto(movimentacao.ProdutoId, produto.Nome),
                Usuario = new NomeIdDto(movimentacao.UsuarioId, user.Nome),
                Quantidade = movimentacao.Quantidade,
                DataMovimentacao = movimentacao.DataMovimentacao,
            };
        }

        // Metodo Update: Atualiza um estoque existente com base no ID e nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
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

            var movimentacaoUpdated = new MovimentacaoEstoque
            {
                Id = id,
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                TipoMovimentacao = (Models.Tipo)dto.TipoMovimentacao,
                Quantidade = dto.Quantidade,
            };

            await _movimentacaoRepository.Update(id, movimentacaoUpdated);

            return new MovimentacaoEstoqueResponseDto
            {
                Id = id,
                Piscina = new NomeIdDto(movimentacaoUpdated.PiscinaId, null),
                Produto = new NomeIdDto(movimentacaoUpdated.ProdutoId, null),
                TipoMovimentacao = (Models.Tipo)movimentacaoUpdated.TipoMovimentacao,
                Quantidade = movimentacaoUpdated.Quantidade,
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
