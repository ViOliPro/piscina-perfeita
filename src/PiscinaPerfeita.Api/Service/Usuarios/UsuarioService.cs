using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Repository.Analises;


namespace PiscinaPerfeita.Api.Service.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuariosRepository;

        public UsuarioService(IUsuarioRepository usuariosRepository)
        {
            _usuariosRepository = usuariosRepository ?? throw new ArgumentNullException(nameof(usuariosRepository));
        }

        public async Task<List<UsuarioResponseDto>> Show()
        {
            var usuarios = await _usuariosRepository.Show();
            return usuarios.Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email
            }).ToList();
        }

        public async Task<UsuarioResponseDto> GetById(Guid id)
        {
            var usuario = await _usuariosRepository.GetById(id);
            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
            };
        }

        public async Task<UsuarioResponseDto> Create(UsuarioRequestDto dto)
        {
            var novoId = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
            var usuario = new Usuario
            {
                Id = novoId,
                Nome = dto.Nome,
                Email = dto.Email,
                Senhahash = BCrypt.Net.BCrypt.HashPassword(dto.SenhaHash)
            };

            await _usuariosRepository.Create(usuario);


            return new UsuarioResponseDto
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
