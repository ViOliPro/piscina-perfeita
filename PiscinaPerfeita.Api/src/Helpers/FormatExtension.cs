using System.Globalization;

namespace PiscinaPerfeita.Api.Helpers
{
    public static class FormatExtensions
    {
        private static readonly CultureInfo PtBrCulture = new CultureInfo("pt-BR");

        /// <summary>
        /// Formata um decimal para string no padrão brasileiro de moeda (R$ 19,99).
        /// </summary>
        public static string ToPtBrCurrency(this decimal? valor)
        {
            return valor?.ToString("C2", PtBrCulture) ?? string.Empty;
        }

        /// <summary>
        /// Formata um decimal para string no padrão brasileiro apenas com os números (19,99).
        /// </summary>
        public static string ToPtBrString(this decimal valor)
        {
            return valor.ToString("N2", PtBrCulture);
        }

        /// <summary>
        /// Suporte para decimais nulos (decimal?). Retorna string vazia ou o valor formatado.
        /// </summary>
        public static string ToPtBrString(this decimal? valor)
        {
            return valor?.ToString("N2", PtBrCulture) ?? string.Empty;
        }
    }
}