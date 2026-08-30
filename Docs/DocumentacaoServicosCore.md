# Documentação dos serviços principais da API

## Visão geral

Além do módulo de autenticação, a API organiza a lógica de negócio em serviços por domínio. Os serviços ficam em `PiscinaPerfeita.Api/src/Service` e atuam como camada intermediária entre os controladores e os repositórios, encapsulando regras de negócio, validações, permissões de tenant e transformações para DTOs.

Os principais domínios cobertos são:

- usuários;
- locais;
- piscinas;
- produtos;
- estoques;
- demais módulos operacionais como análises, depósitos, hidrometros e movimentações.

## Padrão de organização

Cada domínio normalmente segue o mesmo formato:

- interface de contrato, por exemplo `IUsuarioService`;
- implementação concreta, por exemplo `UsuarioService`;
- repositório de dados associado;
- DTOs de entrada e saída;
- uso de `IAuthenticatedUser` para identificar o usuário logado;
- validações de acesso por local, perfil e papel.

Essa estrutura torna a API previsível e facilita manutenção, testes e evolução.

## 1. Serviço de usuários

### Local de implementação

- `PiscinaPerfeita.Api/src/Service/Usuarios/IUsuarioService.cs`
- `PiscinaPerfeita.Api/src/Service/Usuarios/UsuarioService.cs`

### Responsabilidade

O serviço de usuários cuida de:

- listagem e consulta de usuários;
- criação e atualização de perfis;
- exclusão de contas;
- recuperação de senha;
- convite para cadastro;
- aceite de termos;
- autenticação via Google;
- controle de permissões por tenant e perfil.

### Operações principais

#### Show

Lista usuários visíveis ao usuário autenticado.

- `SuperAdmin` consegue ver todos.
- usuários comuns só veem usuários vinculados ao local ativo.

#### GetById

Busca um usuário por identificador e aplica proteção de tenant.

- evita acesso cruzado entre locais;
- retorna 404 quando o usuário alvo não está no mesmo escopo do administrador logado.

#### Create

Cria um usuário novo e também cria o vínculo `UsuarioLocal`.

Regras relevantes:

- apenas `SuperAdmin` pode criar outro `SuperAdmin`;
- um administrador comum cria usuários dentro do seu local atual;
- o `SuperAdmin` pode definir vínculo direto com um local específico ou deixar o vínculo pendente.

#### Update

Atualiza dados do usuário com cuidado de segurança.

- não permite que o usuário altere sua própria role;
- preserva `SenhaHash` caso não tenha sido enviada;
- reinvalida `SecurityStamp` quando o e-mail muda.

#### Delete

Exclui o usuário apenas após validar a pertença ao tenant atual.

- o administrador pai não pode ser removido ou alterado sem autorização do `SuperAdmin`.

#### Recuperação de senha e convite

O serviço gerencia dois fluxos de onboarding e segurança:

- `EsqueciSenha`: gera token, envia e-mail e permite redefinição da senha;
- `CriarConvite`: gera convite para criação de usuário por e-mail;
- `CompletarConvite`: conclui cadastro usando token de convite;
- `UpdatePasswordResetToken`: aplica nova senha e invalida o token usado.

#### Aceite de termos

A regra de negócio garante que usuários antigos sem registro de aceite possam concluir esse passo após login.

### Segurança e tenant

O serviço usa `IAuthenticatedUser` para descobrir:

- user id;
- local atual;
- papel do usuário;
- se o usuário é `SuperAdmin`.

Isso permite aplicar isolamento por local e impedir vazamento de dados entre condomínios.

## 2. Serviço de locais

### Local de implementação

- `PiscinaPerfeita.Api/src/Service/Locais/ILocalService.cs`
- `PiscinaPerfeita.Api/src/Service/Locais/LocalService.cs`

### Responsabilidade

Gerencia os condomínios ou unidades de negócio representados por `Local`.

### Operações principais

#### Show

- `SuperAdmin` consegue ver todos os locais;
- usuários comuns apenas veem os locais aos quais estão vinculados.

#### GetById

Valida acesso antes de retornar o local.

#### Create

Cria um novo local e, se necessário, vincula o criador ao local.

Regras importantes:

- `SuperAdmin` pode criar sem vínculo automático;
- administrador comum que cria um local recebe vínculo ao local criado;
- a criação e o vínculo são executados em transação para evitar órfãos no banco.

#### Update

Permite edição do local, mas restringe o acesso a usuários autorizados.

#### Delete

Exclui o local apenas quando o usuário tem poder para administrá-lo.

### Regras de negócio

Este serviço reforça a separação entre multi-tenant e gestão de permissões. O local não é apenas uma entidade de dados: ele define o escopo de acesso da operação.

## 3. Serviço de piscinas

### Local de implementação

- `PiscinaPerfeita.Api/src/Service/Piscinas/IPiscinaService.cs`
- `PiscinaPerfeita.Api/src/Service/Piscinas/PiscinaService.cs`

### Responsabilidade

Gerencia o cadastro e a manutenção de piscinas do sistema.

### Fluxo principal

- valida se o usuário responsável existe;
- cria a entidade `Piscina` com nome, volume, profundidade e vínculo a `Local`/usuário;
- atualiza os dados e persiste as mudanças.

### Observações

A lógica é simples, porém importante: a piscina representa a unidade operacional principal do cliente e deve estar sempre referenciada a um usuário e a um local relevante.

## 4. Serviço de produtos

### Local de implementação

- `PiscinaPerfeita.Api/src/Service/Produtos/IProdutosService.cs`
- `PiscinaPerfeita.Api/src/Service/Produtos/ProdtosService.cs`

### Responsabilidade

Controla o cadastro de produtos utilizados na operação do condomínio, como produtos químicos e insumos.

### Operações principais

- `Show`: lista produtos;
- `GetById`: busca por ID;
- `Create`: cria novo produto;
- `Update`: atualiza produto existente;
- `Delete`: remove produto.

### Observações

A classe de serviço mantém a regra de transformar o DTO de entrada em entidade do domínio e retornar uma versão de resposta pronta para a API.

## 5. Serviço de estoques

### Local de implementação

- `PiscinaPerfeita.Api/src/Service/Estoques/IEstoqueService.cs`
- `PiscinaPerfeita.Api/src/Service/Estoques/EstoqueService.cs`

### Responsabilidade

Gerencia o controle de nível de estoque dos produtos e a manutenção do saldo disponível em depósitos.

### Operações principais

#### Show

Lista estoques conforme um filtro de status.

Os estados possíveis são:

- baixo;
- normal;
- alerta;
- todos.

#### GetById

Busca um estoque específico.

#### Create

Valida:

- existência do produto;
- existência do depósito;
- consistência entre quantidade mínima e estoque ideal.

#### Update

Mantém o valor anterior quando campos opcionais não vieram preenchidos no DTO, evitando perda de dados por atualização parcial incompleta.

#### Delete

Exclui o registro após validação do ID existente.

### Regra de negócio importante

O serviço valida que:

- `EstoqueIdeal` deve ser maior que `QuantidadeMinima`;
- se ambos forem informados e o ideal não for maior que o mínimo, a operação é rejeitada.

Essa regra evita inconsistência operacional no cálculo de reposição.

## 6. Padrão geral da camada de serviço

Os serviços da API seguem uma abordagem bem definida:

1. validam a entrada e a autorização;
2. consultam o repositório correspondente;
3. aplicam regras de negócio;
4. montam entidades e DTOs;
5. retornam respostas padronizadas para o controller.

Esse padrão centraliza a lógica de negócio fora do controlador e mantém os endpoints mais enxutos.

## 7. Papel do `IAuthenticatedUser`

A classe `IAuthenticatedUser` é um ponto importante na arquitetura da API. Ela permite que os serviços obtêm o contexto do usuário logado sem depender do controller diretamente.

Isso é essencial para:

- verificar papel e perfil;
- identificar local ativo;
- garantir isolamento por tenant;
- bloquear ações fora do escopo do usuário autenticado.

## 8. Observações finais

A camada de serviço da API é o centro da regra de negócio do sistema. Ela não apenas persiste dados, mas também define:

- quem pode acessar cada recurso;
- como cada domínio se comporta;
- como o tenant e o local impactam as operações;
- como o sistema preserva integridade de dados e segurança.

Em outras palavras, os serviços transformam a lógica de negócio em comportamento real da aplicação, funcionando como a ponte entre a API HTTP e o banco de dados.
