using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Locais;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Repository.UsuariosLocal;

namespace PiscinaPerfeita.Api.Service.Account;

public class TokenService : ITokenService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUsuarioLocalRepository _usuarioLocalRepository;
    private readonly ILocalRepository _localRepository;
    private readonly IConfiguration _configuration;

    public TokenService(
        IUsuarioRepository usuarioRepository,
        IUsuarioLocalRepository usuarioLocalRepository,
        ILocalRepository localRepository,
        IConfiguration configuration
    )
    {
        _usuarioRepository =
            usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _usuarioLocalRepository =
            usuarioLocalRepository
            ?? throw new ArgumentNullException(nameof(usuarioLocalRepository));
        _localRepository =
            localRepository ?? throw new ArgumentNullException(nameof(localRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<AuthTokenResult> GerarTokenAsync(Usuario usuario)
    {
        if (usuario.Role == Role.SuperAdmin)
            return await EmitirVerTodosAsync(usuario);

        var (localId, perfil) = await ResolverVinculoPadraoAsync(usuario);
        await _usuarioRepository.UpdateUltimoLocal(usuario.Id, localId ?? Guid.Empty);

        return new AuthTokenResult(
            CriarToken(usuario, localId?.ToString() ?? string.Empty, perfil),
            localId ?? Guid.Empty,
            perfil
        );
    }

    public async Task<AuthTokenResult> GerarTokenParaLocalAsync(Usuario usuario, Guid? newLocalId)
    {
        if (newLocalId == null)
        {
            if (usuario.Role != Role.SuperAdmin)
                throw new ArgumentException("Informe o Local para o qual deseja trocar.");

            return await EmitirVerTodosAsync(usuario);
        }

        Perfil perfilAtivo;
        if (usuario.Role == Role.SuperAdmin)
        {
            var local = await _localRepository.GetById(newLocalId.Value);
            if (local == null)
                throw new KeyNotFoundException("Local não encontrado.");

            perfilAtivo = Perfil.Administrador;
        }
        else
        {
            var vinculo = await _usuarioLocalRepository.Vinculo(usuario.Id, newLocalId.Value);
            if (vinculo == null)
                throw new UnauthorizedAccessException(
                    "Você não tem permissão para acessar este condomínio/local."
                );

            perfilAtivo = vinculo.Perfil;
        }

        await _usuarioRepository.UpdateUltimoLocal(usuario.Id, newLocalId.Value);

        return new AuthTokenResult(
            CriarToken(usuario, newLocalId.Value.ToString(), perfilAtivo),
            newLocalId.Value,
            perfilAtivo
        );
    }

    private async Task<AuthTokenResult> EmitirVerTodosAsync(Usuario usuario)
    {
        await _usuarioRepository.UpdateUltimoLocal(usuario.Id, Guid.Empty);
        return new AuthTokenResult(
            CriarToken(usuario, Guid.Empty.ToString(), Perfil.Administrador),
            Guid.Empty,
            Perfil.Administrador
        );
    }

    private async Task<(Guid? LocalId, Perfil Perfil)> ResolverVinculoPadraoAsync(Usuario usuario)
    {
        var vinculos = await _usuarioLocalRepository.GetAllByUserId(usuario.Id);
        if (vinculos == null || vinculos.Count == 0)
            throw new KeyNotFoundException("Este usuário não está vinculado a nenhum local!");

        var vinculoAtivo =
            vinculos.FirstOrDefault(v => v.LocalId == usuario.UltimoLocalId)
            ?? vinculos.FirstOrDefault(v => v.LocalId == usuario.LocalId)
            ?? vinculos[0];

        return (vinculoAtivo.LocalId, vinculoAtivo.Perfil);
    }

    private string CriarToken(Usuario usuario, string stringLocalId, Perfil perfil)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key =
            _configuration["Jwt:Key"]
            ?? throw new ArgumentNullException("Chave JWT não configurada.");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, usuario.Role.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nome ?? string.Empty),
                    new Claim("local_id", stringLocalId),
                    new Claim("perfil", perfil.ToString()),
                    new Claim("security_stamp", usuario.SecurityStamp ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID Único do Token
                    new Claim(
                        JwtRegisteredClaimNames.Iat,
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                        ClaimValueTypes.Integer64
                    ), // Emissão
                }
            ),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256
            ),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
