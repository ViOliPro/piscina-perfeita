// ============================================================
//  Piscina Perfeita — Módulo: Produtos
// ============================================================
import { useState, useEffect } from "react";
import {
  PageHeader, Card, Button, Modal, Toolbar,
  SearchInput, DataTable, FormGrid, FormField,
  inputStyle, LoadingSpinner, ErrorMessage,
} from "../../components/ui/index.jsx";
import { produtoService } from "../../config/services.js";
import { UNIDADES_MEDIDA, CATEGORIAS_PRODUTO_SUGESTOES } from "../../config/index.js";

// ----------------------------------------------------------
// Formulário
// ----------------------------------------------------------
function ProdutoForm({ initial, onSubmit, onCancel, loading }) {
  const [form, setForm] = useState(
    initial ?? {
      nome: "", unidadeMedida: "kg",
      fabricante: "", marca: "", categoria: "", observacoes: "",
    }
  );
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  function handleSubmit(e) {
    e.preventDefault();
    onSubmit(form);
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Nome do produto *" fullWidth>
          <input required type="text" placeholder="Ex.: Cloro granulado 65%"
            style={inputStyle} value={form.nome} onChange={set("nome")} />
        </FormField>
        <FormField label="Unidade de medida *">
          <select required style={inputStyle} value={form.unidadeMedida} onChange={set("unidadeMedida")}>
            {UNIDADES_MEDIDA.map((u) => <option key={u} value={u}>{u}</option>)}
          </select>
        </FormField>
        <FormField label="Categoria">
          <input
            type="text" list="categorias-produto-sugestoes" placeholder="Ex.: Químicos"
            style={inputStyle} value={form.categoria} onChange={set("categoria")}
          />
          <datalist id="categorias-produto-sugestoes">
            {CATEGORIAS_PRODUTO_SUGESTOES.map((c) => <option key={c} value={c} />)}
          </datalist>
        </FormField>
        <FormField label="Fabricante">
          <input type="text" placeholder="Ex.: HTH"
            style={inputStyle} value={form.fabricante} onChange={set("fabricante")} />
        </FormField>
        <FormField label="Marca">
          <input type="text" placeholder="Ex.: HTH Ultra"
            style={inputStyle} value={form.marca} onChange={set("marca")} />
        </FormField>
        <FormField label="Observações" fullWidth>
          <input type="text" placeholder="Detalhes adicionais sobre o produto"
            style={inputStyle} value={form.observacoes} onChange={set("observacoes")} />
        </FormField>
      </FormGrid>
      <div style={{ display: "flex", justifyContent: "flex-end", gap: 8, marginTop: 16 }}>
        <Button variant="ghost" onClick={onCancel} type="button">Cancelar</Button>
        <Button variant="primary" type="submit" disabled={loading}>
          {loading ? "Salvando…" : initial ? "Salvar alterações" : "Cadastrar produto"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Produtos() {
  const [produtos,  setProdutos]  = useState([]);
  const [loading,   setLoading]   = useState(true);
  const [saving,    setSaving]    = useState(false);
  const [error,     setError]     = useState(null);
  const [modal,     setModal]     = useState({ open: false, editing: null });
  const [search,    setSearch]    = useState("");

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        setProdutos((await produtoService.listar()) ?? []);
      } catch (err) { setError(err.message); }
      finally { setLoading(false); }
    }
    load();
  }, []);

  async function handleSave(dto) {
    try {
      setSaving(true);
      if (modal.editing) {
        const atualizado = await produtoService.atualizar(modal.editing.id, dto);
        setProdutos((prev) => prev.map((p) => p.id === atualizado.id ? atualizado : p));
      } else {
        const novo = await produtoService.criar(dto);
        setProdutos((prev) => [novo, ...prev]);
      }
      setModal({ open: false, editing: null });
    } catch (err) { setError(err.message); }
    finally { setSaving(false); }
  }

  async function handleDelete(id) {
    if (!confirm("Excluir este produto?")) return;
    try {
      await produtoService.excluir(id);
      setProdutos((prev) => prev.filter((p) => p.id !== id));
    } catch (err) { setError(err.message); }
  }

  const filtered = produtos.filter((p) =>
    p.nome.toLowerCase().includes(search.toLowerCase()) ||
    (p.marca ?? "").toLowerCase().includes(search.toLowerCase()) ||
    (p.fabricante ?? "").toLowerCase().includes(search.toLowerCase()) ||
    (p.categoria ?? "").toLowerCase().includes(search.toLowerCase())
  );

  const columns = [
    { key: "nome",          label: "Nome do produto" },
    { key: "categoria",     label: "Categoria",   render: (v) => v || "—" },
    { key: "marca",         label: "Marca",       render: (v) => v || "—" },
    { key: "fabricante",    label: "Fabricante",  render: (v) => v || "—" },
    { key: "unidadeMedida", label: "Unidade de medida" },
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
        title="Produtos"
        description="Cadastro de insumos e químicos"
        action={
          <Button variant="primary" onClick={() => setModal({ open: true, editing: null })}>
            + Novo produto
          </Button>
        }
      />

      {error && <ErrorMessage message={error} />}

      <Toolbar>
        <SearchInput value={search} onChange={setSearch} placeholder="Buscar produto…" />
      </Toolbar>

      <Card noPadding>
        <DataTable columns={columns} data={filtered} emptyMessage="Nenhum produto cadastrado." />
      </Card>

      <Modal
        open={modal.open}
        onClose={() => setModal({ open: false, editing: null })}
        title={modal.editing ? "Editar produto" : "Novo produto"}
      >
        <ProdutoForm
          initial={modal.editing}
          onSubmit={handleSave}
          onCancel={() => setModal({ open: false, editing: null })}
          loading={saving}
        />
      </Modal>
    </div>
  );
}
