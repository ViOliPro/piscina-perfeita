// ============================================================
//  Piscina Perfeita — Módulo: Usuários
// ============================================================
import { useState, useEffect } from "react";
import {
  PageHeader, Card, Badge, Button, Modal, Toolbar,
  SearchInput, FilterSelect, DataTable, FormGrid, FormField,
  inputStyle, LoadingSpinner, ErrorMessage,
} from "../../components/ui/index.jsx";
import { usuarioService, localService, usuarioLocalService } from "../../config/services.js";
import { ROLES, ROLE_LABELS, PERFIS, PERFIL_LABELS } from "../../config/index.js";
import { useAuth } from "../../context/AuthContext.jsx";

// ----------------------------------------------------------
// Formulário
// ----------------------------------------------------------
function UsuarioForm({ initial, onSubmit, onCancel, loading }) {
  const isEdit = !!initial;
  const { user: usuarioLogado } = useAuth();
  const logadoEhSuperAdmin = (usuarioLogado?.role ?? usuarioLogado?.Role) === ROLES.ADMIN;

  const [form, setForm] = useState(
    initial
      ? { nome: initial.nome, email: initial.email ?? "", role: String(initial.role), senha: "" }
      : {
          nome: "", email: "", role: String(ROLES.USER), senha: "",
          cpf: "", perfil: String(PERFIS.VISUALIZADOR), localId: "",
        }
  );
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  function handleSubmit(e) {
    e.preventDefault();
    const dto = { nome: form.nome, email: form.email, role: parseInt(form.role) };
    if (form.senha) dto.senhaHash = form.senha; // backend deve fazer o hash
    // Cpf, Perfil e LocalId só existem no fluxo de criação (UsuarioRequestDto);
    // a edição usa UsuarioRequestUpdateDto, que não os possui.
    if (!isEdit) {
      dto.cpf = form.cpf;
      dto.perfil = parseInt(form.perfil);
      if (form.localId) dto.localId = form.localId;
    }
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
        {!isEdit && (
          <FormField label="CPF">
            <input type="text" placeholder="000.000.000-00"
              style={inputStyle} value={form.cpf} onChange={set("cpf")} />
          </FormField>
        )}
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
          <select
            required
            disabled={isEdit && !logadoEhSuperAdmin && parseInt(form.role) === ROLES.ADMIN}
            style={inputStyle} value={form.role} onChange={set("role")}
          >
            <option value={String(ROLES.USER)}>{ROLE_LABELS[ROLES.USER]}</option>
            {/* Mantém a opção visível ao editar um SuperAdmin existente, mesmo
                que quem está editando não seja SuperAdmin — evita rebaixar o
                papel por acidente por ausência da opção no <select>. */}
            {(logadoEhSuperAdmin || (isEdit && parseInt(form.role) === ROLES.ADMIN)) && (
              <option value={String(ROLES.ADMIN)}>{ROLE_LABELS[ROLES.ADMIN]}</option>
            )}
          </select>
        </FormField>
        {!isEdit && (
          <FormField label="Perfil no local *">
            <select required style={inputStyle} value={form.perfil} onChange={set("perfil")}>
              {Object.entries(PERFIL_LABELS).map(([valor, label]) => (
                <option key={valor} value={valor}>{label}</option>
              ))}
            </select>
          </FormField>
        )}
        {!isEdit && parseInt(form.role) === ROLES.ADMIN && (
          <FormField label="ID do Local (opcional)" fullWidth>
            <input type="text" placeholder="Deixe em branco para não vincular a um local específico"
              style={inputStyle} value={form.localId} onChange={set("localId")} />
          </FormField>
        )}
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
// Vínculos do usuário com Locais (Perfil por Local)
// ----------------------------------------------------------
function VinculosLocaisModal({ usuario, open, onClose }) {
  const [vinculos, setVinculos] = useState([]);
  const [locaisDisponiveis, setLocaisDisponiveis] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [novo, setNovo] = useState({ localId: "", perfil: String(PERFIS.VISUALIZADOR) });

  useEffect(() => {
    if (!open || !usuario) return;
    async function load() {
      try {
        setLoading(true);
        setError(null);
        const [v, l] = await Promise.all([
          usuarioLocalService.porUsuario(usuario.id),
          localService.listar(),
        ]);
        setVinculos(v ?? []);
        setLocaisDisponiveis(l ?? []);
      } catch (err) { setError(err.message); }
      finally { setLoading(false); }
    }
    load();
  }, [open, usuario]);

  async function handleAdd(e) {
    e.preventDefault();
    if (!novo.localId) return;
    try {
      setSaving(true);
      const criado = await usuarioLocalService.criar({
        usuarioId: usuario.id, localId: novo.localId, perfil: novo.perfil,
      });
      setVinculos((prev) => [...prev, criado]);
      setNovo({ localId: "", perfil: String(PERFIS.VISUALIZADOR) });
    } catch (err) { setError(err.message); }
    finally { setSaving(false); }
  }

  async function handleRemove(id) {
    if (!confirm("Remover o acesso deste usuário a este local?")) return;
    try {
      await usuarioLocalService.excluir(id);
      setVinculos((prev) => prev.filter((v) => v.id !== id));
    } catch (err) { setError(err.message); }
  }

  const localNome = (localId) =>
    locaisDisponiveis.find((l) => l.id === localId)?.nome ?? "Local desconhecido";

  return (
    <Modal open={open} onClose={onClose} title={`Locais de ${usuario?.nome ?? ""}`}>
      {error && <ErrorMessage message={error} />}
      {loading ? <LoadingSpinner /> : (
        <>
          {vinculos.length === 0 && (
            <p style={{ fontSize: 13, color: "#5a6b7a", marginBottom: 12 }}>
              Este usuário ainda não está vinculado a nenhum local.
            </p>
          )}
          <div style={{ display: "flex", flexDirection: "column", gap: 8, marginBottom: 16 }}>
            {vinculos.map((v) => (
              <div key={v.id} style={{
                display: "flex", alignItems: "center", justifyContent: "space-between",
                padding: "8px 12px", borderRadius: 8, background: "#f7f9fb",
              }}>
                <div>
                  <div style={{ fontSize: 13, fontWeight: 600 }}>{v.localNome ?? localNome(v.localId)}</div>
                  <div style={{ fontSize: 12, color: "#5a6b7a" }}>{PERFIL_LABELS[v.perfil] ?? "—"}</div>
                </div>
                <Button variant="danger" size="sm" onClick={() => handleRemove(v.id)}>Remover</Button>
              </div>
            ))}
          </div>

          <form onSubmit={handleAdd}>
            <FormGrid>
              <FormField label="Vincular a um novo local">
                <select
                  required style={inputStyle}
                  value={novo.localId}
                  onChange={(e) => setNovo((f) => ({ ...f, localId: e.target.value }))}
                >
                  <option value="">Selecione…</option>
                  {locaisDisponiveis
                    .filter((l) => !vinculos.some((v) => v.localId === l.id))
                    .map((l) => <option key={l.id} value={l.id}>{l.nome}</option>)}
                </select>
              </FormField>
              <FormField label="Perfil">
                <select
                  required style={inputStyle}
                  value={novo.perfil}
                  onChange={(e) => setNovo((f) => ({ ...f, perfil: e.target.value }))}
                >
                  {Object.entries(PERFIL_LABELS).map(([valor, label]) => (
                    <option key={valor} value={valor}>{label}</option>
                  ))}
                </select>
              </FormField>
            </FormGrid>
            <div style={{ display: "flex", justifyContent: "flex-end", marginTop: 12 }}>
              <Button variant="primary" type="submit" disabled={saving || !novo.localId}>
                {saving ? "Vinculando…" : "+ Vincular local"}
              </Button>
            </div>
          </form>
        </>
      )}
    </Modal>
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
  const [vinculosModal, setVinculosModal] = useState({ open: false, usuario: null });
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
      // Perfil só é retornado pela API no momento da criação do usuário;
      // após uma edição, o campo pode não vir preenchido.
      key: "perfil", label: "Perfil",
      render: (v) => (v != null ? (PERFIL_LABELS[v] ?? "—") : "—"),
    },
    {
      key: "createdAt", label: "Cadastro",
      render: (v) => v ? new Date(v).toLocaleDateString("pt-BR") : "—",
    },
    {
      key: "_acoes", label: "",
      render: (_, r) => (
        <div style={{ display: "flex", gap: 6 }}>
          <Button variant="ghost"  size="sm" onClick={() => setVinculosModal({ open: true, usuario: r })}>Locais</Button>
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
            { value: String(ROLES.ADMIN), label: ROLE_LABELS[ROLES.ADMIN] },
            { value: String(ROLES.USER),  label: ROLE_LABELS[ROLES.USER]  },
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

      <VinculosLocaisModal
        usuario={vinculosModal.usuario}
        open={vinculosModal.open}
        onClose={() => setVinculosModal({ open: false, usuario: null })}
      />
    </div>
  );
}
