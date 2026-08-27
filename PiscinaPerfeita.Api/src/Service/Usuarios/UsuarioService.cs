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

        public UsuarioService(
            IUsuarioRepository usuariosRepository,
            IAuthenticatedUser user,
            IUsuarioLocalRepository usuariosLocalRepository,
            ILocalRepository locaisRepository,
            IConfiguration config,
            IEmailService email,
            IUnitOfWork unitOfWork
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
        }

        public async Task<List<UsuarioResponseDto>> Show()
        {
            if (_user.IsSuperAdmin())
                return await _usuariosRepository.Show();

            // Restrito ao Local ativo do Administrador — evita vazamento
            // de usuários entre tenants.
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

            // Garante que o usuário buscado pertence ao mesmo tenant do
            // Administrador logado (evita IDOR entre Locais). Devolve 404
            // em vez de 403 para não confirmar a existência do usuário
            // fora do escopo do tenant atual — leitura não altera
            // privilégio, então não aplica a proteção do Administrador Pai.
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

        public async Task<UsuarioResponseDto> Create(UsuarioRequestDto dto)
        {
            var userByEmail = await _usuariosRepository.GetByEmail(dto.Email);
            if (userByEmail != null)
                throw new InvalidOperationException("Existe um usuario com o email cadastrado");

            var usuarioLogado = await _user.GetCurrentUser();

            //Somente um usuario com role SuperAdmin pode cadastrar outro usuario com role SuperAdmin
            await ValidarPermissaoCadastro(usuarioLogado, dto);

            // Criando um novo usuário com os dados fornecidos no DTO
            var newUsuario = CriarUsuario(dto);

            // Usuario e UsuarioLocal são duas chamadas de repositório
            // independentes (cada uma com seu próprio SaveChangesAsync) —
            // sem transação, uma falha na segunda deixava o Usuario já
            // commitado sozinho, sem nenhum vínculo com Local (órfão).
            await _unitOfWork.ExecuteAsync(async () =>
            {
                await _usuariosRepository.Create(newUsuario);
                await CriarUsuarioLocal(newUsuario, dto, usuarioLogado);
            });

            return new UsuarioResponseDto
            {
                Id = newUsuario.Id,
                Nome = newUsuario.Nome,
                Email = newUsuario.Email ?? string.Empty,
                Cpf = newUsuario.Cpf ?? string.Empty,
                Role = newUsuario.Role,
                CreatedAt = newUsuario.CreatedAt,
                LocalId = newUsuario.LocalId,
                Perfil = dto.Perfil ?? Perfil.Administrador,
            };
        }

        public async Task<UsuarioResponseDto> Update(Guid id, UsuarioRequestUpdateDto dto)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException($"O id informado não pode ser vazio {nameof(id)} .");
            }

            var usuario = await _usuariosRepository.GetByIdDto(id);
            if (usuario == null)
            {
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");
            }

            var getPassword = await _usuariosRepository.GetPasswordById(id);

            var usuarioLogado = await _user.GetCurrentUser();
            if (dto.Role.HasValue)
            {
                await ValidarPermissaoCadastro(
                    usuarioLogado,
                    new UsuarioRequestDto { Role = dto.Role.Value }
                );
            }

            await GarantirUsuarioNoTenantAtual(id, protegerAdministradorPai: dto.Role.HasValue);

            var roleMudou = dto.Role.HasValue && dto.Role.Value != usuario.Role;

            var usuarioDb = new Usuario
            {
                Id = id,
                Nome = !string.IsNullOrEmpty(dto.Nome) ? dto.Nome : usuario.Nome,
                Email = !string.IsNullOrEmpty(dto.Email) ? dto.Email : usuario.Email,
                SenhaHash = !string.IsNullOrEmpty(dto.SenhaHash)
                    ? BCrypt.Net.BCrypt.HashPassword(dto.SenhaHash)
                    : getPassword?.SenhaHash,
                Role = dto.Role ?? usuario.Role,
                SecurityStamp = roleMudou
                    ? Guid.NewGuid().ToString()
                    : (getPassword?.SecurityStamp ?? Guid.NewGuid().ToString()),
            };

            await _usuariosRepository.Update(id, usuarioDb);

            return new UsuarioResponseDto
            {
                Id = usuarioDb.Id,
                Nome = usuarioDb.Nome,
                Email = usuarioDb.Email,
                CreatedAt = usuario.CreatedAt,
                Role = usuarioDb.Role,
            };
        }

        public async Task Delete(Guid id)
        {
            var usuarioDb = await _usuariosRepository.GetById(id);
            if (usuarioDb == null)
            {
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");
            }

            // Mesma proteção de tenant do GetById/Update. Excluir é sempre
            // uma ação sobre o vínculo, então aqui a proteção do
            // Administrador Pai vale sempre.
            await GarantirUsuarioNoTenantAtual(id, protegerAdministradorPai: true);

            await _usuariosRepository.Delete(id);
        }

        // Garante que o usuário-alvo (id) está vinculado, com vínculo ATIVO,
        // ao Local em que o Administrador logado está atuando no momento.
        // Quando protegerAdministradorPai=true, também bloqueia a ação se o
        // vínculo pertencer ao Administrador Pai (só o SuperAdmin pode
        // alterar privilégio/excluir esse vínculo). SuperAdmin sempre passa
        // sem restrição.
        private async Task GarantirUsuarioNoTenantAtual(
            Guid usuarioAlvoId,
            bool protegerAdministradorPai
        )
        {
            if (_user.IsSuperAdmin())
                return;

            var localAtivo = _user.GetLocalId();
            if (localAtivo == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Selecione um Local ativo para gerenciar usuários."
                );

            var vinculo = await _usuariosLocalRepository.Vinculo(usuarioAlvoId, localAtivo);

            // Não confirmamos a existência do usuário fora do tenant atual:
            // 404 igual ao caso de "não existe" (evita enumeração de Ids de
            // outros tenants).
            if (vinculo == null)
                throw new KeyNotFoundException($"Usuario com id {usuarioAlvoId} não encontrado");

            if (protegerAdministradorPai && vinculo.EhAdministradorPai)
                throw new UnauthorizedAccessException(
                    "Somente o SuperAdmin pode alterar ou remover o Administrador responsável por este Local."
                );
        }

        private async Task ValidarPermissaoCadastro(
            CurrentUser usuarioLogado,
            UsuarioRequestDto dto
        )
        {
            // Somente um usuario com role SuperAdmin pode cadastrar outro usuario com role SuperAdmin
            if (usuarioLogado.Role != Role.SuperAdmin && dto.Role == Role.SuperAdmin)
                throw new InvalidOperationException(
                    "Você não tem permissão para esse cadastro, contate um administrador"
                );
        }

        private static Usuario CriarUsuario(UsuarioRequestDto dto)
        {
            return new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Cpf = dto.Cpf,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.SenhaHash),
                Role = dto.Role,
            };
        }

        private async Task CriarUsuarioLocal(
            Usuario usuarioCriado,
            UsuarioRequestDto dto,
            CurrentUser usuarioLogado
        )
        {
            // Um SuperAdmin cria cada entidade (Local, Usuário, etc.) de forma
            // independente e faz o vínculo entre elas depois — por isso o
            // UsuarioLocal nasce "pendente" (LocalId nulo), a menos que o
            // SuperAdmin já informe um LocalId de propósito no cadastro.
            // Isso vale mesmo que o SuperAdmin esteja no momento com um Local
            // ativo (trocado via SwitchLocal): ele administra TODOS os
            // locais, então não deve herdar automaticamente o local em que
            // está navegando no momento.
            if (usuarioLogado.Role == Role.SuperAdmin)
            {
                if (dto.LocalId.HasValue)
                    await GarantirLocalExiste(dto.LocalId.Value);

                var perfilAtribuido = dto.Perfil ?? Perfil.Administrador;
                var novoUsuarioLocal = new UsuarioLocal
                {
                    UsuarioId = usuarioCriado.Id,
                    LocalId = dto.LocalId ?? null,
                    Perfil = perfilAtribuido,
                    // O SuperAdmin pode "criar um Local e vinculá-lo
                    // diretamente a um Administrador" (regra de negócio) —
                    // quando isso acontece já no cadastro (LocalId + Perfil
                    // Administrador informados juntos), esse vínculo nasce
                    // como o Administrador Pai daquele Local.
                    EhAdministradorPai =
                        dto.LocalId.HasValue && perfilAtribuido == Perfil.Administrador,
                };

                await _usuariosLocalRepository.Create(novoUsuarioLocal);
                return;
            }

            // Um Administrador comum só gerencia usuários dentro do seu
            // próprio Local — o novo usuário herda automaticamente o Local
            // de quem o está criando (nunca de um LocalId arbitrário vindo
            // do DTO, para não vazar usuários entre locais diferentes).
            if (usuarioLogado.LocalId == null)
                throw new InvalidOperationException(
                    "Você precisa estar vinculado a um Local para cadastrar usuários."
                );

            var usuarioLocal = new UsuarioLocal
            {
                UsuarioId = usuarioCriado.Id,
                LocalId = usuarioLogado.LocalId,
                Perfil = dto.Perfil ?? Perfil.Visualizador,
                // Mesmo que o Administrador logado crie outro Admin (Admin
                // Filho — permitido pela regra de negócio), o novo vínculo
                // NUNCA nasce como Administrador Pai: só o SuperAdmin ou o
                // fluxo de auto-vínculo em LocalService.VincularCriadorAoNovoLocal
                // podem originar um Administrador Pai.
                EhAdministradorPai = false,
            };

            await _usuariosLocalRepository.Create(usuarioLocal);
        }

        private async Task GarantirLocalExiste(Guid localId)
        {
            var local = await _locaisRepository.GetById(localId);
            if (local == null)
                throw new KeyNotFoundException($"Local com id {localId} não encontrado.");
        }

        // Listar dados do perfil logado
        public async Task<UsuarioResponseDto?> GetMeuPerfil()
        {
            var usuarioLogado = await _user.GetCurrentUser();
            if (usuarioLogado == null)
                return null;

            var userId = usuarioLogado.UserId;
            if (userId == null)
                return null;

            var usuarioDb = await _usuariosRepository.GetById(userId.Value);

            if (usuarioDb == null)
                return null;

            return new UsuarioResponseDto
            {
                Id = usuarioDb.Id,
                Nome = usuarioDb.Nome,
                Email = usuarioDb.Email ?? string.Empty,
                Cpf = usuarioDb.Cpf ?? string.Empty,
                Role = usuarioDb.Role,
                CreatedAt = usuarioDb.CreatedAt,
                LocalId = usuarioDb.LocalId,
            };
        }

        // Atualizar dados do perfil logado
        public async Task<UsuarioResponseDto> UpdateMyProfileAsync(UsuarioRequestUpdateDto dto)
        {
            var usuarioLogado = await _user.GetCurrentUser();
            if (usuarioLogado == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            var userId = usuarioLogado.UserId;
            if (userId == null)
                throw new KeyNotFoundException("ID do usuário não encontrado.");

            var usuarioDb = await _usuariosRepository.GetById(userId.Value);
            if (usuarioDb == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            if (!string.IsNullOrEmpty(dto.Email))
            {
                var emailJaExiste = await _usuariosRepository.GetByEmail(dto.Email);
                if (emailJaExiste != null && emailJaExiste.Id != usuarioDb.Id)
                    throw new InvalidOperationException(
                        "Já existe um usuário com este e-mail cadastrado."
                    );
            }

            if (
                !string.IsNullOrWhiteSpace(dto.Email)
                && !dto.Email.Equals(usuarioDb.Email, StringComparison.OrdinalIgnoreCase)
            )
            {
                usuarioDb.SecurityStamp = Guid.NewGuid().ToString(); // Atualiza o SecurityStamp para invalidar tokens antigos
            }

            // Atualizar os dados do usuário
            var newUsuario = new Usuario
            {
                Nome = !string.IsNullOrEmpty(dto.Nome) ? dto.Nome : usuarioDb.Nome,
                Email = !string.IsNullOrEmpty(dto.Email) ? dto.Email : usuarioDb.Email,
                Cpf = !string.IsNullOrEmpty(dto.Cpf) ? dto.Cpf : usuarioDb.Cpf,
                Role = usuarioDb.Role, // O usuário não pode alterar sua própria role
                SecurityStamp = usuarioDb.SecurityStamp, // Mantém o SecurityStamp atual, a menos que o e-mail seja alterado
            };

            await _usuariosRepository.Update(userId.Value, newUsuario);

            return new UsuarioResponseDto
            {
                Id = usuarioDb.Id,
                Nome = usuarioDb.Nome,
                Email = usuarioDb.Email ?? string.Empty,
                Cpf = usuarioDb.Cpf ?? string.Empty,
                LocalId = usuarioDb.LocalId,
            };
        }

        public async Task<string?> PasswordResetToken(string email)
        {
            var usuario = await _usuariosRepository.GetByEmail(email);
            if (usuario == null)
                throw new KeyNotFoundException($"Usuário com email {email} não encontrado.");

            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert
                .ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('='); // base64url

            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuario.Id,
                TokenHash = TokenHasher.Hash(token),
                ExpiraEm = DateTime.UtcNow.AddHours(1),
            };

            await _usuariosRepository.PasswordResetToken(resetToken);

            var link = $"{_config["Frontend:BaseUrl"]}/redefinir-senha?token={token}";

            return link;
        }

        public async Task UpdatePasswordResetToken(RedefinirSenhaRequestDto data)
        {
            var hash = TokenHasher.Hash(data.Token);

            var resetToken = await _usuariosRepository.GetPasswordResetToken(hash);

            // Token inválido/expirado/já usado deve falhar explicitamente,
            // não seguir adiante silenciosamente.
            if (
                resetToken is null
                || resetToken.UsadoEm is not null
                || resetToken.ExpiraEm < DateTime.UtcNow
            )
                throw new InvalidOperationException("Link inválido ou expirado.");

            if (!ValidarForcaSenha(data.NovaSenha, out var motivo))
                throw new InvalidOperationException($"Senha inválida: {motivo}");

            resetToken.Usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(data.NovaSenha);
            resetToken.Usuario.SecurityStamp = Guid.NewGuid().ToString(); // Atualiza o SecurityStamp para invalidar tokens antigos
            resetToken.UsadoEm = DateTime.UtcNow;

            // Duas escritas independentes — sem transação, se a segunda
            // falhar depois que a senha já mudou, o token de reset fica
            // sem ser marcado como usado e continua válido pra reuso.
            await _unitOfWork.ExecuteAsync(async () =>
            {
                await _usuariosRepository.Update(resetToken.Usuario.Id, resetToken.Usuario);
                await _usuariosRepository.UpdatePasswordResetToken(resetToken);
            });
        }

        public async Task EsqueciSenha(EsqueciSenhaRequestDto dto)
        {
            var usuario = await _usuariosRepository.GetByEmail(dto.Email);
            if (usuario is null)
                return;

            var linkPasswordResetToken = await PasswordResetToken(usuario.Email);

            await _email.EnviarRedefinicaoSenhaAsync(
                usuario.Email,
                usuario.Nome,
                linkPasswordResetToken
            );
        }

        public async Task<PasswordResetToken?> GetPasswordResetTokenByHash(string tokenHash)
        {
            var token = await _usuariosRepository.GetPasswordResetToken(tokenHash);
            return token;
        }

        public async Task<ConviteResponseDto> CriarConvite(ConviteRequestDto dto)
        {
            var existente = await _usuariosRepository.GetByEmail(dto.Email);
            if (existente != null)
                throw new InvalidOperationException("Já existe um usuário com este e-mail.");

            var usuarioLogado = await _user.GetCurrentUser();

            // Mesma regra do cadastro direto: só um SuperAdmin pode convidar
            // outro SuperAdmin (ver ValidarPermissaoCadastro).
            if (usuarioLogado.Role != Role.SuperAdmin && dto.Role == Role.SuperAdmin)
                throw new InvalidOperationException(
                    "Você não tem permissão para esse convite, contate um administrador"
                );

            Guid? localId;
            Perfil perfilAtribuido;
            var criadoPorSuperAdmin = usuarioLogado.Role == Role.SuperAdmin;

            if (criadoPorSuperAdmin)
            {
                // Mesma regra de CriarUsuarioLocal: o SuperAdmin decide
                // livremente. LocalId pode ficar em aberto — o convidado
                // vira Administrador sem Local e, ao aceitar o convite e
                // logar, cai no fluxo de criar o primeiro Local (PrimeiroLocal.jsx).
                if (dto.LocalId.HasValue)
                    await GarantirLocalExiste(dto.LocalId.Value);

                localId = dto.LocalId;
                perfilAtribuido = dto.Perfil ?? Perfil.Administrador;
            }
            else
            {
                // Um Administrador só convida gente pro seu próprio Local —
                // nunca aceita um LocalId arbitrário vindo do request (mesma
                // proteção contra vazamento entre tenants de CriarUsuarioLocal).
                if (usuarioLogado.LocalId == null)
                    throw new InvalidOperationException(
                        "Você precisa estar vinculado a um Local para convidar usuários."
                    );

                localId = usuarioLogado.LocalId;
                perfilAtribuido = dto.Perfil ?? Perfil.Visualizador;
            }

            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert
                .ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            var convite = new ConviteToken
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                Role = dto.Role,
                Perfil = perfilAtribuido,
                LocalId = localId,
                CriadoPorId = usuarioLogado.UserId ?? Guid.Empty,
                CriadoPorSuperAdmin = criadoPorSuperAdmin,
                TokenHash = TokenHasher.Hash(token),
                ExpiraEm = DateTime.UtcNow.AddHours(48),
            };

            await _usuariosRepository.CriarConvite(convite);

            var link = $"{_config["Frontend:BaseUrl"]}/completar-cadastro?token={token}";

            // Diferente do "esqueci senha" (onde escondemos falha de e-mail
            // por segurança, pra não confirmar quais e-mails existem), aqui
            // é uma ação explícita do Admin — faz sentido ele saber que o
            // e-mail não saiu, então deixamos a exceção subir pro controller.
            await _email.EnviarConviteAsync(dto.Email, link);

            return new ConviteResponseDto { Email = dto.Email, ExpiraEm = convite.ExpiraEm };
        }

        public async Task CompletarConvite(CompletarConviteRequestDto dto)
        {
            var hash = TokenHasher.Hash(dto.Token);
            var convite = await _usuariosRepository.GetConviteByHash(hash);

            if (
                convite is null
                || convite.UsadoEm is not null
                || convite.ExpiraEm < DateTime.UtcNow
            )
                throw new InvalidOperationException("Convite inválido ou expirado.");

            // Alguém pode ter se cadastrado com este e-mail por outro caminho
            // entre o convite ser criado e ser aceito — checagem de corrida.
            var existente = await _usuariosRepository.GetByEmail(convite.Email);
            if (existente != null)
                throw new InvalidOperationException("Já existe um usuário com este e-mail.");

            if (!ValidarForcaSenha(dto.Senha, out var motivo))
                throw new InvalidOperationException($"Senha inválida: {motivo}");

            if (!dto.AceiteTermos)
                throw new InvalidOperationException(
                    "É necessário aceitar os Termos de Uso e a Política de Privacidade para concluir o cadastro."
                );

            var novoUsuario = new Usuario
            {
                Nome = dto.Nome,
                Email = convite.Email,
                Cpf = dto.Cpf,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                Role = convite.Role,
                TermosAceitosVersao = LegalConstants.VersaoTermosAtual,
                TermosAceitosEm = DateTimeOffset.UtcNow,
            };

            // Três escritas independentes em sequência — sem transação, uma
            // falha no meio do caminho deixa o Usuario órfão (sem
            // UsuarioLocal) e/ou o convite sem ser marcado como usado
            // (permitindo reaproveitar o mesmo link depois).
            await _unitOfWork.ExecuteAsync(async () =>
            {
                await _usuariosRepository.Create(novoUsuario);

                var usuarioLocal = new UsuarioLocal
                {
                    UsuarioId = novoUsuario.Id,
                    LocalId = convite.LocalId,
                    Perfil = convite.Perfil,
                    // Só reproduz Administrador Pai quando o convite (a) veio de
                    // um SuperAdmin e (b) já tinha LocalId + Perfil Administrador
                    // definidos juntos — mesma condição de CriarUsuarioLocal.
                    EhAdministradorPai =
                        convite.CriadoPorSuperAdmin
                        && convite.LocalId.HasValue
                        && convite.Perfil == Perfil.Administrador,
                };

                await _usuariosLocalRepository.Create(usuarioLocal);

                convite.UsadoEm = DateTime.UtcNow;
                await _usuariosRepository.UpdateConvite(convite);
            });
        }

        // Usado pelo gate de aceite pós-login, para contas que existiam antes
        // desse recurso (ex.: usuário seed) e por isso nunca passaram pelo
        // fluxo de convite/CompletarConvite, onde o aceite normalmente é
        // registrado.
        public Task AceitarTermos(Guid usuarioId) =>
            _usuariosRepository.AceitarTermos(usuarioId, LegalConstants.VersaoTermosAtual);

        private static bool ValidarForcaSenha(string senha, out string motivo)
        {
            if (senha.Length < 8)
            {
                motivo = "A senha precisa ter ao menos 8 caracteres.";
                return false;
            }
            if (!senha.Any(char.IsUpper))
            {
                motivo = "A senha precisa de uma letra maiúscula.";
                return false;
            }
            if (!senha.Any(char.IsDigit))
            {
                motivo = "A senha precisa de um número.";
                return false;
            }
            motivo = "";
            return true;
        }

        public async Task<Usuario?> ObterOuVincularPorEmailGoogleAsync(string email)
        {
            var usuarioExistente = await _usuariosRepository.GetByEmail(email);
            if (usuarioExistente is null)
                return null;

            if (!usuarioExistente.LoginGoogle)
                await _usuariosRepository.MarcarLoginGoogle(usuarioExistente.Id);

            return usuarioExistente;
        }

        public async Task<bool> ExisteConviteAtivoAsync(string email)
        {
            var convite = await _usuariosRepository.GetConviteAtivoByEmail(email);
            return convite is not null;
        }

        public async Task<Usuario?> CompletarConviteGoogleAsync(
            string email,
            string nome,
            string? cpf,
            bool aceiteTermos
        )
        {
            // Corrida: o usuário pode ter completado o cadastro por outro caminho
            // (link do convite por e-mail) entre a checagem em AutenticarAsync e
            // esta chamada — trata como login normal em vez de duplicar conta.
            // Não exige aceite aqui: quem já tem conta já aceitou antes.
            var usuarioExistente = await _usuariosRepository.GetByEmail(email);
            if (usuarioExistente != null)
            {
                if (!usuarioExistente.LoginGoogle)
                    await _usuariosRepository.MarcarLoginGoogle(usuarioExistente.Id);

                return usuarioExistente;
            }

            if (!aceiteTermos)
                throw new InvalidOperationException(
                    "É necessário aceitar os Termos de Uso e a Política de Privacidade para concluir o cadastro."
                );

            var convite = await _usuariosRepository.GetConviteAtivoByEmail(email);
            if (convite is null)
                return null; // convite expirou/foi usado nesse intervalo

            var novoUsuario = new Usuario
            {
                Nome = nome,
                Email = email,
                Cpf = cpf,
                SenhaHash = null,
                LoginGoogle = true,
                Role = convite.Role,
                SecurityStamp = Guid.NewGuid().ToString(),
                TermosAceitosVersao = LegalConstants.VersaoTermosAtual,
                TermosAceitosEm = DateTimeOffset.UtcNow,
            };

            // Mesmo gap do CompletarConvite por e-mail: três escritas em
            // sequência sem transação.
            await _unitOfWork.ExecuteAsync(async () =>
            {
                await _usuariosRepository.Create(novoUsuario);

                var usuarioLocal = new UsuarioLocal
                {
                    UsuarioId = novoUsuario.Id,
                    LocalId = convite.LocalId,
                    Perfil = convite.Perfil,
                    EhAdministradorPai =
                        convite.CriadoPorSuperAdmin
                        && convite.LocalId.HasValue
                        && convite.Perfil == Perfil.Administrador,
                };

                await _usuariosLocalRepository.Create(usuarioLocal);

                convite.UsadoEm = DateTime.UtcNow;
                await _usuariosRepository.UpdateConvite(convite);
            });

            return novoUsuario;
        }
    }
}
