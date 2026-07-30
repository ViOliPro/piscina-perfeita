# Correções de Autenticação, Autorização e Multi-tenancy — PiscinaPerfeita.Api

Este documento resume a análise e as correções aplicadas ao backend em relação
às regras de negócio de hierarquia de acesso (SuperAdmin / Administrador /
Operador / Visualizador) e ao isolamento multi-tenant por `LocalId`.

## 1. Problemas FATAIS corrigidos (o projeto não rodava)

### 1.1 Classe duplicada (erro de compilação CS0101)
`Example/UsuariosController.Example.cs` declarava
`PiscinaPerfeita.Api.Controllers.UsuariosController` — mesmo namespace e nome
do controller real em `Controllers/UsuariosController.cs`. Isso impedia o
projeto de compilar.
**Ação:** arquivo de exemplo removido.

### 1.2 Crash garantido na primeira requisição autorizada
Em `Authorization/Policies.cs`, a constante `GerenciarUsuarioLocal` tinha
exatamente o mesmo valor de string que `GerenciarUsuario`
(`"Perfil:GerenciarUsuario"`). O `Dictionary<string, Perfil[]>` estático em
`PerfilPolicyProvider.Mapa` é inicializado com um dicionário-literal contendo
as duas chaves — como eram idênticas, o runtime lançava
`ArgumentException: An item with the same key has already been added` na
primeira vez que qualquer policy fosse resolvida (ou seja, na primeira
requisição autenticada de qualquer endpoint da API).
**Ação:** `GerenciarUsuarioLocal` agora vale `"Perfil:GerenciarUsuarioLocal"`.

### 1.3 Arquitetura de tenant duplicada e morta
Existiam DOIS sistemas paralelos e não-conectados para autenticação/tenant:

| | Sistema real (usado) | Sistema morto (removido) |
|---|---|---|
| DbContext | `PiscinaPerfeitaContext` (registrado em `Program.cs`) | `AppDbContext` (nunca registrado no DI) |
| Usuário atual | `IAuthenticatedUser` / `AuthenticatedUser` | `Services.ICurrentUserService` / `CurrentUserService` |
| Marcador de entidade | `IBelongsToLocal` | `ITenantScoped` |
| Filtro | Local ativo único por sessão (claim `local_id` do JWT) | Lista de `LocalIds` (nunca usada de verdade) |

O sistema morto (`Data/AppDbContext.TenantFilters.cs`,
`Service/CurrentUserService.cs`, `Service/ICurrentUserService.cs`,
`Models/Interfaces/ICurrentUserService.cs`,
`Middleware/ValidarLocalIdAttribute.cs`) não estava registrado em nenhum
lugar do `DependencyInjectionConfig`/`Program.cs` — era código morto que só
gerava confusão (e, junto com o item 1.1, quebrava o build).
**Ação:** todos esses arquivos foram removidos. A arquitetura real
(`PiscinaPerfeitaContext` + `IAuthenticatedUser` + `IBelongsToLocal`) foi
mantida como única fonte de verdade.

## 2. Vulnerabilidade crítica: vazamento de dados entre tenants (Usuarios)

O `Global Query Filter` por `LocalId` já existia e funciona corretamente para
as entidades "filhas" (`Produto`, `Estoque`, `Piscina`, `Deposito`, `Analise`,
`MovimentacaoEstoque`, `AplicacaoProduto` — todas implementam
`IBelongsToLocal`). **Porém `Usuario` não implementa `IBelongsToLocal`** (o
vínculo é N:N via `UsuarioLocal`, não um `LocalId` direto), e por isso ficou
de fora de qualquer isolamento:

- `UsuarioRepository.FilterRoleUsuario()` só filtrava `Role == Role.Usuario`
  (excluindo SuperAdmins) — **sem nenhum filtro por Local**. Qualquer
  Administrador via a lista de usuários comuns de **todos os tenants**.
- `UsuarioService.GetById/Update/Delete` não validavam se o usuário-alvo
  pertencia ao Local do Administrador logado — um IDOR clássico: sabendo o
  Guid de um usuário de **qualquer outro condomínio/tenant**, um
  Administrador conseguia consultar, editar ou **apagar** esse usuário.
  (O comentário original no `Delete` do controller já reconhecia esse risco,
  mas nada no `Service` o impedia de fato.)

**Por que não usei um Global Query Filter automático para `Usuario` (como
sugeria o item 3 do pedido)?** Porque `Usuario` e `UsuarioLocal` são
consultados **antes** de existir um JWT válido (ex.: login busca o usuário
pelo e-mail, e depois busca todos os seus vínculos para decidir o Local
ativo). Um filtro automático baseado no Local "ativo" quebraria o próprio
login (dependência circular: precisa consultar `UsuarioLocal` sem local
ativo para descobrir qual será o local ativo). Por isso a correção foi feita
como **validação explícita no Service**, exatamente como já era feito nas
entidades filhas via o mecanismo de `EnsureLocalAccess`, mas encaixado na
arquitetura real do projeto:

- `IUsuarioRepository.FilterRoleUsuario(Guid localId)` agora exige vínculo
  ativo (`UsuariosLocais.Any(ul => ul.LocalId == localId && ul.Ativo)`).
- `UsuarioService.Show()` passa o Local ativo do Administrador logado.
- `UsuarioService.GetById/Update/Delete` chamam um novo helper
  `GarantirUsuarioNoTenantAtual` que:
  - deixa passar direto se `IsSuperAdmin()`;
  - senão, exige um vínculo ativo do usuário-alvo no Local ativo do
    chamador (senão, `404` — nunca `403`, para não confirmar a existência
    do registro fora do tenant);
  - opcionalmente também bloqueia a ação se o vínculo-alvo for o do
    "Administrador Pai" (ver seção 3).
- Foi adicionada a navegação `Usuario.UsuariosLocais` (faltava no modelo)
  para viabilizar essa consulta.

## 3. Regra do "Administrador Pai" — não existia no modelo

A regra pede: *"[o SuperAdmin] é o ÚNICO que pode alterar ou remover os
privilégios e vínculos de um Administrador Pai (original)"* e *"O
Administrador Pai pode alterar, gerenciar ou remover privilégios e vínculos
de seus usuários filhos"*.

Não havia **nenhum campo** no banco para diferenciar o Administrador
original ("Pai") de um Admin criado depois ("Filho") — tornando essa regra
impossível de aplicar.

**Ação:** adicionado `UsuarioLocal.EhAdministradorPai` (bool, default
`false`), setado automaticamente como `true` apenas em dois pontos:

1. `LocalService.VincularCriadorAoNovoLocal` — quando um Administrador cria
   seu próprio Local (self-onboarding / "PrimeiroLocal").
2. `UsuarioService.CriarUsuarioLocal` — quando o **SuperAdmin** cadastra um
   usuário já vinculando-o diretamente a um Local com `Perfil.Administrador`
   (regra: *"Pode criar um Local e vinculá-lo diretamente a um
   Administrador"*).

Em qualquer outro caso (inclusive quando um Administrador cria outro Admin —
"Admin Filho", permitido pela regra) o vínculo nasce com
`EhAdministradorPai = false`. O campo **não existe no request DTO**, então
nunca é setável diretamente pela API.

Com o campo em vigor, os pontos de escrita agora bloqueiam ações de um
Administrador comum contra o vínculo do Administrador Pai:

- `UsuarioService.Update`: só protege quando a chamada tenta alterar `Role`
  (privilégio) — edição de nome/e-mail/senha continua liberada.
- `UsuarioService.Delete`: sempre protegido.
- `UsuarioLocalService` (ver seção 4): `Update`/`Delete` de um vínculo
  marcado como Pai sempre exigem SuperAdmin.

**Pendente:** é necessário gerar a migration do EF Core para a nova coluna
(não foi possível gerar aqui — ver seção 6).

## 4. `UsuarioLocalService` estava travado só para SuperAdmin

Toda a gestão de vínculos (`Create`/`Update`/`Delete` de `UsuarioLocal`)
exigia `SuperAdmin` (`GarantirSuperAdmin()`), o que contradizia a regra de
que o Administrador Pai deve poder gerenciar vínculos dos seus usuários
filhos.

**Ação:** reescrito para permitir que um Administrador gerencie vínculos,
mas com escopo estrito:

- `Create`/`Update`: o `LocalId` do dto é **ignorado** para quem não é
  SuperAdmin — o vínculo só pode ser criado/editado no Local ativo do
  próprio Administrador (nunca em outro tenant).
- `Update`/`Delete`: novo helper `GarantirPodeGerenciarVinculo` — exige que
  o vínculo pertença ao Local ativo do Administrador (senão `404`) e
  bloqueia qualquer ação sobre um vínculo `EhAdministradorPai == true`
  (só SuperAdmin).
- O campo `EhAdministradorPai` nunca é aceito vindo do request; em
  `Update`, o valor existente é sempre preservado (essa rota não cria nem
  remove o status de Admin Pai).
- `Show`/`GetById` (visão global, todos os tenants) continuam restritos a
  SuperAdmin — são as telas administrativas de "ver tudo".

Correção adicional: `UsuarioLocalRepository.Update` não persistia
`EhAdministradorPai` (o objeto passado era só copiado campo a campo, e o
campo novo não estava na lista) — corrigido.

## 5. Outras correções pontuais

- `UsuariosController.Delete`: comentário desatualizado dizia que o endpoint
  deveria ficar restrito a SuperAdmin "enquanto o escopo não é implementado
  no service" — atualizado para refletir que o `Service` agora valida tenant
  + Administrador Pai corretamente.
- `Dtos/Response/UsuarioLocalResponseDto`: exposto `EhAdministradorPai`
  (somente leitura) para o front poder desabilitar os botões de
  editar/excluir desse vínculo específico.

## 6. Pendências / próximos passos recomendados

1. **Gerar a migration** para a nova coluna
   `UsuarioLocal.EhAdministradorPai`:
   ```bash
   cd PiscinaPerfeita.Api/src
   dotnet ef migrations add AddEhAdministradorPaiToUsuarioLocal
   dotnet ef database update
   ```
   (Não foi possível gerar aqui: este ambiente não tem o SDK do .NET nem
   acesso ao NuGet para restaurar os pacotes do projeto.)
2. Revisar o front-end (`VinculosLocaisModal`, telas de gestão de usuário)
   para desabilitar edição/remoção quando `EhAdministradorPai === true`.
3. Considerar expor no front um indicador visual ("Administrador
   responsável") para esse vínculo.
4. Rodar uma suíte de testes manuais (ou automatizados) cobrindo os
   cenários de hierarquia descritos no pedido original, em especial:
   - Administrador tentando acessar/editar/excluir usuário de outro Local
     (deve dar 404).
   - Administrador tentando editar/excluir o vínculo do Administrador Pai
     (deve dar 401/403).
   - SuperAdmin continua com acesso irrestrito em todos os fluxos.
