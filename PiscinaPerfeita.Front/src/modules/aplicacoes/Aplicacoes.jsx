// ============================================================
//  Piscina Perfeita — Módulo: Aplicações de Produto
//
//  Registra o uso de um produto em uma piscina. Ao salvar, o backend
//  cria MovimentacaoEstoque (Tipo=Aplicação) e dá baixa no Estoque.
//
//  Performance (TanStack Query):
//  - Listagem de aplicações carrega sozinha → tabela aparece rápido
//  - Refs (piscinas/produtos/depósitos/estoques) só quando o modal abre
//    ou quando veio prefill de Análises (form já aberto)
//  - Análises só no modal (select “análise relacionada”)
//  - Cache compartilhado com outras telas (staleTime 10 min nas refs)
// ============================================================
import { useState, useEffect, lazy, Suspense } from "react";
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
import {
  aplicacaoProdutoService,
  piscinaService,
  produtoService,
  depositoService,
  analiseService,
  estoqueService,
} from "../../config/services.js";
import { UNIDADES_LANCAMENTO } from "../../config/index.js";
import { useAuth } from "../../context/AuthContext.jsx";
import { getLocalDateTimeInput } from "../../utils/getLocalDateTimeInput.js";
import ProtecaoDeRota from "../../helpers/ProtecaoDeRota.jsx";
import { PERMISSIONS } from "../../helpers/Permissions.js";
import { qk, diasAtrasISO } from "../../helpers/queryKeys.js";

const UsoProdutosCard = lazy(() => import("./UsoProdutosCard.jsx"));

// ----------------------------------------------------------
// Formulário
// ----------------------------------------------------------
function AplicacaoForm({
  piscinas,
  produtos,
  depositos,
  analises,
  estoques,
  refsLoading,
  initial,
  onSubmit,
  onCancel,
  loading,
}) {
  const [form, setForm] = useState(
    initial ?? {
      piscinaId: "",
      produtoId: "",
      depositoId: "",
      quantidade: "",
      unidadeLancamento: "",
      analiseId: "",
      dataAplicacao: getLocalDateTimeInput(),
      observacoes: "",
    },
  );
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));
  const produtoSelecionado = produtos.find((p) => p.id === form.produtoId);

  // Veio do botão "Registrar aplicação" na tela de uma Análise: Piscina e
  // Análise já chegam definidas pelo contexto e ficam travadas.
  const contextoTravado = Boolean(initial?.fromAnalise);

  // Filtro em cadeia: produto só após depósito, e só os que têm estoque nele.
  const produtosDoDeposito = form.depositoId
    ? produtos.filter((p) =>
        estoques.some(
          (e) => e?.deposito?.id === form.depositoId && e?.produto?.id === p.id,
        ),
      )
    : [];

  useEffect(() => {
    if (
      form.produtoId &&
      !produtosDoDeposito.some((p) => p.id === form.produtoId)
    ) {
      setForm((f) => ({ ...f, produtoId: "" }));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [form.depositoId]);

  function handleSubmit(e) {
    e.preventDefault();
    const { fromAnalise, ...dto } = form;
    onSubmit({ ...dto, analiseId: form.analiseId || null });
  }

  const analisesDaPiscina = analises.filter(
    (a) => a?.piscina?.id === form.piscinaId,
  );

  if (refsLoading) {
    return (
      <div style={{ padding: "24px 0", textAlign: "center" }}>
        <LoadingSpinner />
        <p style={{ fontSize: 13, color: "#6B8CAE", marginTop: 8 }}>
          Carregando opções do formulário…
        </p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Piscina *">
          <select
            required
            disabled={contextoTravado}
            style={inputStyle}
            value={form.piscinaId}
            onChange={set("piscinaId")}
          >
            <option value="">Selecione a piscina</option>
            {piscinas.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nome}
              </option>
            ))}
          </select>
        </FormField>
        <FormField
          label={`Análise relacionada ${contextoTravado ? "" : "(opcional)"}`}
        >
          <select
            disabled={contextoTravado}
            style={inputStyle}
            value={form.analiseId}
            onChange={set("analiseId")}
          >
            <option value="">Nenhuma</option>
            {analisesDaPiscina.map((a) => (
              <option key={a.id} value={a.id}>
                {new Date(a.dataAnalise).toLocaleDateString("pt-BR")} — pH{" "}
                {a.ph ?? "—"} / Cloro {a.cloroLivre ?? "—"}
              </option>
            ))}
          </select>
        </FormField>
        <FormField label="Depósito *">
          <select
            required
            style={inputStyle}
            value={form.depositoId}
            onChange={set("depositoId")}
          >
            <option value="">Selecione o depósito</option>
            {depositos.map((d) => (
              <option key={d.id} value={d.id}>
                {d.nome}
              </option>
            ))}
          </select>
        </FormField>
        <FormField label="Produto *">
          <select
            required
            disabled={!form.depositoId}
            style={inputStyle}
            value={form.produtoId}
            onChange={set("produtoId")}
          >
            <option value="">
              {form.depositoId
                ? "Selecione o produto"
                : "Selecione um depósito primeiro"}
            </option>
            {produtosDoDeposito.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nome} ({p.unidadeMedida})
              </option>
            ))}
          </select>
        </FormField>
        <FormField label="Quantidade aplicada *">
          <input
            required
            type="number"
            step="0.0001"
            min="0.0001"
            placeholder="Ex.: 500"
            style={inputStyle}
            value={form.quantidade}
            onChange={set("quantidade")}
          />
        </FormField>
        <FormField label="Unidade da aplicação">
          <select
            style={inputStyle}
            value={form.unidadeLancamento}
            onChange={set("unidadeLancamento")}
          >
            <option value="">
              {produtoSelecionado
                ? `Mesma do produto (${produtoSelecionado.unidadeMedida})`
                : "Mesma unidade do produto"}
            </option>
            {UNIDADES_LANCAMENTO.map((u) => (
              <option key={u} value={u}>
                {u}
              </option>
            ))}
          </select>
        </FormField>
        <FormField label="Data e hora *">
          <input
            required
            type="datetime-local"
            style={inputStyle}
            value={form.dataAplicacao}
            onChange={set("dataAplicacao")}
          />
        </FormField>
        <FormField label="Observações" fullWidth>
          <input
            type="text"
            placeholder="Detalhes adicionais sobre a aplicação"
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
        <Button
          variant="ghost"
          onClick={onCancel}
          type="button"
          permission={PERMISSIONS.APLICACOES.CREATE}
        >
          Cancelar
        </Button>
        <Button
          variant="primary"
          type="submit"
          disabled={loading}
          permission={PERMISSIONS.APLICACOES.CREATE}
        >
          {loading ? "Salvando…" : "Registrar aplicação"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Aplicacoes({ prefill, onPrefillConsumed }) {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [modal, setModal] = useState({ open: false, initial: null });
  const [search, setSearch] = useState("");
  const [piscinaGrafico, setPiscinaGrafico] = useState("");

  // Prefill de Análises: form já aberto → refs precisam carregar.
  const formPrecisaRefs = modal.open || Boolean(prefill);

  // Listagem principal — independente das refs.
  const filtrosAplicacoes = { dataInicio: diasAtrasISO(30) };
  const {
    data: aplicacoes = [],
    isLoading: loadingAplicacoes,
    error: errorAplicacoes,
  } = useQuery({
    queryKey: qk.aplicacoes(filtrosAplicacoes),
    queryFn: () => aplicacaoProdutoService.listar(filtrosAplicacoes),
  });

  // Refs compartilhadas — só quando o form precisa; cache 10 min.
  const { data: piscinas = [], isLoading: loadingPiscinas } = useQuery({
    queryKey: qk.piscinas,
    queryFn: () => piscinaService.listar(),
    staleTime: 10 * 60_000,
    // Piscinas também alimentam o select do gráfico; buscar em background
    // mesmo sem modal (não bloqueia a tabela).
    enabled: true,
  });

  const { data: produtos = [], isLoading: loadingProdutos } = useQuery({
    queryKey: qk.produtos,
    queryFn: () => produtoService.listar(),
    staleTime: 10 * 60_000,
    enabled: formPrecisaRefs,
  });

  const { data: depositos = [], isLoading: loadingDepositos } = useQuery({
    queryKey: qk.depositos,
    queryFn: () => depositoService.listar(),
    staleTime: 10 * 60_000,
    enabled: formPrecisaRefs,
  });

  const { data: estoques = [], isLoading: loadingEstoques } = useQuery({
    queryKey: qk.estoques("todos"),
    queryFn: () => estoqueService.listar(),
    staleTime: 10 * 60_000,
    enabled: formPrecisaRefs,
  });

  // Análises só no modal (select opcional / prefill travado).
  const filtrosAnalisesForm = { dataInicio: diasAtrasISO(60), limit: 100 };
  const { data: analises = [], isLoading: loadingAnalises } = useQuery({
    queryKey: qk.analises(filtrosAnalisesForm),
    queryFn: () => analiseService.listar(filtrosAnalisesForm),
    enabled: formPrecisaRefs,
  });

  const refsLoading =
    formPrecisaRefs &&
    (loadingPiscinas ||
      loadingProdutos ||
      loadingDepositos ||
      loadingEstoques ||
      loadingAnalises);

  // Prefill: abre modal já com piscina + análise travadas.
  useEffect(() => {
    if (prefill) {
      setModal({
        open: true,
        initial: {
          piscinaId: prefill.piscinaId ?? "",
          produtoId: "",
          depositoId: "",
          quantidade: "",
          unidadeLancamento: "",
          analiseId: prefill.analiseId ?? "",
          dataAplicacao: getLocalDateTimeInput(),
          observacoes: "",
          fromAnalise: true,
        },
      });
      onPrefillConsumed?.();
    }
  }, [prefill]);

  // Default do gráfico: primeira piscina.
  useEffect(() => {
    if (!piscinaGrafico && piscinas.length > 0) {
      setPiscinaGrafico(piscinas[0].id);
    }
  }, [piscinas, piscinaGrafico]);

  async function handleSave(dto) {
    try {
      setSaving(true);
      setError(null);
      await aplicacaoProdutoService.criar({
        ...dto,
        usuarioId: user?.userId,
      });
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["aplicacoes"] }),
        queryClient.invalidateQueries({ queryKey: ["estoques"] }),
        queryClient.invalidateQueries({ queryKey: ["movimentacoes"] }),
      ]);
      setModal({ open: false, initial: null });
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  const filtered = aplicacoes.filter((a) =>
    `${a.produto?.nome} ${a.piscina?.nome}`
      .toLowerCase()
      .includes(search.toLowerCase()),
  );

  const columns = [
    {
      key: "piscina",
      label: "Piscina",
      render: (_, r) => r.piscina?.nome ?? "—",
    },
    {
      key: "produto",
      label: "Produto",
      render: (_, r) => r.produto?.nome ?? "—",
    },
    {
      key: "deposito",
      label: "Depósito",
      render: (_, r) => r.deposito?.nome ?? "—",
    },
    {
      key: "quantidade",
      label: "Quantidade",
      render: (v, r) => `${v} ${r.unidadeLancamento || ""}`,
    },
    {
      key: "dataAplicacao",
      label: "Data",
      render: (v) => new Date(v).toLocaleString("pt-BR"),
    },
    {
      key: "analiseId",
      label: "Análise relacionada",
      render: (v) => (v ? "Vinculada" : "—"),
    },
  ];

  // Só bloqueia a página na listagem principal — refs não seguram a tela.
  if (loadingAplicacoes) return <LoadingSpinner />;

  const piscinaGraficoNome =
    piscinas.find((p) => p.id === piscinaGrafico)?.nome ?? "";

  return (
    <ProtecaoDeRota permissao={PERMISSIONS.APLICACOES.VIEW}>
      <div>
        <PageHeader
          title="Aplicações de produto"
          description="Registre o uso de produtos nas piscinas — o estoque é atualizado automaticamente"
          action={
            <Button
              variant="primary"
              onClick={() => setModal({ open: true, initial: null })}
              permission={PERMISSIONS.APLICACOES.CREATE}
            >
              + Registrar aplicação
            </Button>
          }
        />

        {(error || errorAplicacoes) && (
          <ErrorMessage message={error || errorAplicacoes?.message} />
        )}

        {piscinas.length > 0 && (
          <div style={{ marginBottom: 8 }}>
            <label
              style={{
                display: "block",
                fontSize: 12,
                fontWeight: 600,
                marginBottom: 6,
                color: "#0A1628",
              }}
            >
              Piscina do gráfico de uso
            </label>
            <select
              style={{ ...inputStyle, maxWidth: 320, marginBottom: 8 }}
              value={piscinaGrafico}
              onChange={(e) => setPiscinaGrafico(e.target.value)}
            >
              {piscinas.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.nome}
                </option>
              ))}
            </select>
            {piscinaGrafico && (
              <Suspense fallback={null}>
                <UsoProdutosCard
                  piscinaId={piscinaGrafico}
                  piscinaNome={piscinaGraficoNome}
                />
              </Suspense>
            )}
          </div>
        )}

        <Toolbar>
          <SearchInput
            value={search}
            onChange={setSearch}
            placeholder="Buscar produto ou piscina…"
          />
        </Toolbar>

        <Card noPadding>
          <DataTable
            columns={columns}
            data={filtered}
            emptyMessage="Nenhuma aplicação registrada."
          />
        </Card>

        <Modal
          open={modal.open}
          onClose={() => setModal({ open: false, initial: null })}
          title="Registrar aplicação de produto"
        >
          <AplicacaoForm
            piscinas={piscinas}
            produtos={produtos}
            depositos={depositos}
            analises={analises}
            estoques={estoques}
            refsLoading={refsLoading}
            initial={modal.initial}
            onSubmit={handleSave}
            onCancel={() => setModal({ open: false, initial: null })}
            loading={saving}
          />
        </Modal>
      </div>
    </ProtecaoDeRota>
  );
}
