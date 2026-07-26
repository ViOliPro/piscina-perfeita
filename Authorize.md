## REGRAS DE AUTHORIZE UTILIZADO PELA MAIORIA DOS CRUDS

### Controllers

-Analises, AplicacoesProduto, Deposito, Estoque, Movimentacoes, Produtos

- GET - (Listar) - Todos os perfis
- POST - (Cadastrar) - Administrador, Operador
- PUT - (Editar) - Administrador, Operador
- DELETE - (Deletar) - Administrador

## POLICY DEPOSITO

- GET - (Listar) - Todos os perfis
- POST - (Cadastrar) - Administrador, Operador
- PUT - (Editar) - Administrador
- DELETE - (Deletar) - Administrador

## POLICY LOCAL

- GET - (Listar) - Administrador
- POST - (Cadastrar) - Administrador
- PUT - (Editar) - Administrador
- DELETE - (Deletar) - Administrador

## POLICY USUARIO

- GET - (Listar) - Administrador
- POST - (Cadastrar) - Administrador
- PUT - (Editar) - Administrador
- DELETE - (Deletar) - Administrador

## POLICY USUARIOLOCAL

- GET - (Listar) - Administrador
- POST - (Cadastrar) - Administrador
- PUT - (Editar) - Administrador
- DELETE - (Deletar) - Administrador
