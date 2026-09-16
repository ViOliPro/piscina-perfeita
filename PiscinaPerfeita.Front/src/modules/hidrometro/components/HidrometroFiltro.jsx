import { useState } from "react";
import { Button, FilterSelect, Tabs, Toolbar } from "../../../components/ui/index.jsx";
import { inputStyle } from "../../../components/ui/styles.js";

const TABS = [
  { id: "mes", label: "Mês único" },
  { id: "intervalo", label: "Intervalo de datas" },
];

/**
 * Filtro de período do módulo Hidrômetro. Controlado pelo pai: `filtro` é o
 * valor aplicado (usado tanto pelo dashboard quanto pelo histórico), e
 * `onChange` recebe o novo filtro já validado/pronto para uso.
 *
 * O filtro por mês aplica na hora (não depende de ida à API para o
 * histórico). O filtro por intervalo de datas exige "Aplicar" — evita
 * disparar uma nova busca de dashboard a cada tecla digitada nos campos
 * de data, seguindo o mesmo padrão já usado no módulo de Movimentações.
 */
export function HidrometroFiltro({ filtro, opcoesMes, onChange }) {
  const [dataInicioInput, setDataInicioInput] = useState(filtro.dataInicio ?? "");
  const [dataFimInput, setDataFimInput] = useState(filtro.dataFim ?? "");
  const [erro, setErro] = useState(null);

  function trocarTipo(tipo) {
    setErro(null);
    if (tipo === "mes") {
      onChange({ tipo: "mes", mes: "", dataInicio: "", dataFim: "" });
    } else {
      setDataInicioInput("");
      setDataFimInput("");
      onChange({ tipo: "intervalo", mes: "", dataInicio: "", dataFim: "" });
    }
  }

  function aplicarIntervalo() {
    if (dataInicioInput && dataFimInput && dataFimInput < dataInicioInput) {
      setErro("A data final deve ser posterior ou igual à data inicial.");
      return;
    }
    setErro(null);
    onChange({
      tipo: "intervalo",
      mes: "",
      dataInicio: dataInicioInput,
      dataFim: dataFimInput,
    });
  }

  function limparIntervalo() {
    setDataInicioInput("");
    setDataFimInput("");
    setErro(null);
    onChange({ tipo: "intervalo", mes: "", dataInicio: "", dataFim: "" });
  }

  return (
    <div>
      <Tabs tabs={TABS} active={filtro.tipo} onChange={trocarTipo} />

      {filtro.tipo === "mes" ? (
        <Toolbar>
          <FilterSelect
            value={filtro.mes}
            onChange={(mes) => onChange({ tipo: "mes", mes, dataInicio: "", dataFim: "" })}
            placeholder="Todos os períodos"
            options={opcoesMes}
          />
        </Toolbar>
      ) : (
        <>
          <Toolbar>
            <input
              type="date"
              aria-label="Data inicial"
              title="Data inicial"
              style={inputStyle}
              value={dataInicioInput}
              onChange={(e) => setDataInicioInput(e.target.value)}
            />
            <input
              type="date"
              aria-label="Data final"
              title="Data final"
              style={inputStyle}
              value={dataFimInput}
              onChange={(e) => setDataFimInput(e.target.value)}
            />
            <Button variant="ghost" onClick={aplicarIntervalo}>
              Aplicar
            </Button>
            <Button variant="ghost" onClick={limparIntervalo}>
              Limpar
            </Button>
          </Toolbar>
          {erro && (
            <p style={{ color: "#E74C3C", fontSize: 13, marginTop: -6 }}>{erro}</p>
          )}
        </>
      )}
    </div>
  );
}
