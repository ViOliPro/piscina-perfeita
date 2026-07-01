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
            return await _usuariosRepository.Show();
        }

        public async Task<UsuarioResponseDto> GetById(Guid id)
        {
            var usuarioDb = await _usuariosRepository.GetById(id);
            if(usuarioDb == null)
            {
                throw new KeyNotFoundException($"Usuario com id {id} não encontrado");
            }

            return usuarioDb;
        }

        public async Task<UsuarioResponseDto> Create(UsuarioRequestDto dto)
        {
            var usuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.SenhaHash),
                Role = dto.Role
            };

            await _usuariosRepository.Create(usuario);


            return new UsuarioResponseDto
            {
                Nome = usuario.Nome,
                Email = usuario.Email,
                Role = usuario.Role,
                CreatedAt = usuario.CreatedAt
            };
        }

        public async Task<UsuarioResponseDto> Update(Guid id, UsuarioRequestUpdateDto dto)
        {
            if(id == Guid.Empty)
            {
                throw new ArgumentException($"O id informado não pode ser vazio {nameof(id)} .");
            }

            var usuario = await _usuariosRepository.GetById(id);
            if (usuario == null)
            {
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");
            }


            var usuarioDb = new Usuario
            {
                Id = id,
                Nome = !string.IsNullOrEmpty(dto.Nome) ? dto.Nome : usuario.Nome,
                Email = !string.IsNullOrEmpty(dto.Email) ? dto.Email : usuario.Email,
                SenhaHash = !string.IsNullOrEmpty(dto.SenhaHash) ? BCrypt.Net.BCrypt.HashPassword(dto.SenhaHash) : dto.SenhaHash,
                Role = dto.Role ?? usuario.Role 
            };
       
            await _usuariosRepository.Update(id, usuarioDb);

            return new UsuarioResponseDto
            {
                Id = usuarioDb.Id,
                Nome = usuarioDb.Nome,
                Email = usuarioDb.Email,
                CreatedAt = usuarioDb.CreatedAt,
                Role = usuarioDb.Role
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
