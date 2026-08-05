using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class HidrometroRequestDto
    {
        [Required(ErrorMessage = "O PH deve estar entre 0 e 14.")]
        public float Consumo { get; set; }
        public DateTimeOffset? CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    }
}
