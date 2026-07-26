// ProtecaoDeRota.jsx
import Forbiden from "../modules/Forbidden/index.jsx";
import { can } from "../helpers/Permissions.js";

export default function ProtecaoDeRota({ user, permissao, children }) {
  if (!can(permissao, user)) {
    return <Forbiden />;
  }

  return children;
}
