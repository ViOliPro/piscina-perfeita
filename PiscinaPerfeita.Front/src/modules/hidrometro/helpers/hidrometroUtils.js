import { inicioDoDiaISO, fimDoDiaISO } from "../../../helpers/queryKeys.js";

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

export function formatarDataCurta(dataYmd) {
  if (!dataYmd) return "";
  const data = new Date(`${dataYmd}T00:00:00`);
  if (Number.isNaN(data.getTime())) return "";
  return new Intl.DateTimeFormat("pt-BR", { dateStyle: "short" }).format(data);
}

// filtro = { tipo: "mes" | "intervalo", mes, dataInicio, dataFim }
// dataInicio/dataFim vêm de <input type="date"> (yyyy-MM-dd); mes vem de
// obterOpcoesMes (yyyy-MM). Este é o mesmo filtro usado tanto para recortar
// o histórico no cliente quanto para os parâmetros enviados ao dashboard.

export function filtroPeriodoVazio(filtro) {
  if (!filtro) return true;
  if (filtro.tipo === "intervalo") return !filtro.dataInicio && !filtro.dataFim;
  return !filtro.mes;
}

export function filtrarLancamentosPorPeriodo(lancamentos, filtro) {
  const lista = lancamentos ?? [];
  if (filtroPeriodoVazio(filtro)) return lista;

  if (filtro.tipo === "intervalo") {
    const inicio = filtro.dataInicio
      ? new Date(`${filtro.dataInicio}T00:00:00`)
      : null;
    const fim = filtro.dataFim
      ? new Date(`${filtro.dataFim}T23:59:59.999`)
      : null;

    return lista.filter((item) => {
      const data = new Date(item.dataLeitura);
      if (inicio && data < inicio) return false;
      if (fim && data > fim) return false;
      return true;
    });
  }

  return lista.filter((item) => item.dataLeitura?.slice(0, 7) === filtro.mes);
}

// Converte o filtro (formato de domínio/tela) nos query params que a API
// de dashboard espera. Sem filtro aplicado, não manda nada — o backend
// já assume o mês corrente como padrão, igual ao comportamento anterior.
export function paramsDashboardDoFiltro(filtro) {
  if (filtroPeriodoVazio(filtro)) return {};

  if (filtro.tipo === "intervalo") {
    const params = {};
    if (filtro.dataInicio) params.dataInicio = inicioDoDiaISO(filtro.dataInicio);
    if (filtro.dataFim) params.dataFim = fimDoDiaISO(filtro.dataFim);
    return params;
  }

  return { mes: filtro.mes };
}

export function obterOpcoesMes(lancamentos) {
  const meses = [
    ...new Set(
      (lancamentos ?? [])
        .map((item) => item.dataLeitura?.slice(0, 7))
        .filter(Boolean),
    ),
  ]
    .sort()
    .reverse();

  return meses.map((value) => ({ value, label: formatarMes(value) }));
}
