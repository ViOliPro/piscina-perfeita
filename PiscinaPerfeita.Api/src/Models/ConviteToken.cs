namespace PiscinaPerfeita.Api.Models;

// Convite de cadastro por link — o Admin/SuperAdmin decide quem entra
// (e-mail, Role, Perfil, LocalId) e o convidado só preenche Nome+Senha
// depois. Mesma lógica de token do PasswordResetToken (hash + expiração +
// uso único), só que aqui ainda não existe Usuario até o convite ser aceito.
public class ConviteToken
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public Role Role { get; set; }
    public Perfil Perfil { get; set; }

    // Nulo quando um SuperAdmin convida sem já saber o Local (o convidado
    // vira Administrador e cai no fluxo de PrimeiroLocal.jsx ao logar
    // depois) — preenchido quando um Administrador convida alguém pro seu
    // próprio Local, ou quando o SuperAdmin já sabe o Local de destino.
    public Guid? LocalId { get; set; }

    // Quem gerou o convite (Admin ou SuperAdmin) — sem FK/navegação por
    // simplicidade; é só auditoria, não é referenciado em nenhuma query hoje.
    public Guid CriadoPorId { get; set; }

    // Precisamos saber se quem convidou era SuperAdmin ou Administrador para
    // reproduzir a mesma regra de CriarUsuarioLocal na hora de aceitar o
    // convite: só um convite de SuperAdmin (com LocalId + Perfil Administrador
    // já definidos juntos) pode originar um Administrador Pai. Um convite
    // feito por um Administrador NUNCA gera Administrador Pai, mesmo
    // convidando outro Administrador (Admin Filho).
    public bool CriadoPorSuperAdmin { get; set; }

    public string TokenHash { get; set; } = null!; // NUNCA guardar o token em texto puro
    public DateTime ExpiraEm { get; set; }
    public DateTime? UsadoEm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
