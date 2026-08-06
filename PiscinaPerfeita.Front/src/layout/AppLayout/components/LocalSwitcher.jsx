import { useState } from "react";
import { useAuth } from "../../../context/AuthContext.jsx";
import { useLocaisDoUsuario } from "../hooks/useLocaisDoUsuario.js";
import styles from "./components.module.css";

export function LocalSwitcher() {
  const { user, switchLocal } = useAuth();
  const { locais } = useLocaisDoUsuario(user?.localId);
  const [open, setOpen] = useState(false);
  const [busy, setBusy] = useState(false);

  const localAtual = locais.find((l) => l.localId === user?.localId);
  const nomeAtual = localAtual?.localNome ?? "Local não definido";

  // Só um local vinculado (ou nenhum ainda carregado): mostra badge estático
  if (locais.length <= 1) {
    return (
      <span title={nomeAtual} className={styles.localSwitcherBadge}>
        📍 {nomeAtual}
      </span>
    );
  }

  async function handleSwitch(localId) {
    if (localId === user?.localId) {
      setOpen(false);
      return;
    }
    setBusy(true);
    const ok = await switchLocal(localId);
    setBusy(false);
    setOpen(false);
    if (ok) window.location.reload();
  }

  return (
    <div className={styles.localSwitcherWrap}>
      <button
        onClick={() => setOpen((o) => !o)}
        disabled={busy}
        className={styles.localSwitcherButton}
      >
        📍 <span className={styles.localSwitcherLabel}>{nomeAtual}</span> ▾
      </button>

      {open && (
        <div className={styles.localSwitcherDropdown}>
          {locais.map((l) => (
            <button
              key={l.id}
              onClick={() => handleSwitch(l.localId)}
              className={`${styles.localSwitcherOption} ${
                l.localId === user?.localId
                  ? styles.localSwitcherOptionActive
                  : ""
              }`}
            >
              {l.localId === user?.localId ? "✓ " : ""}
              {l.localNome ?? "Local sem nome"}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
