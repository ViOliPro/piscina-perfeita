using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Locais;
using PiscinaPerfeita.Api.Repository.UsuariosLocal;

namespace PiscinaPerfeita.Api.Service.Locais
{
    public class LocalService : ILocalService
    {
        private readonly ILocalRepository _localRepository;
        private readonly IUsuarioLocalRepository _usuarioLocalRepository;
        private readonly IAuthenticatedUser _user;

        public LocalService(
            ILocalRepository localRepository,
            IUsuarioLocalRepository usuarioLocalRepository,
            IAuthenticatedUser user
        )
        {
            _localRepository =
                localRepository ?? throw new ArgumentNullException(nameof(localRepository));
            _usuarioLocalRepository =
                usuarioLocalRepository
                ?? throw new ArgumentNullException(nameof(usuarioLocalRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        // Um SuperAdmin enxerga todos os Locais cadastrados no sistema.
        // Um usuário comum só enxerga os Locais aos quais está vinculado.
        public async Task<List<LocalResponseDto>> Show()
        {
            var usuarioLogado = await _user.GetCurrentUser();

            if (usuarioLogado.Role == Role.SuperAdmin)
                return await _localRepository.Show();

            if (usuarioLogado.UserId == null)
                throw new UnauthorizedAccessException("Usuário não autenticado no sistema.");

            var vinculos = await _usuarioLocalRepository.GetAllByUserId(usuarioLogado.UserId.Value);
            var localIds = vinculos.Where(v => v.LocalId.HasValue).Select(v => v.LocalId!.Value);

            return await _localRepository.ShowByIds(localIds);
        }

        public async Task<LocalResponseDto> GetById(Guid id)
        {
            await GarantirAcessoAoLocal(id);

            var local = await _localRepository.GetById(id);
            if (local == null)
                throw new KeyNotFoundException($"Local com id {id} não encontrado.");

            return local;
        }

        // Somente um SuperAdmin pode cadastrar novos Locais (condomínios/unidades).
        public async Task<LocalResponseDto> Create(LocalRequestDto dto)
        {
            await GarantirSuperAdmin();

            var local = new Local
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Telefone = dto.Telefone,
                Observacoes = dto.Observacoes,
                Endereco = dto.Endereco,
                Cidade = dto.Cidade,
                Estado = dto.Estado,
                Pais = dto.Pais,
                Cep = dto.Cep,
            };

            await _localRepository.Create(local);

            return new LocalResponseDto
            {
                Id = local.Id,
                Nome = local.Nome,
                Descricao = local.Descricao,
                Telefone = local.Telefone,
                Observacoes = local.Observacoes,
                Endereco = local.Endereco,
                Cidade = local.Cidade,
                Estado = local.Estado,
                Pais = local.Pais,
                Cep = local.Cep,
            };
        }

        public async Task<LocalResponseDto> Update(Guid id, LocalRequestDto dto)
        {
            await GarantirSuperAdmin();

            var localExistente = await _localRepository.GetById(id);
            if (localExistente == null)
                throw new KeyNotFoundException($"Local com id {id} não encontrado.");

            var local = new Local
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Telefone = dto.Telefone,
                Observacoes = dto.Observacoes,
                Endereco = dto.Endereco,
                Cidade = dto.Cidade,
                Estado = dto.Estado,
                Pais = dto.Pais,
                Cep = dto.Cep,
            };

            await _localRepository.Update(id, local);

            return new LocalResponseDto
            {
                Id = id,
                Nome = local.Nome,
                Descricao = local.Descricao,
                Telefone = local.Telefone,
                Observacoes = local.Observacoes,
                Endereco = local.Endereco,
                Cidade = local.Cidade,
                Estado = local.Estado,
                Pais = local.Pais,
                Cep = local.Cep,
            };
        }

        public async Task Delete(Guid id)
        {
            await GarantirSuperAdmin();

            var local = await _localRepository.GetById(id);
            if (local == null)
                throw new KeyNotFoundException($"Local com id {id} não encontrado.");

            await _localRepository.Delete(id);
        }

        private async Task GarantirSuperAdmin()
        {
            var usuarioLogado = await _user.GetCurrentUser();
            if (usuarioLogado.Role != Role.SuperAdmin)
                throw new UnauthorizedAccessException(
                    "Somente um SuperAdmin pode gerenciar Locais."
                );
        }

        private async Task GarantirAcessoAoLocal(Guid localId)
        {
            var usuarioLogado = await _user.GetCurrentUser();
            if (usuarioLogado.Role == Role.SuperAdmin)
                return;

            if (usuarioLogado.UserId == null)
                throw new UnauthorizedAccessException("Usuário não autenticado no sistema.");

            var vinculo = await _usuarioLocalRepository.Vinculo(
                usuarioLogado.UserId.Value,
                localId
            );
            if (vinculo == null)
                throw new UnauthorizedAccessException(
                    "Você não tem permissão para acessar este Local."
                );
        }
    }
}
