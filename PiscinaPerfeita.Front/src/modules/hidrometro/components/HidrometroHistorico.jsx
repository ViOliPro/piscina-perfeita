import { useMemo } from "react";
import { Button, Card, DataTable } from "../../../components/ui/index.jsx";
import { PERMISSIONS } from "../../../helpers/Permissions.js";
import {
  filtrarLancamentosPorPeriodo,
  filtroPeriodoVazio,
  formatarDataCurta,
  formatarDataHora,
  formatarMes,
  formatarMetrosCubicos,
} from "../helpers/hidrometroUtils.js";
import styles from "./components.module.css";

export function HidrometroHistorico({ lancamentos, filtro, onDelete }) {
  const exibidos = useMemo(() => {
    const filtrados = filtrarLancamentosPorPeriodo(lancamentos, filtro);
    return [...filtrados].sort(
      (a, b) => new Date(b.dataLeitura) - new Date(a.dataLeitura),
    );
  }, [lancamentos, filtro]);

  const emptyMessage = useMemo(() => {
    if (filtroPeriodoVazio(filtro))
      return "Nenhuma leitura de hidrômetro registrada.";

    if (filtro.tipo === "mes")
      return `Nenhuma leitura encontrada em ${formatarMes(filtro.mes)}.`;

    const inicio = formatarDataCurta(filtro.dataInicio);
    const fim = formatarDataCurta(filtro.dataFim);
    if (inicio && fim) return `Nenhuma leitura encontrada entre ${inicio} e ${fim}.`;
    if (inicio) return `Nenhuma leitura encontrada a partir de ${inicio}.`;
    return `Nenhuma leitura encontrada até ${fim}.`;
  }, [filtro]);

  const columns = [
    {
      key: "dataLeitura",
      label: "Data e hora",
      render: (valor) => formatarDataHora(valor),
    },
    {
      key: "leituraAtual",
      label: "Leitura (m³)",
      render: (valor) => formatarMetrosCubicos(valor),
    },
    {
      key: "consumo",
      label: "Consumo (m³)",
      render: (valor) => formatarMetrosCubicos(valor),
    },
    {
      key: "observacoes",
      label: "Observações",
      render: (valor) => valor || "—",
    },
    {
      key: "acoes",
      label: "",
      render: (_, registro) => (
        <Button
          variant="danger"
          size="sm"
          onClick={() => onDelete(registro.id)}
          permission={PERMISSIONS.HIDROMETRO.DELETE}
        >
          Excluir
        </Button>
      ),
    },
  ];

  return (
    <section aria-label="Histórico de leituras">
      <Card title={`Histórico de leituras (${exibidos.length})`} noPadding>
        <div className={styles.historicoTableWrap}>
          <DataTable
            columns={columns}
            data={exibidos}
            emptyMessage={emptyMessage}
          />
        </div>
      </Card>
    </section>
  );
}
