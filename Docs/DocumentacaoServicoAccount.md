# Documentação do módulo de autenticação e sessão

## Visão geral

O módulo de autenticação da API fica em `PiscinaPerfeita.Api/src/Service/Account` e é responsável por:

- validar credenciais de login por e-mail e senha;
- emitir tokens JWT de acesso e refresh;
- rotacionar e revogar refresh tokens;
- resolver qual local o usuário está acessando;
- suportar autenticação via Google;
- controlar a troca de local e a sessão do usuário.

Este módulo atua como a camada de segurança da aplicação e centraliza as regras de autenticação, autorização e gestão da sessão antes das operações do restante da API.

## Estrutura do módulo

### Arquivos principais

- `AccountService.cs` — orquestra o fluxo de login, refresh, troca de local e revogação do refresh token.
- `IAccountService.cs` — contrato de autenticação do usuário.
- `TokenService.cs` — gera JWTs, resolve o local ativo, valida e rotaciona refresh tokens.
- `ITokenService.cs` — contrato para a geração e validação de tokens.
- `Google/GoogleAuthService.cs` — fluxo de autenticação Google e cadastro de convite.
- `Google/IGoogleAuthService.cs` — contrato para autenticação social.

## Responsabilidade de cada componente

### AccountService

A classe `AccountService` implementa o fluxo principal da conta do usuário.

#### Métodos

- `Login(AccountRequestDto request)`
  - valida os dados de entrada;
  - busca o usuário pelo e-mail;
  - verifica senha com BCrypt;
  - gera token de acesso e refresh via `ITokenService`;
  - retorna o payload de autenticação junto com o refresh token.

- `SwitchLocal(Guid userId, Guid? newLocalId)`
  - permite trocar o local ativo do usuário;
  - usa o usuário autenticado e o local de origem do token para validar a permissão;
  - emite novo JWT e refresh token para o novo contexto local.

- `Refresh(string rawRefreshToken)`
  - valida o refresh token;
  - gira a sessão e cria novo access token;
  - retorna dados do usuário e novo token de refresh.

- `RevogarRefreshToken(string rawRefreshToken)`
  - invalida imediatamente o token recebido.

#### Regras importantes

- login exige e-mail e senha preenchidos;
- senha inválida dispara `ArgumentException` com mensagem genérica para o cliente;
- usuário não encontrado ou sem senha retorna erro de autenticação;
- a resposta montada inclui o `UserResponseDto` com dados do usuário e o `LocalId` atual.

### TokenService

A classe `TokenService` é o núcleo da autenticação da aplicação. Ela gerencia a criação, validação e rotação da sessão.

#### Funcionalidades

- `GerarTokenAsync(Usuario usuario)`
  - garante um `SecurityStamp`;
  - resolve o local padrão do usuário;
  - atualiza o último local acessado;
  - gera um refresh token;
  - cria o JWT com claims de autenticação.

- `GerarTokenParaLocalAsync(Usuario usuario, Guid? newLocalId)`
  - aceita troca explícita de local;
  - para `SuperAdmin`, permite visão de todos os locais com `Guid.Empty`;
  - para usuários normais, exige vínculo ativo com o local solicitado;
  - também atualiza o último local e gera novo refresh token.

- `ValidarERotacionarRefreshTokenAsync(string rawToken)`
  - calcula o hash do refresh token;
  - busca o registro no repositório de refresh tokens;
  - rejeita token inexistente, revogado ou expirado;
  - marca o token como revogado e retorna o usuário dono da sessão.

- `RevogarRefreshTokenAsync(string rawToken)`
  - invalida o refresh token do usuário sem interromper a sessão ativa imediatamente.

#### Regras de negócio de local

- `SuperAdmin` usa o “modo ver todos” com `LocalId = Guid.Empty`;
- usuários comuns usam o `UltimoLocalId` ou o primeiro vínculo ativo como local padrão;
- a troca de local exige vínculo válido para o local pedido;
- o `perfil` do token pode variar conforme o vínculo do usuário com o local.

### GoogleAuthService

A classe `GoogleAuthService` encapsula a autenticação com Google.

#### Fluxo do login Google

1. valida o `idToken` Google;
2. verifica se o e-mail foi validado pelo provedor;
3. procura ou vincula o usuário pelo e-mail;
4. gera JWT e refresh token;
5. retorna um `AuthResult` com token e payload do usuário.

#### Fluxos disponíveis

- `AutenticarAsync(string idToken)`
  - autentica usuários já cadastrados ou vinculados ao Google;
  - identifica convites pendentes;
  - retorna erro caso o usuário não tenha acesso liberado.

- `CompletarCadastroAsync(string idToken, string? cpf, bool aceiteTermos)`
  - valida o Google token;
  - exige aceite dos termos para concluir o cadastro;
  - cria ou completa o convite do usuário;
  - gera token após a conclusão.

## Estrutura de autenticação

### Tokens emitidos

#### Access Token

- tipo: JWT;
- expira em 1 hora;
- contém claims como:
  - identificador do usuário;
  - e-mail;
  - role;
  - nome;
  - `local_id`;
  - `perfil`;
  - `security_stamp`;
  - `jti` e `iat`.

#### Refresh Token

- gerado como valor aleatório em Base64;
- armazenado com hash em repositório de refresh tokens;
- expira em 30 dias;
- usa rotação para reforçar segurança.

## Fluxo principal de autenticação

### Login com e-mail e senha

1. controller recebe `AccountRequestDto`;
2. `AccountService.Login` valida dados e usuário;
3. `BCrypt.Verify` confirma senha;
4. `ITokenService.GerarTokenAsync` gera access token + refresh token;
5. controller retorna `AccountResponseDto` e salva o refresh token em cookie.

### Refresh da sessão

1. cliente envia refresh token via cookie;
2. controller chama `AccountService.Refresh`;
3. `TokenService.ValidarERotacionarRefreshTokenAsync` valida e marca o token como revogado;
4. novo access token e refresh token são emitidos;
5. novo payload do usuário é devolvido ao cliente.

### Troca de local

1. o usuário chama o endpoint de troca de local;
2. `SwitchLocal` usa o `userId` do token autenticado para validar a autorização;
3. `TokenService.GerarTokenParaLocalAsync` verifica vínculo e perfil;
4. JWT e refresh token são regenerados para o novo contexto local.

## Segurança e boas práticas implementadas

- uso de `BCrypt.Net` para hash de senha;
- JWT assinado com chave simétrica via `SymmetricSecurityKey`;
- refresh tokens persistidos com hash e expiração;
- cookie `HttpOnly`, `Secure` e `SameSite=None` para o refresh token;
- controle por `Role` e `Perfil` para diferenciação de acessos;
- rotação do `SecurityStamp` quando necessário;
- validação de vínculo do usuário com o local antes de permitir acesso.

## Observações de arquitetura

O módulo de autenticação não fica isolado do restante da aplicação: ele depende de repositórios e serviços de usuários, locais e refresh token. Essa organização torna o fluxo de autenticação previsível e facilita manutenção de regras de negócio, especialmente em cenários de multi-tenancy, onde cada usuário pode ter acesso a um ou mais locais com perfis diferentes.

## Conclusão

O módulo `Account` é a base da segurança da API. Ele centraliza autenticação, geração de sessão, autorização por local e regras de accesso para usuários tradicionais e Google, garantindo que cada chamada posterior ao sistema opere com um contexto de usuário e local conhecido e validado.
