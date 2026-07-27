import { PERFIS, PERFIL_LABELS, ROLE_LABELS, ROLES } from "../config/index";
import { useAuth } from "../context/AuthContext";

const PERFIL_ADMINISTRADOR = PERFIL_LABELS[PERFIS.ADMINISTRADOR];
const PERFIL_OPERADOR = PERFIL_LABELS[PERFIS.OPERADOR];
const PERFIL_VISUALIZADOR = PERFIL_LABELS[PERFIS.VISUALIZADOR];
const ROLE_SUPERADMIN = ROLE_LABELS[ROLES.ADMIN];

export const PERMISSIONS = {
  LOCAIS: {
    VIEW: "locais.view",
    CREATE: "locais.create",
    EDIT: "locais.edit",
    DELETE: "locais.delete",
  },
  PRODUTOS: {
    VIEW: "produtos.view",
    CREATE: "produtos.create",
    EDIT: "produtos.edit",
    DELETE: "produtos.delete",
  },
  USUARIOS: {
    VIEW: "usuarios.view",
    CREATE: "usuarios.create",
    EDIT: "usuarios.edit",
    DELETE: "usuarios.delete",
    VINCULO: "usuarios.vinculo",
  },
  ANALISES: {
    VIEW: "analises.view",
    CREATE: "analises.create",
    EDIT: "analises.edit",
    DELETE: "analises.delete",
    VIEW_BTN: "analises.ViewBtn",
  },
  PISCINAS: {
    VIEW: "piscinas.view",
    CREATE: "piscinas.create",
    EDIT: "piscinas.edit",
    DELETE: "piscinas.delete",
  },
  CADASTROS: {
    VIEW: "cadastros.view",
    CREATE: "cadastros.create",
    EDIT: "cadastros.edit",
    DELETE: "cadastros.delete",
  },
  DEPOSITOS: {
    VIEW: "depositos.view",
    CREATE: "depositos.create",
    EDIT: "depositos.edit",
    DELETE: "depositos.delete",
  },
  APLICACOES: {
    VIEW: "aplicacoes.view",
    CREATE: "aplicacoes.create",
    EDIT: "aplicacoes.edit",
    DELETE: "aplicacoes.delete",
  },
  ESTOQUES: {
    VIEW: "estoques.view",
    CREATE: "estoques.create",
    EDIT: "estoques.edit",
    DELETE: "estoques.delete",
    MOVIMENTAR: "estoques.movimentar",
    INVENTARIO: "estoques.inventario",
  },
  MOVIMENTACOES: {
    VIEW: "movimentacoes.view",
    CREATE: "movimentacoes.create",
    EDIT: "movimentacoes.edit",
    DELETE: "movimentacoes.delete",
    VIEW_INPUT_USUARIOS: "movimentacoes.viewInput",
  },
  INVENTARIOS: {
    VIEW: "inventarios.view",
    CREATE: "inventarios.create",
    EDIT: "inventarios.edit",
    DELETE: "inventarios.delete",
  },
};

export const USER_PERMISSIONS = {
  [PERFIL_LABELS[PERFIS.ADMINISTRADOR]]: [
    ...Object.values(PERMISSIONS).flatMap((modulo) => Object.values(modulo)),
  ],
  [ROLE_LABELS[ROLES.ADMIN]]: [
    ...Object.values(PERMISSIONS).flatMap((modulo) => Object.values(modulo)),
  ],
  [PERFIL_LABELS[PERFIS.OPERADOR]]: [
    PERMISSIONS.PRODUTOS.VIEW,
    PERMISSIONS.PRODUTOS.CREATE,
    PERMISSIONS.PRODUTOS.EDIT,
    PERMISSIONS.DEPOSITOS.VIEW,
    PERMISSIONS.DEPOSITOS.CREATE,
    PERMISSIONS.DEPOSITOS.EDIT,
    PERMISSIONS.ANALISES.VIEW,
    PERMISSIONS.ANALISES.CREATE,
    PERMISSIONS.APLICACOES.VIEW,
    PERMISSIONS.APLICACOES.CREATE,
    PERMISSIONS.ESTOQUES.VIEW,
    PERMISSIONS.ESTOQUES.CREATE,
    PERMISSIONS.MOVIMENTACOES.VIEW,
    PERMISSIONS.MOVIMENTACOES.CREATE,
    PERMISSIONS.INVENTARIOS.VIEW,
    PERMISSIONS.INVENTARIOS.CREATE,
  ],
  [PERFIL_LABELS[PERFIS.VISUALIZADOR]]: [
    PERMISSIONS.PISCINAS.VIEW,
    PERMISSIONS.PRODUTOS.VIEW,
    PERMISSIONS.DEPOSITOS.VIEW,
    PERMISSIONS.ANALISES.VIEW,
    PERMISSIONS.APLICACOES.VIEW,
    PERMISSIONS.ESTOQUES.VIEW,
    PERMISSIONS.MOVIMENTACOES.VIEW,
    PERMISSIONS.INVENTARIOS.VIEW,
  ],
};

export function can(requiredPermission, user) {
  // SuperAdmin sem perfil, apenas ROLE
  const isSuperAdmin = ROLES.ADMIN === user.role;
  const isPerfilAdministrador = PERFIS.ADMINISTRADOR === user.perfil;

  const permissoesAtribuidas = isSuperAdmin
    ? [ROLE_LABELS[ROLES.ADMIN]]
    : isPerfilAdministrador
      ? [PERFIL_LABELS[PERFIS.ADMINISTRADOR]]
      : [PERFIL_LABELS[user.perfil]];

  // 1. Busca a lista de permissões que esse perfil tem direito
  // Se userPerfil nao for passado utiliza o padrao da aplicacao
  const perfil = permissoesAtribuidas;

  const userPerms = USER_PERMISSIONS[perfil] || [];

  // 2. Se nenhuma permissão foi exigida no NAV, é um item público (ex: Dashboard) -> libera!
  if (!requiredPermission) return true;

  // 3. SE FOR UM OBJETO (Ex: passou PERMISSIONS.LOCAIS inteiro pelo menu de navegação)
  if (typeof requiredPermission === "object") {
    const moduloPerms = Object.values(requiredPermission);

    // ".some" verifica: "O perfil do usuário possui PELO MENOS UMA das permissões desse módulo?"
    return moduloPerms.some((perm) => userPerms.includes(perm));
  }

  // 4. SE FOR UMA STRING DIRETA (Ex: verificando um botão: can(user, PERMISSIONS.LOCAIS.DELETE))
  return userPerms.includes(requiredPermission);
}
