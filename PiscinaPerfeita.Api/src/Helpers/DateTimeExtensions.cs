namespace PiscinaPerfeita.Api.Helpers
{
    public static class DateTimeExtensions
    {
        public static string ToDataBR(this DateTimeOffset dataHoraOriginal)
        {
            // 1. Localiza o fuso horário oficial de Brasília
            TimeZoneInfo fusoBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

            // 2. Converte o DateTimeOffset para o horário de Brasília
            DateTimeOffset dataHoraBrasilia = TimeZoneInfo.ConvertTime(dataHoraOriginal, fusoBrasilia);

            // 3. Retorna apenas a data formatada no padrão brasileiro (DD/MM/AAAA)
            return dataHoraBrasilia.ToString("dd/MM/yyyy");
        }

        // DICA EXTRA: Se quiser formatar com Horas também (DD/MM/AAAA HH:MM)
        public static string ToDataHoraBR(this DateTimeOffset dataHoraOriginal)
        {
            TimeZoneInfo fusoBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            DateTimeOffset dataHoraBrasilia = TimeZoneInfo.ConvertTime(dataHoraOriginal, fusoBrasilia);
            return dataHoraBrasilia.ToString("dd/MM/yyyy HH:mm");
        }
    }
}
