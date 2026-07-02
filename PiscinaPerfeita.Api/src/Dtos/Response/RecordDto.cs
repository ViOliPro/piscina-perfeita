namespace PiscinaPerfeita.Api.Dtos.Response
{
    // Dto para retornar apenas o Id e Nome de um objeto
    public record NomeIdDto(Guid Id, string? Nome);

    // Dto retorno Enum Estoque TipoMovimentacao
    public enum Tipo
    {
        Entrada,
        Saida
    }

    public enum Role
    {
        Admin = 0,
        User = 1
    }

}
