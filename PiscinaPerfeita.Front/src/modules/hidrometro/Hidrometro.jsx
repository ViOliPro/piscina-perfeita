import { useCallback, useEffect, useMemo, useState } from "react";
import {
  Button,
  ErrorMessage,
  LoadingSpinner,
  Modal,
  PageHeader,
} from "../../components/ui/index.jsx";
import { hidrometroService } from "../../config/services.js";
import { PERMISSIONS } from "../../helpers/Permissions.js";
import ProtecaoDeRota from "../../helpers/ProtecaoDeRota.jsx";
import HidrometroDashboard from "./components/HidrometroDashboard.jsx";
import HidrometroForm from "./components/HidrometroForm.jsx";
import HidrometroHistorico from "./components/HidrometroHistorico.jsx";

export default function Hidrometro() {
  const [dashboard, setDashboard] = useState(null);
  const [lancamentos, setLancamentos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
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
      setError(err?.message ?? "Não foi possível carregar os dados do hidrômetro.");
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

  async function handleSave(dto) {
    setSaving(true);
    setError(null);

    try {
      await hidrometroService.criar(dto);
      setModalOpen(false);
      await carregarDados();
    } catch (err) {
      setError(err?.message ?? "Não foi possível salvar o lançamento.");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!window.confirm("Excluir este lançamento de hidrômetro?")) return;

    setError(null);
    try {
      await hidrometroService.excluir(id);
      await carregarDados();
    } catch (err) {
      setError(err?.message ?? "Não foi possível excluir o lançamento.");
    }
  }

  return (
    <ProtecaoDeRota permissao={PERMISSIONS.HIDROMETRO.VIEW}>
      <div>
        <PageHeader
          title="Hidrômetro"
          description="Acompanhe as leituras do medidor de água e identifique variações de consumo."
          action={
            <Button
              variant="primary"
              onClick={() => setModalOpen(true)}
              permission={PERMISSIONS.HIDROMETRO.CREATE}
            >
              + Nova leitura
            </Button>
          }
        />

        {error && <ErrorMessage message={error} />}

        {loading ? (
          <LoadingSpinner />
        ) : (
          <>
            <HidrometroDashboard dashboard={dashboard} />
            <HidrometroHistorico
              lancamentos={lancamentos}
              onDelete={handleDelete}
            />
          </>
        )}

        <Modal
          open={modalOpen}
          onClose={() => !saving && setModalOpen(false)}
          title="Nova leitura de hidrômetro"
        >
          <HidrometroForm
            ultimaLeitura={ultimaLeitura}
            onSubmit={handleSave}
            onCancel={() => setModalOpen(false)}
            loading={saving}
          />
        </Modal>
      </div>
    </ProtecaoDeRota>
  );
}
