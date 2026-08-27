import { useEffect, useMemo, useState } from "react";
import {
  PageHeader,
  Card,
  Button,
  FormField,
  LoadingSpinner,
  ErrorMessage,
  Badge,
} from "../../components/ui/index.jsx";
import { inputStyle } from "../../components/ui/styles.js";
import {
  depositoService,
  estoqueService,
  movimentacaoService,
  produtoService,
} from "../../config/services.js";
import {
  TIPO_MOVIMENTACAO,
  TIPO_LABELS,
  UNIDADES_LANCAMENTO,
} from "../../config/index.js";
import { PERMISSIONS } from "../../helpers/Permissions.js";
import ProtecaoDeRota from "../../helpers/ProtecaoDeRota.jsx";

const tipos = [
  TIPO_MOVIMENTACAO.ENTRADA,
  TIPO_MOVIMENTACAO.COMPRA,
  TIPO_MOVIMENTACAO.AJUSTE_INVENTARIO,
];
const novaLinha = () => ({
  produtoId: "",
  quantidade: "",
  unidadeLancamento: "",
});

export default function ContagemInventario() {
  const [depositos, setDepositos] = useState([]);
  const [produtos, setProdutos] = useState([]);
  const [estoques, setEstoques] = useState([]);
  const [depositoId, setDepositoId] = useState("");
  const [tipo, setTipo] = useState(TIPO_MOVIMENTACAO.COMPRA);
  const [itens, setItens] = useState([novaLinha()]);
  const [contagens, setContagens] = useState({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [resultado, setResultado] = useState([]);
  useEffect(() => {
    async function carregar() {
      try {
        const [d, p, e] = await Promise.all([
          depositoService.listar(),
          produtoService.listar(),
          estoqueService.listar(),
        ]);
        setDepositos(d ?? []);
        setProdutos(p ?? []);
        setEstoques(e ?? []);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }
    carregar();
  }, []);
  const saldos = useMemo(
    () =>
      new Map(
        estoques
          .filter((e) => e.deposito?.id === depositoId)
          .map((e) => [e.produto?.id, e.quantidadeAtual ?? 0]),
      ),
    [estoques, depositoId],
  );
  // Produtos elegíveis para Compra/Entrada: só os que já têm Estoque
  // cadastrado nesse depósito (mesmo critério que o modo Ajuste já usava
  // pra listar a tabela de contagem). Cadastro de um produto novo num
  // depósito continua sendo feito na tela de Estoques, não aqui.
  const produtosDoDeposito = useMemo(
    () => (depositoId ? produtos.filter((p) => saldos.has(p.id)) : []),
    [produtos, saldos, depositoId],
  );
  const ehAjuste = tipo === TIPO_MOVIMENTACAO.AJUSTE_INVENTARIO;
  const atualizar = (i, campo, valor) =>
    setItens((atual) =>
      atual.map((item, n) => (n === i ? { ...item, [campo]: valor } : item)),
    );
  const remover = (i) =>
    setItens((atual) =>
      atual.length === 1 ? atual : atual.filter((_, n) => n !== i),
    );
  const atualizarContagem = (estoqueId, valor) =>
    setContagens((atual) => ({ ...atual, [estoqueId]: valor }));
  async function confirmar() {
    const preenchidos = ehAjuste
      ? estoques
          .filter(
            (e) =>
              e.deposito?.id === depositoId &&
              contagens[e.id] !== undefined &&
              contagens[e.id] !== "",
          )
          .map((e) => ({
            produtoId: e.produto?.id,
            quantidade: Number(contagens[e.id]),
            unidadeLancamento: null,
          }))
      : itens.filter((i) => i.produtoId && Number(i.quantidade) > 0);
    if (!depositoId || preenchidos.length === 0) {
      setError(
        "Selecione o depósito e informe ao menos um produto com quantidade válida.",
      );
      return;
    }
    if (
      new Set(preenchidos.map((i) => i.produtoId)).size !== preenchidos.length
    ) {
      setError("Não repita o mesmo produto no lançamento.");
      return;
    }
    try {
      setSaving(true);
      setError(null);
      const res = await movimentacaoService.lancarLoteInventario({
        depositoId,
        tipoMovimentacao: tipo,
        itens: preenchidos.map((i) => ({
          ...i,
          quantidade: Number(i.quantidade),
          unidadeLancamento: i.unidadeLancamento || null,
        })),
      });
      setResultado(res ?? []);
      setItens([novaLinha()]);
      setContagens({});
      setEstoques((await estoqueService.listar()) ?? []);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }
  if (loading) return <LoadingSpinner />;
  return (
    <ProtecaoDeRota permissao={PERMISSIONS.INVENTARIOS.VIEW}>
      <div>
        <PageHeader
          title="Atualização de inventário"
          description="Registre compras, entradas ou ajustes para vários produtos em uma única operação."
        />
        {error && <ErrorMessage message={error} />}
        <Card>
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))",
              gap: 12,
              marginBottom: 16,
            }}
          >
            <FormField label="Tipo de lançamento *">
              <select
                style={inputStyle}
                value={tipo}
                onChange={(e) => setTipo(Number(e.target.value))}
              >
                {tipos.map((t) => (
                  <option key={t} value={t}>
                    {TIPO_LABELS[t]}
                  </option>
                ))}
              </select>
            </FormField>
            <FormField label="Depósito *">
              <select
                required
                style={inputStyle}
                value={depositoId}
                onChange={(e) => {
                  setDepositoId(e.target.value);
                  setResultado([]);
                  setItens([novaLinha()]);
                  setContagens({});
                }}
              >
                <option value="">Selecione o depósito</option>
                {depositos.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.nome}
                  </option>
                ))}
              </select>
            </FormField>
          </div>
          <p style={{ fontSize: 13, color: "#5a6b7a" }}>
            {ehAjuste
              ? "Informe apenas o saldo físico contado nos produtos do depósito. O sistema calcula a diferença."
              : "Cada linha gera sua própria movimentação, agrupada e confirmada de uma só vez."}
          </p>
          {ehAjuste ? (
            <>
              <div style={{ overflowX: "auto" }}>
                <table
                  style={{
                    width: "100%",
                    borderCollapse: "collapse",
                    fontSize: 13,
                  }}
                >
                  <thead>
                    <tr
                      style={{
                        textAlign: "left",
                        borderBottom: "1px solid #e7ecf0",
                      }}
                    >
                      <th style={{ padding: 10 }}>Produto</th>
                      <th style={{ padding: 10 }}>Saldo atual</th>
                      <th style={{ padding: 10 }}>Saldo contado</th>
                      <th style={{ padding: 10 }}>Diferença</th>
                    </tr>
                  </thead>
                  <tbody>
                    {estoques
                      .filter((e) => e.deposito?.id === depositoId)
                      .map((e) => {
                        const contado = contagens[e.id];
                        const diferenca =
                          contado === undefined || contado === ""
                            ? null
                            : Number(contado) - (e.quantidadeAtual ?? 0);
                        return (
                          <tr
                            key={e.id}
                            style={{ borderBottom: "1px solid #f2f5f7" }}
                          >
                            <td style={{ padding: 10 }}>
                              {e.produto?.nome ?? "—"}
                            </td>
                            <td style={{ padding: 10 }}>
                              {e.quantidadeAtual ?? 0}{" "}
                              {e.produto?.unidadeMedida ?? ""}
                            </td>
                            <td style={{ padding: 10 }}>
                              <input
                                type="number"
                                min="0"
                                step="0.0001"
                                placeholder="—"
                                style={{ ...inputStyle, width: 120 }}
                                value={contado ?? ""}
                                onChange={(ev) =>
                                  atualizarContagem(e.id, ev.target.value)
                                }
                              />
                            </td>
                            <td style={{ padding: 10 }}>
                              {diferenca === null ? (
                                "—"
                              ) : diferenca === 0 ? (
                                <Badge variant="ok">Sem diferença</Badge>
                              ) : (
                                <Badge variant={diferenca > 0 ? "info" : "bad"}>
                                  {diferenca > 0 ? "+" : ""}
                                  {diferenca.toFixed(4)}
                                </Badge>
                              )}
                            </td>
                          </tr>
                        );
                      })}
                  </tbody>
                </table>
              </div>
              <div
                style={{
                  display: "flex",
                  justifyContent: "flex-end",
                  marginTop: 16,
                }}
              >
                <Button
                  type="button"
                  variant="primary"
                  disabled={saving}
                  onClick={confirmar}
                  permission={PERMISSIONS.INVENTARIOS.CREATE}
                >
                  {saving ? "Confirmando…" : "Confirmar ajuste"}
                </Button>
              </div>
            </>
          ) : (
            <>
              <div style={{ overflowX: "auto" }}>
                <table
                  style={{
                    width: "100%",
                    borderCollapse: "collapse",
                    fontSize: 13,
                  }}
                >
                  <thead>
                    <tr
                      style={{
                        textAlign: "left",
                        borderBottom: "1px solid #e7ecf0",
                      }}
                    >
                      <th style={{ padding: 10 }}>Produto</th>
                      <th style={{ padding: 10 }}>Saldo atual</th>
                      <th style={{ padding: 10 }}>Quantidade</th>
                      <th style={{ padding: 10 }}>Unidade</th>
                      <th />
                    </tr>
                  </thead>
                  <tbody>
                    {itens.map((item, i) => {
                      const produto = produtos.find(
                        (p) => p.id === item.produtoId,
                      );
                      return (
                        <tr
                          key={i}
                          style={{ borderBottom: "1px solid #f2f5f7" }}
                        >
                          <td style={{ padding: 10 }}>
                            <select
                              style={inputStyle}
                              value={item.produtoId}
                              disabled={!depositoId}
                              title={
                                !depositoId
                                  ? "Selecione um depósito primeiro"
                                  : undefined
                              }
                              onChange={(e) =>
                                atualizar(i, "produtoId", e.target.value)
                              }
                            >
                              <option value="">
                                {depositoId
                                  ? "Selecione"
                                  : "Selecione um depósito primeiro"}
                              </option>
                              {produtosDoDeposito.map((p) => (
                                <option key={p.id} value={p.id}>
                                  {p.nome}
                                </option>
                              ))}
                            </select>
                          </td>
                          <td style={{ padding: 10 }}>
                            {item.produtoId
                              ? `${saldos.get(item.produtoId) ?? 0} ${produto?.unidadeMedida ?? ""}`
                              : "—"}
                          </td>
                          <td style={{ padding: 10 }}>
                            <input
                              type="number"
                              min="0.0001"
                              step="0.0001"
                              style={{ ...inputStyle, width: 120 }}
                              value={item.quantidade}
                              onChange={(e) =>
                                atualizar(i, "quantidade", e.target.value)
                              }
                            />
                          </td>
                          <td style={{ padding: 10 }}>
                            <select
                              style={inputStyle}
                              value={item.unidadeLancamento}
                              onChange={(e) =>
                                atualizar(
                                  i,
                                  "unidadeLancamento",
                                  e.target.value,
                                )
                              }
                            >
                              <option value="">Unidade do produto</option>
                              {UNIDADES_LANCAMENTO.map((u) => (
                                <option key={u} value={u}>
                                  {u}
                                </option>
                              ))}
                            </select>
                          </td>
                          <td style={{ padding: 10 }}>
                            <Button
                              type="button"
                              variant="ghost"
                              onClick={() => remover(i)}
                              disabled={itens.length === 1}
                            >
                              Remover
                            </Button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
              <div
                style={{
                  display: "flex",
                  justifyContent: "space-between",
                  gap: 8,
                  marginTop: 16,
                }}
              >
                <Button
                  type="button"
                  variant="ghost"
                  onClick={() => setItens((atual) => [...atual, novaLinha()])}
                >
                  + Adicionar produto
                </Button>
                <Button
                  type="button"
                  variant="primary"
                  disabled={saving}
                  onClick={confirmar}
                  permission={PERMISSIONS.INVENTARIOS.CREATE}
                >
                  {saving ? "Confirmando…" : "Confirmar lançamento"}
                </Button>
              </div>
            </>
          )}
        </Card>
        {resultado.length > 0 && (
          <Card>
            <h3 style={{ fontSize: 14, marginTop: 0 }}>Lançamento concluído</h3>
            {resultado.map((r) => (
              <p key={r.produtoId}>
                <Badge variant="ok">Registrado</Badge> {r.produtoNome}: saldo{" "}
                {r.quantidadeAnterior} → {r.quantidadeAtual}
              </p>
            ))}
          </Card>
        )}
      </div>
    </ProtecaoDeRota>
  );
}
