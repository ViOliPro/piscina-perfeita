// ============================================================
//  Piscina Perfeita — Módulo: Contagem de Inventário (Ajuste)
//
//  Objetivo: mitigar perdas ou desvios de produtos. O usuário escolhe um
//  Depósito, o sistema lista os produtos com o saldo lógico atual, e o
//  usuário preenche quanto contou fisicamente de cada um. A diferença é
//  calculada automaticamente e, ao confirmar, o backend gera uma
//  MovimentacaoEstoque do tipo "Ajuste de Inventário" para cada produto
//  com divergência (produtos que batem certinho não geram ajuste).
// ============================================================
import { useState, useEffect } from "react";
import {
  PageHeader, Card, Button, FormField, inputStyle,
  LoadingSpinner, ErrorMessage, DataTable, Badge,
} from "../../components/ui/index.jsx";
import { estoqueService, depositoService, movimentacaoService } from "../../config/services.js";
import { useAuth } from "../../context/AuthContext.jsx";

export default function ContagemInventario() {
  const { user } = useAuth();
  const [depositos, setDepositos] = useState([]);
  const [depositoId, setDepositoId] = useState("");
  const [estoques, setEstoques] = useState([]);
  const [contagens, setContagens] = useState({}); // { estoqueId: "quantidade digitada" }
  const [loading, setLoading] = useState(true);
  const [loadingEstoques, setLoadingEstoques] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [resultado, setResultado] = useState(null); // resultado da última contagem enviada

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

  useEffect(() => {
    if (!depositoId) { setEstoques([]); return; }
    async function loadEstoques() {
      try {
        setLoadingEstoques(true);
        setError(null);
        setResultado(null);
        const todos = (await estoqueService.listar()) ?? [];
        // Filtra pelo depósito escolhido — o Estoque retorna { deposito: {id, nome} }.
        const doDeposito = todos.filter((e) => e.deposito?.id === depositoId);
        setEstoques(doDeposito);
        setContagens({});
      } catch (err) { setError(err.message); }
      finally { setLoadingEstoques(false); }
    }
    loadEstoques();
  }, [depositoId]);

  function setContagem(estoqueId, valor) {
    setContagens((prev) => ({ ...prev, [estoqueId]: valor }));
  }

  async function handleConfirmar() {
    const itens = estoques
      .filter((e) => contagens[e.id] !== undefined && contagens[e.id] !== "")
      .map((e) => ({
        produtoId: e.produto?.id,
        quantidadeContada: contagens[e.id],
      }));

    if (itens.length === 0) {
      setError("Preencha ao menos uma quantidade contada antes de confirmar.");
      return;
    }

    try {
      setSaving(true);
      setError(null);
      const res = await movimentacaoService.contagemInventario(depositoId, user?.userId, itens);
      setResultado(res ?? []);
      setContagens({});
      // Recarrega os estoques do depósito pra refletir os novos saldos.
      const todos = (await estoqueService.listar()) ?? [];
      setEstoques(todos.filter((e) => e.deposito?.id === depositoId));
    } catch (err) { setError(err.message); }
    finally { setSaving(false); }
  }

  if (loading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader
        title="Contagem de inventário"
        description="Confira o saldo físico dos produtos e ajuste divergências automaticamente"
      />

      {error && <ErrorMessage message={error} />}

      <Card>
        <FormField label="Depósito a conferir *">
          <select
            style={{ ...inputStyle, maxWidth: 320 }}
            value={depositoId}
            onChange={(e) => setDepositoId(e.target.value)}
          >
            <option value="">Selecione um depósito…</option>
            {depositos.map((d) => <option key={d.id} value={d.id}>{d.nome}</option>)}
          </select>
        </FormField>
      </Card>

      {loadingEstoques && <LoadingSpinner />}

      {!loadingEstoques && depositoId && estoques.length === 0 && (
        <Card>
          <p style={{ fontSize: 13, color: "#5a6b7a" }}>
            Nenhum produto com estoque cadastrado neste depósito ainda.
          </p>
        </Card>
      )}

      {!loadingEstoques && estoques.length > 0 && (
        <Card noPadding>
          <table style={{ width: "100%", borderCollapse: "collapse", fontSize: 13 }}>
            <thead>
              <tr style={{ textAlign: "left", borderBottom: "1px solid #e7ecf0" }}>
                <th style={{ padding: "10px 12px" }}>Produto</th>
                <th style={{ padding: "10px 12px" }}>Saldo lógico (sistema)</th>
                <th style={{ padding: "10px 12px" }}>Quantidade contada</th>
                <th style={{ padding: "10px 12px" }}>Diferença</th>
              </tr>
            </thead>
            <tbody>
              {estoques.map((e) => {
                const contado = contagens[e.id];
                const diferenca = contado !== undefined && contado !== ""
                  ? parseFloat(contado) - (e.quantidadeAtual ?? 0)
                  : null;
                return (
                  <tr key={e.id} style={{ borderBottom: "1px solid #f2f5f7" }}>
                    <td style={{ padding: "10px 12px" }}>{e.produto?.nome ?? "—"}</td>
                    <td style={{ padding: "10px 12px" }}>
                      {e.quantidadeAtual ?? 0} {e.produto?.unidadeMedida ?? ""}
                    </td>
                    <td style={{ padding: "10px 12px" }}>
                      <input
                        type="number" step="0.0001" min="0" placeholder="—"
                        style={{ ...inputStyle, width: 120 }}
                        value={contado ?? ""}
                        onChange={(ev) => setContagem(e.id, ev.target.value)}
                      />
                    </td>
                    <td style={{ padding: "10px 12px" }}>
                      {diferenca === null ? (
                        "—"
                      ) : diferenca === 0 ? (
                        <Badge variant="ok">Sem diferença</Badge>
                      ) : (
                        <Badge variant={diferenca > 0 ? "info" : "bad"}>
                          {diferenca > 0 ? "+" : ""}{diferenca.toFixed(4)} {e.produto?.unidadeMedida ?? ""}
                        </Badge>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
          <div style={{ display: "flex", justifyContent: "flex-end", padding: 12 }}>
            <Button variant="primary" onClick={handleConfirmar} disabled={saving}>
              {saving ? "Confirmando…" : "Confirmar contagem"}
            </Button>
          </div>
        </Card>
      )}

      {resultado && (
        <Card>
          <h3 style={{ fontSize: 14, marginTop: 0 }}>Resultado da contagem</h3>
          <DataTable
            columns={[
              { key: "produtoNome", label: "Produto" },
              { key: "quantidadeAnterior", label: "Saldo anterior" },
              { key: "quantidadeContada", label: "Contado" },
              {
                key: "diferenca", label: "Ajuste gerado",
                render: (v, r) => v === 0
                  ? <Badge variant="ok">Sem ajuste</Badge>
                  : <Badge variant={v > 0 ? "info" : "bad"}>{v > 0 ? "+" : ""}{v}</Badge>,
              },
            ]}
            data={resultado}
            emptyMessage="—"
          />
        </Card>
      )}
    </div>
  );
}
