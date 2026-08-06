import { useState } from "react";
import {
  Button,
  ErrorMessage,
  LoadingSpinner,
  Modal,
  PageHeader,
} from "../../components/ui/index.jsx";
import { PERMISSIONS } from "../../helpers/Permissions.js";
import ProtecaoDeRota from "../../helpers/ProtecaoDeRota.jsx";
import { useHidrometroData } from "./hooks/useHidrometroData.js";
import { HidrometroDashboard } from "./components/HidrometroDashboard.jsx";
import { HidrometroForm } from "./components/HidrometroForm.jsx";
import { HidrometroHistorico } from "./components/HidrometroHistorico.jsx";

export default function Hidrometro() {
  const {
    dashboard,
    lancamentos,
    ultimaLeitura,
    loading,
    saving,
    error,
    salvarLeitura,
    excluirLeitura,
  } = useHidrometroData();

  const [modalOpen, setModalOpen] = useState(false);

  async function handleSave(dto) {
    const ok = await salvarLeitura(dto);
    if (ok) setModalOpen(false);
  }

  function handleDelete(id) {
    if (!window.confirm("Excluir este lançamento de hidrômetro?")) return;
    excluirLeitura(id);
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
