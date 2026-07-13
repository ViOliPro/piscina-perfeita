// ============================================================
//  Piscina Perfeita — Módulo: Depósitos
// ============================================================
import { useState, useEffect } from "react";
import {
  PageHeader, Card, Button, Modal, Toolbar,
  SearchInput, DataTable, FormGrid, FormField,
  inputStyle, LoadingSpinner, ErrorMessage,
} from "../../components/ui/index.jsx";
import { depositoService } from "../../config/services.js";

// ----------------------------------------------------------
// Formulário
// ----------------------------------------------------------
function DepositoForm({ initial, onSubmit, onCancel, loading }) {
  const [form, setForm] = useState(initial ?? { nome: "", observacao: "" });
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  function handleSubmit(e) {
    e.preventDefault();
    onSubmit(form);
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Nome do depósito *" fullWidth>
          <input required type="text" placeholder="Ex.: Depósito Central"
            style={inputStyle} value={form.nome} onChange={set("nome")} />
        </FormField>
        <FormField label="Observação" fullWidth>
          <input type="text" placeholder="Detalhes adicionais sobre o depósito"
            style={inputStyle} value={form.observacao} onChange={set("observacao")} />
        </FormField>
      </FormGrid>
      <div style={{ display: "flex", justifyContent: "flex-end", gap: 8, marginTop: 16 }}>
        <Button variant="ghost" onClick={onCancel} type="button">Cancelar</Button>
        <Button variant="primary" type="submit" disabled={loading}>
          {loading ? "Salvando…" : initial ? "Salvar alterações" : "Cadastrar depósito"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Depositos() {
  const [depositos, setDepositos] = useState([]);
  const [loading,   setLoading]   = useState(true);
  const [saving,    setSaving]    = useState(false);
  const [error,     setError]     = useState(null);
  const [modal,     setModal]     = useState({ open: false, editing: null });
  const [search,    setSearch]    = useState("");

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        setDepositos((await depositoService.listar()) ?? []);
      } catch (err) { setError(err.message); }
      finally { setLoading(false); }
    }
    load();
  }, []);

  async function handleSave(dto) {
    try {
      setSaving(true);
      if (modal.editing) {
        const atualizado = await depositoService.atualizar(modal.editing.id, dto);
        setDepositos((prev) => prev.map((d) => d.id === atualizado.id ? atualizado : d));
      } else {
        const novo = await depositoService.criar(dto);
        setDepositos((prev) => [novo, ...prev]);
      }
      setModal({ open: false, editing: null });
    } catch (err) { setError(err.message); }
    finally { setSaving(false); }
  }

  async function handleDelete(id) {
    if (!confirm("Excluir este depósito?")) return;
    try {
      await depositoService.excluir(id);
      setDepositos((prev) => prev.filter((d) => d.id !== id));
    } catch (err) { setError(err.message); }
  }

  const filtered = depositos.filter((d) =>
    d.nome.toLowerCase().includes(search.toLowerCase())
  );

  const columns = [
    { key: "nome",       label: "Nome do depósito" },
    { key: "observacao", label: "Observação", render: (v) => v || "—" },
    {
      key: "_acoes", label: "",
      render: (_, r) => (
        <div style={{ display: "flex", gap: 6 }}>
          <Button variant="ghost"  size="sm" onClick={() => setModal({ open: true, editing: r })}>Editar</Button>
          <Button variant="danger" size="sm" onClick={() => handleDelete(r.id)}>Excluir</Button>
        </div>
      ),
    },
  ];

  if (loading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader
        title="Depósitos"
        description="Locais físicos de armazenamento dentro do seu condomínio"
        action={
          <Button variant="primary" onClick={() => setModal({ open: true, editing: null })}>
            + Novo depósito
          </Button>
        }
      />

      {error && <ErrorMessage message={error} />}

      <Toolbar>
        <SearchInput value={search} onChange={setSearch} placeholder="Buscar depósito…" />
      </Toolbar>

      <Card noPadding>
        <DataTable columns={columns} data={filtered} emptyMessage="Nenhum depósito cadastrado." />
      </Card>

      <Modal
        open={modal.open}
        onClose={() => setModal({ open: false, editing: null })}
        title={modal.editing ? "Editar depósito" : "Novo depósito"}
      >
        <DepositoForm
          initial={modal.editing}
          onSubmit={handleSave}
          onCancel={() => setModal({ open: false, editing: null })}
          loading={saving}
        />
      </Modal>
    </div>
  );
}
