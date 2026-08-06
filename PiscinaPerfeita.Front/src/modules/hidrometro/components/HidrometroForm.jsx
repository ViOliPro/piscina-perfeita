import { useState } from "react";
import {
  Button,
  ErrorMessage,
  FormField,
  FormGrid,
} from "../../../components/ui/index.jsx";
import { inputStyle } from "../../../components/ui/styles.js";
import { getLocalDateTimeInput } from "../../../utils/getLocalDateTimeInput.js";
import styles from "./components.module.css";

export function HidrometroForm({ ultimaLeitura, onSubmit, onCancel, loading }) {
  const [form, setForm] = useState({
    dataLeitura: getLocalDateTimeInput(),
    leituraAtual: "",
    observacoes: "",
  });
  const [erro, setErro] = useState(null);

  function setCampo(campo) {
    return (event) => {
      setForm((atual) => ({ ...atual, [campo]: event.target.value }));
      setErro(null);
    };
  }

  function handleSubmit(event) {
    event.preventDefault();
    const leituraAtual = Number(form.leituraAtual);
    const dataLeitura = new Date(form.dataLeitura);

    if (!form.dataLeitura || Number.isNaN(dataLeitura.getTime())) {
      setErro("Informe uma data e hora válidas para a leitura.");
      return;
    }
    if (dataLeitura.getTime() > Date.now()) {
      setErro("A data e hora da leitura não podem estar no futuro.");
      return;
    }
    if (!Number.isFinite(leituraAtual) || leituraAtual < 0) {
      setErro("Informe uma leitura numérica maior ou igual a zero.");
      return;
    }
    if (ultimaLeitura != null && leituraAtual < ultimaLeitura) {
      setErro(
        `A leitura deve ser maior ou igual à última registrada (${ultimaLeitura.toFixed(2)} m³).`,
      );
      return;
    }

    onSubmit({
      dataLeitura: form.dataLeitura,
      leituraAtual,
      observacoes: form.observacoes.trim() || null,
    });
  }

  return (
    <form onSubmit={handleSubmit}>
      {erro && <ErrorMessage message={erro} />}

      <FormGrid>
        <FormField label="Data e hora da leitura *">
          <input
            type="datetime-local"
            required
            max={getLocalDateTimeInput()}
            style={inputStyle}
            value={form.dataLeitura}
            onChange={setCampo("dataLeitura")}
          />
        </FormField>

        <FormField label="Leitura atual (m³) *">
          <input
            type="number"
            step="0.01"
            min="0"
            required
            placeholder={
              ultimaLeitura != null
                ? `Ex.: ${ultimaLeitura + 1}`
                : "Ex.: 812,40"
            }
            style={inputStyle}
            value={form.leituraAtual}
            onChange={setCampo("leituraAtual")}
          />
        </FormField>

        <FormField label="Observações">
          <textarea
            rows="3"
            maxLength="500"
            placeholder="Ex.: lavagem da garagem, teste de vazamento..."
            style={inputStyle}
            className={styles.formTextarea}
            value={form.observacoes}
            onChange={setCampo("observacoes")}
          />
        </FormField>
      </FormGrid>

      <div className="pp-stack-actions">
        <Button
          variant="ghost"
          type="button"
          onClick={onCancel}
          disabled={loading}
        >
          Cancelar
        </Button>
        <Button variant="primary" type="submit" disabled={loading}>
          {loading ? "Salvando…" : "Salvar leitura"}
        </Button>
      </div>
    </form>
  );
}
