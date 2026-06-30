// ============================================================
//  Piscina Perfeita — Módulo: Piscinas
// ============================================================
import { useState, useEffect } from "react";
import {
  PageHeader,
  Card,
  Badge,
  Button,
  Modal,
  Toolbar,
  SearchInput,
  DataTable,
  FormGrid,
  FormField,
  inputStyle,
  LoadingSpinner,
  ErrorMessage,
} from "../../components/ui/index.jsx";
import { piscinaService, usuarioService } from "../../config/services.js";

// ----------------------------------------------------------
// Formulário
// ----------------------------------------------------------
function PiscinaForm({ usuarios, initial, onSubmit, onCancel, loading }) {
  const [form, setForm] = useState(
    initial ?? {
      nome: "",
      volumeLitros: "",
      profundidadeMedia: "",
      usuarioId: "",
    },
  );
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  function handleSubmit(e) {
    e.preventDefault();
    onSubmit({
      volumeLitros: form.volumeLitros ? parseFloat(form.volumeLitros) : null,
      nome: form.nome,
      usuarioId: form.usuarioId,
      profundidadeMedia: form.profundidadeMedia
        ? parseFloat(form.profundidadeMedia)
        : null,
    });
    console.log({ form });
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Nome *" fullWidth>
          <input
            required
            type="text"
            placeholder="Ex.: Piscina Olímpica Principal"
            style={inputStyle}
            value={form.nome}
            onChange={set("nome")}
          />
        </FormField>
        <FormField label="Volume (litros)">
          <input
            type="number"
            min="0"
            placeholder="Ex.: 1000000"
            style={inputStyle}
            value={form.volumeLitros}
            onChange={set("volumeLitros")}
          />
        </FormField>
        <FormField label="Profundidade média (m)">
          <input
            type="number"
            step="0.1"
            min="0"
            placeholder="Ex.: 2.0"
            style={inputStyle}
            value={form.profundidadeMedia}
            onChange={set("profundidadeMedia")}
          />
        </FormField>
        <FormField label="Responsável *">
          <select
            required
            style={inputStyle}
            value={form.usuarioId}
            onChange={set("usuarioId")}
          >
            <option value="">Selecione o usuário</option>
            {usuarios.map((u) => (
              <option key={u.id} value={u.id}>
                {u.nome}
              </option>
            ))}
          </select>
        </FormField>
      </FormGrid>
      <div
        style={{
          display: "flex",
          justifyContent: "flex-end",
          gap: 8,
          marginTop: 16,
        }}
      >
        <Button variant="ghost" onClick={onCancel} type="button">
          Cancelar
        </Button>
        <Button variant="primary" type="submit" disabled={loading}>
          {loading
            ? "Salvando…"
            : initial
              ? "Salvar alterações"
              : "Cadastrar piscina"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Piscinas() {
  const [piscinas, setPiscinas] = useState([]);
  const [usuarios, setUsuarios] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [modal, setModal] = useState({ open: false, editing: null });
  const [search, setSearch] = useState("");

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        const [p, u] = await Promise.all([
          piscinaService.listar(),
          usuarioService.listar(),
        ]);
        setPiscinas(p ?? []);
        setUsuarios(u ?? []);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  async function handleSave(dto) {
    try {
      setSaving(true);
      if (modal.editing) {
        const atualizada = await piscinaService.atualizar(
          modal.editing.id,
          dto,
        );
        console.log({ atualizada });
        setPiscinas((prev) =>
          prev.map((p) => (p.id === atualizada.id ? atualizada : p)),
        );
      } else {
        const nova = await piscinaService.criar(dto);
        setPiscinas((prev) => [nova, ...prev]);
      }
      setModal({ open: false, editing: null });
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (
      !confirm(
        "Excluir esta piscina? Todos os dados relacionados serão afetados.",
      )
    )
      return;
    try {
      await piscinaService.excluir(id);
      setPiscinas((prev) => prev.filter((p) => p.id !== id));
    } catch (err) {
      setError(err.message);
    }
  }

  const filtered = piscinas.filter((p) =>
    p.nome.toLowerCase().includes(search.toLowerCase()),
  );

  const columns = [
    { key: "nome", label: "Nome" },
    {
      key: "volumeLitros",
      label: "Volume (L)",
      render: (v) => (v ? v.toLocaleString("pt-BR") : "—"),
    },
    {
      key: "profundidadeMedia",
      label: "Profundidade média",
      render: (v) => (v ? `${v} m` : "—"),
    },
    {
      key: "createdAt",
      label: "Criada em",
      render: (v) => (v ? new Date(v).toLocaleDateString("pt-BR") : "—"),
    },
    {
      key: "usuario",
      label: "Responsável",
      render: (_, r) => r.usuarioPiscina?.nome ?? "—",
    },
    {
      key: "_acoes",
      label: "",
      render: (_, r) => (
        <div style={{ display: "flex", gap: 6 }}>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setModal({ open: true, editing: r })}
          >
            Editar
          </Button>
          <Button variant="danger" size="sm" onClick={() => handleDelete(r.id)}>
            Excluir
          </Button>
        </div>
      ),
    },
  ];

  if (loading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader
        title="Piscinas"
        description="Cadastro e gerenciamento"
        action={
          <Button
            variant="primary"
            onClick={() => setModal({ open: true, editing: null })}
          >
            + Nova piscina
          </Button>
        }
      />

      {error && <ErrorMessage message={error} />}

      <Toolbar>
        <SearchInput
          value={search}
          onChange={setSearch}
          placeholder="Buscar piscina…"
        />
      </Toolbar>

      <Card noPadding>
        <DataTable
          columns={columns}
          data={filtered}
          emptyMessage="Nenhuma piscina cadastrada."
        />
      </Card>

      <Modal
        open={modal.open}
        onClose={() => setModal({ open: false, editing: null })}
        title={modal.editing ? "Editar piscina" : "Nova piscina"}
      >
        <PiscinaForm
          usuarios={usuarios}
          initial={modal.editing}
          onSubmit={handleSave}
          onCancel={() => setModal({ open: false, editing: null })}
          loading={saving}
        />
      </Modal>
    </div>
  );
}
