import { useMemo, useState } from "react";
import {
  Button,
  Card,
  DataTable,
  FilterSelect,
  Toolbar,
} from "../../../components/ui/index.jsx";
import { PERMISSIONS } from "../../../helpers/Permissions.js";
import {
  formatarDataHora,
  formatarMes,
  formatarMetrosCubicos,
  obterOpcoesMes,
} from "../helpers/hidrometroUtils.js";
import styles from "./components.module.css";

export function HidrometroHistorico({ lancamentos, onDelete }) {
  const [filtroMes, setFiltroMes] = useState("");

  const opcoesMes = useMemo(() => obterOpcoesMes(lancamentos), [lancamentos]);

  const exibidos = useMemo(() => {
    const filtrados = filtroMes
      ? lancamentos.filter(
          (item) => item.dataLeitura?.slice(0, 7) === filtroMes,
        )
      : lancamentos;

    return [...filtrados].sort(
      (a, b) => new Date(b.dataLeitura) - new Date(a.dataLeitura),
    );
  }, [filtroMes, lancamentos]);

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
      <Toolbar>
        <FilterSelect
          value={filtroMes}
          onChange={setFiltroMes}
          placeholder="Todos os períodos"
          options={opcoesMes}
        />
      </Toolbar>

      <Card title={`Histórico de leituras (${exibidos.length})`} noPadding>
        <div className={styles.historicoTableWrap}>
          <DataTable
            columns={columns}
            data={exibidos}
            emptyMessage={
              filtroMes
                ? `Nenhuma leitura encontrada em ${formatarMes(filtroMes)}.`
                : "Nenhuma leitura de hidrômetro registrada."
            }
          />
        </div>
      </Card>
    </section>
  );
}
