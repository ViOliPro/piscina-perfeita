using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class HidrometroResponseDto
    {
        public Guid Id { get; set; }
        public float Consumo { get; set; }
        public DateTimeOffset? CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    }
}
