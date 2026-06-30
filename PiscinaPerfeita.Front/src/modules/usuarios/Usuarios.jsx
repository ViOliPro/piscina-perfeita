// ============================================================
//  Piscina Perfeita — Módulo: Usuários
// ============================================================
import { useState, useEffect } from "react";
import {
  PageHeader, Card, Badge, Button, Modal, Toolbar,
  SearchInput, FilterSelect, DataTable, FormGrid, FormField,
  inputStyle, LoadingSpinner, ErrorMessage,
} from "../../components/ui/index.jsx";
import { usuarioService } from "../../config/services.js";
import { ROLES, ROLE_LABELS } from "../../config/index.js";

// ----------------------------------------------------------
// Formulário
// ----------------------------------------------------------
function UsuarioForm({ initial, onSubmit, onCancel, loading }) {
  const isEdit = !!initial;
  const [form, setForm] = useState(
    initial
      ? { nome: initial.nome, email: initial.email ?? "", role: String(initial.role), senha: "" }
      : { nome: "", email: "", role: String(ROLES.USER), senha: "" }
  );
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  function handleSubmit(e) {
    e.preventDefault();
    const dto = { nome: form.nome, email: form.email, role: parseInt(form.role) };
    if (form.senha) dto.senhaHash = form.senha; // backend deve fazer o hash
    onSubmit(dto);
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Nome completo *" fullWidth>
          <input required type="text" placeholder="Nome do usuário"
            style={inputStyle} value={form.nome} onChange={set("nome")} />
        </FormField>
        <FormField label="E-mail *" fullWidth>
          <input required type="email" placeholder="email@empresa.com.br"
            style={inputStyle} value={form.email} onChange={set("email")} />
        </FormField>
        <FormField label={isEdit ? "Nova senha (deixe em branco para manter)" : "Senha *"}>
          <input
            type="password"
            required={!isEdit}
            minLength={8}
            placeholder={isEdit ? "Nova senha (opcional)" : "Mínimo 8 caracteres"}
            style={inputStyle}
            value={form.senha}
            onChange={set("senha")}
          />
        </FormField>
        <FormField label="Papel *">
          <select required style={inputStyle} value={form.role} onChange={set("role")}>
            <option value={String(ROLES.USER)}>User</option>
            <option value={String(ROLES.ADMIN)}>Admin</option>
          </select>
        </FormField>
      </FormGrid>
      <div style={{ display: "flex", justifyContent: "flex-end", gap: 8, marginTop: 16 }}>
        <Button variant="ghost" onClick={onCancel} type="button">Cancelar</Button>
        <Button variant="primary" type="submit" disabled={loading}>
          {loading ? "Salvando…" : isEdit ? "Salvar alterações" : "Criar usuário"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Usuarios() {
  const [usuarios,  setUsuarios]  = useState([]);
  const [loading,   setLoading]   = useState(true);
  const [saving,    setSaving]    = useState(false);
  const [error,     setError]     = useState(null);
  const [modal,     setModal]     = useState({ open: false, editing: null });
  const [search,    setSearch]    = useState("");
  const [filtroRole, setFiltroRole] = useState("");

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        setUsuarios((await usuarioService.listar()) ?? []);
      } catch (err) { setError(err.message); }
      finally { setLoading(false); }
    }
    load();
  }, []);

  async function handleSave(dto) {
    try {
      setSaving(true);
      if (modal.editing) {
        const atualizado = await usuarioService.atualizar(modal.editing.id, dto);
        setUsuarios((prev) => prev.map((u) => u.id === atualizado.id ? atualizado : u));
      } else {
        const novo = await usuarioService.criar(dto);
        setUsuarios((prev) => [novo, ...prev]);
      }
      setModal({ open: false, editing: null });
    } catch (err) { setError(err.message); }
    finally { setSaving(false); }
  }

  async function handleDelete(id) {
    if (!confirm("Excluir este usuário? Esta ação não pode ser desfeita.")) return;
    try {
      await usuarioService.excluir(id);
      setUsuarios((prev) => prev.filter((u) => u.id !== id));
    } catch (err) { setError(err.message); }
  }

  const filtered = usuarios.filter((u) => {
    const txt = `${u.nome} ${u.email}`.toLowerCase();
    return txt.includes(search.toLowerCase()) &&
           (filtroRole === "" || u.role === parseInt(filtroRole));
  });

  const columns = [
    { key: "nome",  label: "Nome" },
    { key: "email", label: "E-mail", render: (v) => v ?? "—" },
    {
      key: "role", label: "Papel",
      render: (v) => (
        <Badge variant={v === ROLES.ADMIN ? "info" : "ok"}>
          {ROLE_LABELS[v] ?? "—"}
        </Badge>
      ),
    },
    {
      key: "createdAt", label: "Cadastro",
      render: (v) => v ? new Date(v).toLocaleDateString("pt-BR") : "—",
    },
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
        title="Usuários"
        description="Controle de acesso ao sistema"
        action={
          <Button variant="primary" onClick={() => setModal({ open: true, editing: null })}>
            + Novo usuário
          </Button>
        }
      />

      {error && <ErrorMessage message={error} />}

      <Toolbar>
        <SearchInput value={search} onChange={setSearch} placeholder="Buscar nome ou e-mail…" />
        <FilterSelect
          value={filtroRole} onChange={setFiltroRole}
          placeholder="Todos os papéis"
          options={[
            { value: String(ROLES.ADMIN), label: "Admin" },
            { value: String(ROLES.USER),  label: "User"  },
          ]}
        />
      </Toolbar>

      <Card noPadding>
        <DataTable columns={columns} data={filtered} emptyMessage="Nenhum usuário encontrado." />
      </Card>

      <Modal
        open={modal.open}
        onClose={() => setModal({ open: false, editing: null })}
        title={modal.editing ? "Editar usuário" : "Novo usuário"}
      >
        <UsuarioForm
          initial={modal.editing}
          onSubmit={handleSave}
          onCancel={() => setModal({ open: false, editing: null })}
          loading={saving}
        />
      </Modal>
    </div>
  );
}
