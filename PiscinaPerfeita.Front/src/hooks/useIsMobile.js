// ============================================================
//  Piscina Perfeita — Hook: useIsMobile
//  Detecta se a viewport está abaixo do breakpoint mobile (768px).
//  Reativo: atualiza automaticamente ao redimensionar.
// ============================================================
import { useState, useEffect } from "react";

const MOBILE_BREAKPOINT = 768;

export function useIsMobile() {
  const [isMobile, setIsMobile] = useState(
    () => window.matchMedia(`(max-width: ${MOBILE_BREAKPOINT}px)`).matches
  );

  useEffect(() => {
    const mq = window.matchMedia(`(max-width: ${MOBILE_BREAKPOINT}px)`);
    const handler = (e) => setIsMobile(e.matches);

    // Usa addEventListener moderno, com fallback para addListener legado
    if (mq.addEventListener) {
      mq.addEventListener("change", handler);
      return () => mq.removeEventListener("change", handler);
    } else {
      mq.addListener(handler);
      return () => mq.removeListener(handler);
    }
  }, []);

  return isMobile;
}
