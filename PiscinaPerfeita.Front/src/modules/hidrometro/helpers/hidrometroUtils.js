export function formatarNumero(valor, casasDecimais = 2) {
  if (valor == null || Number.isNaN(Number(valor))) return "—";
  return new Intl.NumberFormat("pt-BR", {
    minimumFractionDigits: casasDecimais,
    maximumFractionDigits: casasDecimais,
  }).format(Number(valor));
}

export function formatarMetrosCubicos(valor) {
  const numero = formatarNumero(valor);
  return numero === "—" ? numero : `${numero} m³`;
}

export function formatarDataHora(valor) {
  if (!valor) return "—";
  const data = new Date(valor);
  if (Number.isNaN(data.getTime())) return "—";

  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(data);
}

export function formatarMes(anoMes) {
  if (!anoMes) return "";
  const [ano, mes] = anoMes.split("-");
  const data = new Date(Number(ano), Number(mes) - 1, 1);
  return new Intl.DateTimeFormat("pt-BR", {
    month: "long",
    year: "numeric",
  }).format(data);
}

export function obterOpcoesMes(lancamentos) {
  const meses = [...new Set(
    (lancamentos ?? [])
      .map((item) => item.dataLeitura?.slice(0, 7))
      .filter(Boolean),
  )].sort().reverse();

  return meses.map((value) => ({ value, label: formatarMes(value) }));
}
