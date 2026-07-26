// ProtecaoDeRota.jsx
import Forbiden from "../modules/Forbidden/index.jsx";
import { useCan } from "../context/AuthContext.jsx";

export default function ProtecaoDeRota({ user, permissao, children }) {
  const permitido = useCan(permissao);

  return permitido ? children : <Forbiden />;
}
