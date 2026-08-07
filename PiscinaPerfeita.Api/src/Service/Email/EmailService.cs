using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace PiscinaPerfeita.Api.Service.Email;

/// <summary>
/// Envia e-mails via API do Resend (https://resend.com/docs/api-reference/emails/send-email).
/// Free tier: 100 e-mails/dia, 3.000/mês — suficiente pro volume esperado hoje.
/// Precisa verificar um domínio no painel do Resend antes de usar um remetente
/// @seudominio.com; enquanto não verificar, só é permitido enviar para o
/// próprio e-mail cadastrado na conta Resend (bom para testar antes do domínio).
/// </summary>
public class ResendEmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly string _remetente; // ex.: "PiscinaPerfeita <naoresponda@seudominio.com>"

    

    public ResendEmailService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.resend.com/");
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            config["RESEND__APIKEY"]
        );
        _remetente =
            config["RESEND__REMETENTE"]
            ?? throw new InvalidOperationException("Configuração 'RESEND__REMETENTE' ausente.");
    }

    public async Task EnviarRedefinicaoSenhaAsync(
        string destinatarioEmail,
        string destinatarioNome,
        string linkRedefinicao
    )
    {
        var payload = new
        {
            from = _remetente,
            to = new[] { destinatarioEmail },
            subject = "Redefinição de senha — PiscinaPerfeita",
            html = MontarHtml(destinatarioNome, linkRedefinicao),
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        var response = await _http.PostAsync("emails", content);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            // Logar mas não relançar pro chamador quebrar o fluxo do usuário —
            // o endpoint de "esqueci senha" já responde sucesso genérico antes
            // de chamar isso; falha de e-mail deve virar alerta/log, não 500 pro usuário.
            throw new EmailDeliveryException($"Resend retornou {response.StatusCode}: {body}");
        }
    }

    private static string MontarHtml(string nome, string link) =>
        $"""
            <p>Olá, {nome}.</p>
            <p>Recebemos uma solicitação para redefinir sua senha no PiscinaPerfeita.</p>
            <p><a href="{link}">Clique aqui para definir uma nova senha</a></p>
            <p>Se você não pediu isso, pode ignorar este e-mail — sua senha continua a mesma.</p>
            <p>Este link expira em 1 hora.</p>
            """;

    public async Task EnviarConviteAsync(string destinatarioEmail, string linkConvite)
    {
        var payload = new
        {
            from = _remetente,
            to = new[] { destinatarioEmail },
            subject = "Você foi convidado para o PiscinaPerfeita",
            html = MontarHtmlConvite(linkConvite),
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        var response = await _http.PostAsync("emails", content);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"_____________________________________________________________{response}");
            Console.WriteLine($"_____________________________________________________________{body}");
            throw new EmailDeliveryException($"Resend retornou {response.StatusCode}: {body}");
        }
    }

    private static string MontarHtmlConvite(string link) =>
        $"""
            <p>Você foi convidado para acessar o PiscinaPerfeita.</p>
            <p><a href="{link}">Clique aqui para completar seu cadastro</a></p>
            <p>Você vai definir seu nome e senha na próxima tela.</p>
            <p>Este link expira em 48 horas. Se você não esperava este convite, pode ignorá-lo.</p>
            """;
}

public class EmailDeliveryException : Exception
{
    public EmailDeliveryException(string message)
        : base(message) { }
}
