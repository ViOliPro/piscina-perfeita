namespace PiscinaPerfeita.Api.Authorization
{
    public static class Policies
    {
        // Policies de Role (as que já existem no AddAuthorization)
        public const string SuperAdminOnly = "SuperAdminOnly";
        public const string User = "User";
        public const string UserOrSuper = "UserOrSuper";

        // Policies de Perfil — nome da CAPACIDADE/AÇÃO, não da lista de perfis
        // Configuracao padrão usada nos cruds em geral
        // Consulte o controller correspondente
        // CRUD DE USO GERAL
        public const string Listar = "Perfil:ListarTudo";
        public const string Cadastrar = "Perfil:Cadastrar";
        public const string Editar = "Perfil:Editar";
        public const string Deletar = "Perfil:Deletar";

        //POLICY DEPOSITO
        public const string GerenciarDeposito = "Perfil:GerenciarDeposito";
        public const string GerenciarDepositoUpdate = "Perfil:GerenciarDepositoUpdate";

        //POLICY LOCAL
        public const string GerenciarLocal = "Perfil:GerenciarLocal";

        //POLICY USUARIO
        public const string GerenciarUsuario = "Perfil:GerenciarUsuario";

        //POLICY USUARIOLOCAL
        public const string GerenciarUsuarioLocal = "Perfil:GerenciarUsuario";
    }
}
