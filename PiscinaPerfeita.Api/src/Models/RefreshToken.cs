namespace PiscinaPerfeita.Api.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiraEm { get; set; }
        public DateTime? RevogadoEm { get; set; }
    }
}
