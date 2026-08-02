namespace PiscinaPerfeita.Api.Service.Email;

public interface IEmailService
{
    Task EnviarRedefinicaoSenhaAsync(
        string destinatarioEmail,
        string destinatarioNome,
        string linkRedefinicao
    );
}
