import { useState, useEffect } from "react";
import { usuarioLocalService } from "../../../config/services.js";

/**
 * Encapsula a busca dos Locais vinculados ao usuário logado.
 * Nenhum componente visual deve chamar usuarioLocalService diretamente.
 */
export function useLocaisDoUsuario(localId) {
  const [locais, setLocais] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let ativo = true;
    setIsLoading(true);

    usuarioLocalService
      .meusLocais()
      .then((res) => {
        if (!ativo) return;
        setLocais(res ?? []);
        setError(null);
      })
      .catch((err) => {
        if (!ativo) return;
        // silencioso por design: indicador é acessório, não trava a tela
        setError(err);
      })
      .finally(() => {
        if (ativo) setIsLoading(false);
      });

    return () => {
      ativo = false;
    };
  }, [localId]);

  return { locais, isLoading, error };
}
