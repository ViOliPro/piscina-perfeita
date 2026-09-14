using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Data;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Helpers.Security;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Locais;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Repository.UsuariosLocal;
using PiscinaPerfeita.Api.Service.Email;
using PiscinaPerfeita.Api.Service.Audit;

namespace PiscinaPerfeita.Api.Service.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuariosRepository;
        private readonly IUsuarioLocalRepository _usuariosLocalRepository;
        private readonly ILocalRepository _locaisRepository;
        private readonly IAuthenticatedUser _user;
        private readonly IConfiguration _config;
        private readonly IEmailService _email;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _audit;

        public UsuarioService(
            IUsuarioRepository usuariosRepository,
            IAuthenticatedUser user,
            IUsuarioLocalRepository usuariosLocalRepository,
            ILocalRepository locaisRepository,
            IConfiguration config,
            IEmailService email,
            IUnitOfWork unitOfWork,
            IAuditService audit
        )
        {
            _usuariosRepository =
                usuariosRepository ?? throw new ArgumentNullException(nameof(usuariosRepository));
            _usuariosLocalRepository =
                usuariosLocalRepository
                ?? throw new ArgumentNullException(nameof(usuariosLocalRepository));
            _locaisRepository =
                locaisRepository ?? throw new ArgumentNullException(nameof(locaisRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        }

        public async Task<List<UsuarioResponseDto>> Show()
        {
            if (_user.IsSuperAdmin())
                return await _usuariosRepository.Show();

            var localId = _user.GetLocalId();
            if (localId == Guid.Empty)
                throw new InvalidOperationException(
                    "Selecione um Local ativo para listar os usuários."
                );

            return await _usuariosRepository.FilterRoleUsuario(localId);
        }

        public async Task<UsuarioResponseDto> GetById(Guid id)
        {
            var usuarioDb = await _usuariosRepository.GetByIdDto(id);
            if (usuarioDb == null)
            {
                throw new KeyNotFoundException($"Usuario com id {id} não encontrado");
            }

            await GarantirUsuarioNoTenantAtual(id, protegerAdministradorPai: false);

            return usuarioDb;
        }

        public async Task<Usuario?> GetUsuarioByEmail(string email)
        {
            var usuarioDb = await _usuariosRepository.GetByEmail(email);
            if (usuarioDb == null)
            {
                return null;
            }

            return usuarioDb;
        }

        public async Task AceitarTermos(Guid usuarioId)
        {
            await _usuariosRepository.AceitarTermos(usuarioId, LegalConstants.VersaoTermosAtual);
            await _audit.WriteAsync(
                new AuditEntry
                {
                    Action = AuditActions.AuthAcceptTerms,
                    EntityType = "Usuario",
                    EntityId = usuarioId,
                    Summary = "Aceite de termos de uso",
                    Payload = new { versao = LegalConstants.VersaoTermosAtual },
                }
            );
        }

        // STUB_RESTORE_MARKER - full methods follow in next push if truncated
        public Task PasswordResetToken(string email) => throw new NotImplementedException("temp");
    }
}
