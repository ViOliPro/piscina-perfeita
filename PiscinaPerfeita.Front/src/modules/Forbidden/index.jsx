export default function Forbidden() {
  return (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        minHeight: "100vh",
        background: "#f8fafc",
        padding: "20px",
      }}
    >
      <div style={{ textAlign: "center", maxWidth: "500px" }}>
        <h1 style={{ fontSize: "72px", margin: 0 }}>403</h1>

        <h2>Acesso negado</h2>

        <p style={{ color: "#666", lineHeight: 1.6 }}>
          Você não possui permissão para acessar esta página. Se acredita que
          deveria ter acesso, entre em contato com o administrador.
        </p>
      </div>
    </div>
  );
}
