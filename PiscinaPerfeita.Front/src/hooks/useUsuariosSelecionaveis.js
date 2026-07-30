// ============================================================
//  Piscina Perfeita — hook: useUsuariosSelecionaveis
//
//  Centraliza a regra de negócio para qualquer formulário que precise
//  de um seletor de "usuário responsável" (Estoque, Movimentações, etc.):
//
//    - Visualizador e Operador: NUNCA veem nem selecionam outro usuário.
//      Para Operador em especial, a API sequer é chamada (nenhuma
//      requisição de listagem de usuários deve sair do front).
//    - Administrador/SuperAdmin: veem a lista completa de usuários do
//      Local atual, restrita a quem realmente pode ser "responsável"
//      por um lançamento (Perfil Operador ou Administrador — um
//      Visualizador nunca atua, então não faz sentido aparecer aqui).
// ============================================================
import { useState, useEffect } from "react";
import { useCan } from "../context/AuthContext.jsx";
import { usuarioService } from "../config/services.js";
import { PERMISSIONS } from "./../helpers/Permissions.js";
import { PERFIS } from "../config/index.js";

export function useUsuariosSelecionaveis() {
  const podeVer = useCan(PERMISSIONS.GERAL.VIEW_USUARIO_SELETOR);
  const [usuarios, setUsuarios] = useState([]);

  useEffect(() => {
    // Operador/Visualizador: nem chega a chamar a API de usuários.
    if (!podeVer) {
      setUsuarios([]);
      return;
    }

    let ativo = true;
    usuarioService
      .listar()
      .then((lista) => {
        if (!ativo) return;
        // O backend já filtra por Local atual — aqui só restringimos por
        // Perfil (Operador e Administrador), conforme a regra de negócio.
        setUsuarios(
          (lista ?? []).filter(
            (u) =>
              u.perfil === PERFIS.OPERADOR || u.perfil === PERFIS.ADMINISTRADOR,
          ),
        );
      })
      .catch(() => {
        if (ativo) setUsuarios([]);
      });

    return () => {
      ativo = false;
    };
  }, [podeVer]);

  return { usuarios, podeVerUsuario: podeVer };
}
