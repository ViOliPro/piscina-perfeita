import { useCallback, useEffect, useMemo, useState } from "react";
import { hidrometroService } from "../../../config/services.js";

/**
 * Encapsula toda a camada de dados do módulo Hidrômetro:
 * carregamento de dashboard + histórico, criação e exclusão de leituras.
 * Nenhum componente visual deste módulo deve chamar hidrometroService diretamente.
 */
export function useHidrometroData() {
  const [dashboard, setDashboard] = useState(null);
  const [lancamentos, setLancamentos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);

  const carregarDados = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      // O dashboard e o histórico são contratos independentes da API.
      const [dashboardResult, historicoResult] = await Promise.allSettled([
        hidrometroService.dashboard(),
        hidrometroService.listar(),
      ]);

      if (historicoResult.status === "rejected") throw historicoResult.reason;

      // Durante a implantação, o endpoint de dashboard pode ainda não existir.
      // Nesse caso, o histórico continua utilizável e o componente mostra o placeholder.
      setDashboard(
        dashboardResult.status === "fulfilled" ? dashboardResult.value : null,
      );
      setLancamentos(historicoResult.value ?? []);
    } catch (err) {
      setError(
        err?.message ?? "Não foi possível carregar os dados do hidrômetro.",
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    carregarDados();
  }, [carregarDados]);

  // A API é a única fonte da última leitura usada pela validação. O cliente não
  // reconstitui a sequência das leituras nem deriva consumo.
  const ultimaLeitura = useMemo(
    () => dashboard?.ultimaLeitura ?? null,
    [dashboard],
  );

  async function salvarLeitura(dto) {
    setSaving(true);
    setError(null);

    try {
      await hidrometroService.criar(dto);
      await carregarDados();
      return true;
    } catch (err) {
      setError(err?.message ?? "Não foi possível salvar o lançamento.");
      return false;
    } finally {
      setSaving(false);
    }
  }

  async function excluirLeitura(id) {
    setError(null);
    try {
      await hidrometroService.excluir(id);
      await carregarDados();
    } catch (err) {
      setError(err?.message ?? "Não foi possível excluir o lançamento.");
    }
  }

  return {
    dashboard,
    lancamentos,
    ultimaLeitura,
    loading,
    saving,
    error,
    salvarLeitura,
    excluirLeitura,
  };
}
