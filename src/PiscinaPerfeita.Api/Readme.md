# 1 - Cadastro de usuário
## Atributos necessários
- nome (string)
- password (string) -> BCrypt
- email (string)
- role - (enum Admin = 0, Cliente = 1)


# 2 - Cadastro de Piscina
* _usuarioId_ (guid - uuid) -> 44629be7-5863-4054-8cb6-5ca242662dcb
- nome (string)
- volumeLitros (int)
- profundidadeMedia (int)

# 3 - Produto

# 4 - Analises 
* _piscinaId_ (guid - uuid) -> 97d00fb5-d9be-4242-b6b3-d36dd235a5ad
- ph (decimal)
- cloro (decimal) 
- alcalinidade (decimal)
- temperatura (decimal)
- observacoes (string)

# 5 -Estoque
- piscinaId
- produtoId

# 6 - Movimentação Estoque
- piscinaId
- produtoId