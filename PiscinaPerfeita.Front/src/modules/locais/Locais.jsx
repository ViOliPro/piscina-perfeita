// ============================================================
//  Piscina Perfeita — Módulo: Locais (condomínios/unidades)
// ============================================================
import { useState, useEffect } from "react";
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
  inputStyle,
  LoadingSpinner,
  ErrorMessage,
} from "../../components/ui/index.jsx";
import { localService } from "../../config/services.js";
import { ROLES, PERFIS } from "../../config/index.js";
import { useAuth } from "../../context/AuthContext.jsx";

// ----------------------------------------------------------
// Formulário
// ----------------------------------------------------------
function LocalForm({ initial, onSubmit, onCancel, loading }) {
  const [form, setForm] = useState(
    initial ?? {
      nome: "",
      descricao: "",
      telefone: "",
      observacoes: "",
      endereco: "",
      cidade: "",
      estado: "",
      pais: "",
      cep: "",
    },
  );
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  function handleSubmit(e) {
    e.preventDefault();
    onSubmit(form);
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Nome do local *" fullWidth>
          <input
            required
            type="text"
            placeholder="Ex.: Condomínio Vila das Palmeiras"
            style={inputStyle}
            value={form.nome}
            onChange={set("nome")}
          />
        </FormField>
        <FormField label="Descrição" fullWidth>
          <input
            type="text"
            placeholder="Ex.: Torre A"
            style={inputStyle}
            value={form.descricao}
            onChange={set("descricao")}
          />
        </FormField>
        <FormField label="Telefone">
          <input
            type="text"
            placeholder="(00) 00000-0000"
            style={inputStyle}
            value={form.telefone}
            onChange={set("telefone")}
          />
        </FormField>
        <FormField label="CEP">
          <input
            type="text"
            placeholder="00000-000"
            style={inputStyle}
            value={form.cep}
            onChange={set("cep")}
          />
        </FormField>
        <FormField label="Endereço" fullWidth>
          <input
            type="text"
            placeholder="Rua, número, bairro"
            style={inputStyle}
            value={form.endereco}
            onChange={set("endereco")}
          />
        </FormField>
        <FormField label="Cidade">
          <input
            type="text"
            style={inputStyle}
            value={form.cidade}
            onChange={set("cidade")}
          />
        </FormField>
        <FormField label="Estado">
          <input
            type="text"
            placeholder="UF"
            style={inputStyle}
            value={form.estado}
            onChange={set("estado")}
          />
        </FormField>
        <FormField label="País">
          <input
            type="text"
            style={inputStyle}
            value={form.pais}
            onChange={set("pais")}
          />
        </FormField>
        <FormField label="Observações" fullWidth>
          <input
            type="text"
            style={inputStyle}
            value={form.observacoes}
            onChange={set("observacoes")}
          />
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
              : "Cadastrar local"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Locais() {
  const { user, switchLocal } = useAuth();
  // Gestão (criar/editar/excluir) de Locais é restrita a SuperAdmin no
  // backend (LocalService.GarantirSuperAdmin) — o front só espelha isso na UI.
  const ehSuperAdmin = (user?.role ?? user?.Role) === ROLES.ADMIN;
  const ehPerfilAdmin = (user?.perfil ?? user?.Perfil) === PERFIS.ADMINISTRADOR;

  const [locais, setLocais] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [modal, setModal] = useState({ open: false, editing: null });
  const [search, setSearch] = useState("");

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        setLocais((await localService.listar()) ?? []);
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
        const atualizado = await localService.atualizar(modal.editing.id, dto);
        setLocais((prev) =>
          prev.map((l) => (l.id === atualizado.id ? atualizado : l)),
        );
      } else {
        const novo = await localService.criar(dto);
        setLocais((prev) => [novo, ...prev]);
        const ok = await switchLocal(novo.id);
        console.log(ok);

        if (ok) window.location.reload();
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
        "Excluir este local? Usuários vinculados perderão o acesso a ele.",
      )
    )
      return;
    try {
      await localService.excluir(id);
      setLocais((prev) => prev.filter((l) => l.id !== id));
    } catch (err) {
      setError(err.message);
    }
  }

  const filtered = locais.filter(
    (l) =>
      l.nome.toLowerCase().includes(search.toLowerCase()) ||
      (l.cidade ?? "").toLowerCase().includes(search.toLowerCase()),
  );

  const columns = [
    { key: "nome", label: "Nome" },
    { key: "descricao", label: "Descrição", render: (v) => v || "—" },
    {
      key: "_cidadeEstado",
      label: "Cidade/UF",
      render: (_, r) => [r.cidade, r.estado].filter(Boolean).join("/") || "—",
    },
    { key: "telefone", label: "Telefone", render: (v) => v || "—" },
    ...(ehSuperAdmin || ehPerfilAdmin
      ? [
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
                <Button
                  variant="danger"
                  size="sm"
                  onClick={() => handleDelete(r.id)}
                >
                  Excluir
                </Button>
              </div>
            ),
          },
        ]
      : []),
  ];

  if (loading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader
        title="Locais"
        description={
          ehSuperAdmin || ehPerfilAdmin
            ? "Condomínios e unidades cadastrados no sistema"
            : "Condomínios e unidades aos quais você tem acesso"
        }
        action={
          (ehSuperAdmin || ehPerfilAdmin) && (
            <Button
              variant="primary"
              onClick={() => setModal({ open: true, editing: null })}
            >
              + Novo local
            </Button>
          )
        }
      />

      {error && <ErrorMessage message={error} />}

      <Toolbar>
        <SearchInput
          value={search}
          onChange={setSearch}
          placeholder="Buscar local…"
        />
      </Toolbar>

      <Card noPadding>
        <DataTable
          columns={columns}
          data={filtered}
          emptyMessage="Nenhum local cadastrado."
        />
      </Card>

      {(ehSuperAdmin || ehPerfilAdmin) && (
        <Modal
          open={modal.open}
          onClose={() => setModal({ open: false, editing: null })}
          title={modal.editing ? "Editar local" : "Novo local"}
        >
          <LocalForm
            initial={modal.editing}
            onSubmit={handleSave}
            onCancel={() => setModal({ open: false, editing: null })}
            loading={saving}
          />
        </Modal>
      )}
    </div>
  );
}
