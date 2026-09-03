import { useCallback, useEffect, useState } from "react";
import {
  analiseService,
  estoqueService,
  movimentacaoService,
  piscinaService,
} from "../../../config/services.js";

function inicioDoMesISO() {
  const agora = new Date();
  return new Date(agora.getFullYear(), agora.getMonth(), 1).toISOString();
}

function diasAtrasISO(dias) {
  const d = new Date();
  d.setDate(d.getDate() - dias);
  d.setHours(0, 0, 0, 0);
  return d.toISOString();
}

/**
 * Encapsula a camada de dados do Dashboard: carregamento de piscinas,
 * análises, estoque baixo e movimentações. Nenhum componente visual
 * deste módulo deve chamar os services diretamente.
 */
export function useDashboardData() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [piscinas, setPiscinas] = useState([]);
  const [analises, setAnalises] = useState([]);
  const [estoqueBaixo, setEstoqueBaixo] = useState([]);
  const [movimentos, setMovimentos] = useState([]);

  const carregarDados = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      // Movimentações: últimos 14 dias (bem menos que o mês inteiro)
      // Análises: backend ainda não aceita filtro no GET listar —
      // limitamos no cliente o que a UI precisa (cards usam poucas)
      const [p, a, e, m] = await Promise.all([
        piscinaService.listar(),
        analiseService.listar(),
        estoqueService.listarBaixo(),
        movimentacaoService.listar({
          dataInicio: diasAtrasISO(14),
        }),
      ]);

      setPiscinas(p ?? []);
      // UI do dashboard só usa: analises[0], filter de hoje, e lista curta
      setAnalises((a ?? []).slice(0, 10));
      setEstoqueBaixo(e ?? []);
      setMovimentos((m ?? []).slice(0, 5));
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    carregarDados();
  }, [carregarDados]);

  return { loading, error, piscinas, analises, estoqueBaixo, movimentos };
}
