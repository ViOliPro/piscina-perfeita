import { useCallback, useEffect, useState } from "react";
import {
  analiseService,
  estoqueService,
  movimentacaoService,
  piscinaService,
} from "../../../config/services.js";

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
      const [p, a, e, m] = await Promise.all([
        piscinaService.listar(),
        analiseService.listar(),
        estoqueService.listarBaixo(),
        movimentacaoService.listar(),
      ]);
      setPiscinas(p ?? []);
      setAnalises(a ?? []);
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
