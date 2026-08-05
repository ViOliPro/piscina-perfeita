import { PERMISSIONS } from "../../helpers/Permissions";
// ----------------------------------------------------------
// Definição da navegação
// ----------------------------------------------------------
export const NAV = [
  { id: "dashboard", label: "Dashboard", icon: "📊", section: "principal" },
  {
    id: "locais",
    label: "Locais",
    icon: "📍",
    section: "cadastros",
    permissions: PERMISSIONS.LOCAIS,
  },
  {
    id: "piscinas",
    label: "Piscinas",
    icon: "🏊",
    section: "cadastros",
    permissions: PERMISSIONS.PISCINAS,
  },
  {
    id: "usuarios",
    label: "Usuários",
    icon: "👥",
    section: "cadastros",
    permissions: PERMISSIONS.USUARIOS,
  },
  {
    id: "produtos",
    label: "Produtos",
    icon: "📦",
    section: "cadastros",
    permissions: PERMISSIONS.PRODUTOS,
  },
  {
    id: "depositos",
    label: "Depósitos",
    icon: "🗄️",
    section: "cadastros",
    permissions: PERMISSIONS.DEPOSITOS,
  },
  {
    id: "analises",
    label: "Análises",
    icon: "🧪",
    section: "operacional",
    permissions: PERMISSIONS.ANALISES,
  },
  {
    id: "aplicacoes",
    label: "Aplicações",
    icon: "💧",
    section: "operacional",
    permissions: PERMISSIONS.APLICACOES,
  },
  {
    id: "estoque",
    label: "Estoque",
    icon: "🗃️",
    section: "operacional",
    permissions: PERMISSIONS.ESTOQUES,
  },
  {
    id: "movimentacoes",
    label: "Movimentações",
    icon: "↔️",
    section: "operacional",
    permissions: PERMISSIONS.MOVIMENTACOES,
  },
  {
    id: "inventario",
    label: "Contagem de Inventário",
    icon: "🔢",
    section: "operacional",
    permissions: PERMISSIONS.INVENTARIOS,
  },
  {
    id: "hidrometro",
    label: "Hidrômetro",
    icon: "🚰",
    section: "operacional",
    permissions: PERMISSIONS.HIDROMETRO,
  },
];
