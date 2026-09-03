// ============================================================
//  Piscina Perfeita — hook: useUsuariosSelecionaveis
// ============================================================
import { useQuery } from "@tanstack/react-query";
import { useCan } from "../context/AuthContext.jsx";
import { usuarioService } from "../config/services.js";
import { PERMISSIONS } from "./../helpers/Permissions.js";
import { PERFIS } from "../config/index.js";
import { qk } from "../helpers/queryKeys.js";

export function useUsuariosSelecionaveis() {
  const podeVer = useCan(PERMISSIONS.GERAL.VIEW_USUARIO_SELETOR);

  const { data: usuarios = [] } = useQuery({
    queryKey: qk.usuarios,
    queryFn: () => usuarioService.listar(),
    enabled: !!podeVer,
    staleTime: 5 * 60_000,
    select: (lista) =>
      (lista ?? []).filter(
        (u) =>
          u.perfil === PERFIS.OPERADOR || u.perfil === PERFIS.ADMINISTRADOR,
      ),
  });

  return { usuarios: podeVer ? usuarios : [], podeVerUsuario: podeVer };
}
