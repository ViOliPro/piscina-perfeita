using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Repository.Analises;


namespace PiscinaPerfeita.Api.Service.Estoques
{
    public class EstoqueService : IEstoqueService
    {
        private readonly IEstoqueService _usuariosRepository;

        public EstoqueService(IEstoqueService estoquesRepository)
        {
            _usuariosRepository = estoquesRepository ?? throw new ArgumentNullException(nameof(estoquesRepository));
        }

        public async Task<List<EstoqueResponseDto>> Show()
        {
            var estoques = await _usuariosRepository.Show();
            return estoques.Select(u => new EstoqueResponseDto
            {
                PiscinaId = u.PiscinaId,
                ProdutoId = u.ProdutoId,
                QuantidadeAtual = u.QuantidadeAtual,
            }).ToList();
        }

        public async Task<EstoqueResponseDto> GetById(Guid id)
        {
            var estoques = await _usuariosRepository.GetById(id);
            return new EstoqueResponseDto
            {
                PiscinaId = estoques.PiscinaId,
                ProdutoId = estoques.ProdutoId,
                QuantidadeAtual = estoques.QuantidadeAtual,
            };
        }

        public async Task<EstoqueResponseDto> Create(EstoqueRequestDto dto)
        {
            var estoqueId = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
            var estoque = new Usuario
            {
                PiscinaId = estoqueId.PiscinaId,
                ProdutoId = estoqueId.ProdutoId,
                QuantidadeAtual = estoqueId.QuantidadeAtual,
            };

            await _estoquesRepository.Create(estoque);


            return new EstoqueResponseDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
            };
        }

        public async Task<UsuarioResponseDto> Update(Guid id, UsuarioRequestDto dto)
        {
            var usuarioDb = await _usuariosRepository.GetById(id);
            if (usuarioDb == null)
            {
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");
            }

            usuarioDb.Nome = dto.Nome;
            usuarioDb.Email = dto.Email;
            usuarioDb.Senhahash = dto.SenhaHash;

            await _usuariosRepository.Update(id, usuarioDb);

            return new UsuarioResponseDto
            {
                Id = usuarioDb.Id,
                Nome = usuarioDb.Nome,
                Email = usuarioDb.Email
            };
        }

        public async Task Delete(Guid id)
        {
            var usuarioDb = await _usuariosRepository.GetById(id);
            if (usuarioDb == null)
            {
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");
            }

            await _usuariosRepository.Delete(id);

        }
    }
}
