// ============================================================
//  Piscina Perfeita — Módulo: Piscinas (listagem leve + useQuery)
// ============================================================
import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  PageHeader,
  Card,
  Button,
  Modal,
  Toolbar,
  SearchInput,
  DataTable,
  FormGrid,
  FormField,
  LoadingSpinner,
  ErrorMessage,
} from "../../components/ui/index.jsx";
import { inputStyle } from "../../components/ui/styles.js";
import { piscinaService } from "../../config/services.js";
import { PERMISSIONS } from "../../helpers/Permissions.js";
import ProtecaoDeRota from "../../helpers/ProtecaoDeRota.jsx";
import { useUsuariosSelecionaveis } from "../../hooks/useUsuariosSelecionaveis.js";
import { qk } from "../../helpers/queryKeys.js";

// ----------------------------------------------------------
// Formulário
// ----------------------------------------------------------
function PiscinaForm({ initial, onSubmit, onCancel, loading }) {
  const { usuarios, podeVerUsuario } = useUsuariosSelecionaveis();
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
      ...form,
      volumeLitros: form.volumeLitros ? parseFloat(form.volumeLitros) : null,
      profundidadeMedia: form.profundidadeMedia
        ? parseFloat(form.profundidadeMedia)
        : null,
      usuarioId: podeVerUsuario ? form.usuarioId || null : null,
    });
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
        <FormField label="Volume (litros) *">
          <input
            type="number"
            required
            min="0"
            placeholder="Ex.: 1000000"
            style={inputStyle}
            value={form.volumeLitros}
            onChange={set("volumeLitros")}
          />
        </FormField>
        <FormField label="Profundidade média (m) *">
          <input
            type="number"
            required
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
        <Button
          variant="ghost"
          onClick={onCancel}
          type="button"
          permission={PERMISSIONS.PISCINAS.CREATE}
        >
          Cancelar
        </Button>
        <Button
          variant="primary"
          type="submit"
          disabled={loading}
          permission={PERMISSIONS.PISCINAS.CREATE}
        >
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
// Módulo principal — listagem leve via TanStack Query
// ----------------------------------------------------------
export default function Piscinas() {
  const queryClient = useQueryClient();
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [modal, setModal] = useState({ open: false, editing: null });
  const [search, setSearch] = useState("");

  // Query leve compartilhada com selects (qk.piscinas)
  const {
    data: piscinas = [],
    isLoading,
    isError,
    error: queryError,
  } = useQuery({
    queryKey: qk.piscinas,
    queryFn: () => piscinaService.listar(),
    staleTime: 10 * 60 * 1000, // 10 min — ref estável
  });

  async function handleSave(dto) {
    try {
      setSaving(true);
      setError(null);
      if (modal.editing) {
        await piscinaService.atualizar(modal.editing.id, dto);
      } else {
        await piscinaService.criar(dto);
      }
      await queryClient.invalidateQueries({ queryKey: qk.piscinas });
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
      setError(null);
      await piscinaService.excluir(id);
      await queryClient.invalidateQueries({ queryKey: qk.piscinas });
    } catch (err) {
      setError(err.message);
    }
  }

  const filtered = piscinas.filter((p) =>
    (p.nome ?? "").toLowerCase().includes(search.toLowerCase()),
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
      render: (_, r) => r.usuario?.nome ?? "—",
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
            permission={PERMISSIONS.PISCINAS.EDIT}
          >
            Editar
          </Button>
          <Button
            variant="danger"
            size="sm"
            onClick={() => handleDelete(r.id)}
            permission={PERMISSIONS.PISCINAS.DELETE}
          >
            Excluir
          </Button>
        </div>
      ),
    },
  ];

  if (isLoading) return <LoadingSpinner />;

  return (
    <ProtecaoDeRota permissao={PERMISSIONS.PISCINAS.VIEW}>
      <div>
        <PageHeader
          title="Piscinas"
          description="Cadastro e gerenciamento"
          action={
            <Button
              variant="primary"
              onClick={() => setModal({ open: true, editing: null })}
              permission={PERMISSIONS.PISCINAS.CREATE}
            >
              + Nova piscina
            </Button>
          }
        />

        {(error || isError) && (
          <ErrorMessage message={error ?? queryError?.message} />
        )}

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
            initial={
              modal.editing
                ? {
                    nome: modal.editing.nome ?? "",
                    volumeLitros: modal.editing.volumeLitros ?? "",
                    profundidadeMedia: modal.editing.profundidadeMedia ?? "",
                    usuarioId:
                      modal.editing.usuarioId ??
                      modal.editing.usuario?.id ??
                      "",
                  }
                : null
            }
            onSubmit={handleSave}
            onCancel={() => setModal({ open: false, editing: null })}
            loading={saving}
          />
        </Modal>
      </div>
    </ProtecaoDeRota>
  );
}
