// ============================================================
//  Piscina Perfeita — Componente Logo
//
//  Uso:
//    <Logo />                   → logo completa (ícone + wordmark)
//    <Logo variant="icon" />    → só o ícone (sidebar, favicon)
//    <Logo variant="wordmark" /> → só o texto
//    <Logo height={40} />       → tamanho customizado
// ============================================================

// Logo completa (importa o SVG como URL via Vite)
import logoSvg from "../../assets/logo.svg";

export function Logo({ variant = "full", height = 48, className, style }) {
  if (variant === "icon") {
    return <LogoIcon height={height} style={style} className={className} />;
  }

  if (variant === "wordmark") {
    return <LogoWordmark height={height} style={style} className={className} />;
  }

  // full — usa o SVG gerado como asset
  return (
    <img
      src={logoSvg}
      alt="Piscina Perfeita"
      height={height}
      style={{ display: "block", ...style }}
      className={className}
    />
  );
}

// ----------------------------------------------------------
// Ícone inline (gota + fundo arredondado)
// Útil quando não é possível usar o arquivo SVG externo.
// ----------------------------------------------------------
export function LogoIcon({ height = 40, style, className }) {
  const size = height;
  return (
    <svg
      width={size} height={size}
      viewBox="0 0 32 32"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      style={style}
      className={className}
      aria-label="Piscina Perfeita"
    >
      <defs>
        <linearGradient id="pp-bg" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#1E3A5F"/>
          <stop offset="100%" stopColor="#0A1628"/>
        </linearGradient>
        <linearGradient id="pp-drop" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#5BC0EB"/>
          <stop offset="100%" stopColor="#2E86AB"/>
        </linearGradient>
      </defs>
      <rect width="32" height="32" rx="8" fill="url(#pp-bg)"/>
      <path
        d="M16 4C16 4 7 15.5 7 20.5C7 25.2 11.1 29 16 29C20.9 29 25 25.2 25 20.5C25 15.5 16 4 16 4Z"
        fill="url(#pp-drop)"
      />
      <path
        d="M9.5 21.5C11.2 19.5 13.5 20.5 16 21.5C18.5 22.5 20.8 21.5 22.5 19.5"
        stroke="#fff" strokeWidth="1.4" strokeLinecap="round"
      />
      <path
        d="M10.5 24.5C12.2 22.5 14 23.5 16 24.5C18 25.5 19.8 24 21.5 22.5"
        stroke="#fff" strokeWidth="1.1" strokeLinecap="round" opacity="0.5"
      />
      <ellipse cx="12.5" cy="13" rx="1.8" ry="3" fill="#fff" opacity="0.22"
        transform="rotate(-20 12.5 13)"/>
    </svg>
  );
}

// ----------------------------------------------------------
// Wordmark inline (sem ícone)
// ----------------------------------------------------------
export function LogoWordmark({ height = 40, style, className }) {
  return (
    <svg
      viewBox="0 0 190 48"
      height={height}
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      style={style}
      className={className}
      aria-label="Piscina Perfeita"
    >
      <text x="0" y="22"
        fontFamily="Inter, system-ui, sans-serif"
        fontSize="22" fontWeight="700" letterSpacing="-0.4"
        fill="#0A1628">Piscina</text>
      <text x="0" y="44"
        fontFamily="Inter, system-ui, sans-serif"
        fontSize="22" fontWeight="700" letterSpacing="-0.4"
        fill="#2E86AB">Perfeita</text>
      <line x1="0" y1="47.5" x2="114" y2="47.5"
        stroke="#2E86AB" strokeWidth="2" strokeLinecap="round"/>
    </svg>
  );
}
