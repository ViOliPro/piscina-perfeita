// ============================================================
//  Piscina Perfeita — Onboarding: primeiro Local do Administrador
//
//  Um usuário com Perfil Administrador pode ser criado sem nenhum
//  Local vinculado (ex.: um síndico profissional recém-cadastrado).
//  Antes de liberar o resto do sistema — que depende de um Local
//  ativo para funcionar — pedimos que ele crie seu primeiro
//  condomínio/unidade aqui.
// ============================================================
import { useState } from "react";
import {
  Card, Button, FormGrid, FormField, inputStyle, ErrorMessage,
} from "../../components/ui/index.jsx";
import { localService } from "../../config/services.js";
import { useAuth } from "../../context/AuthContext.jsx";
import { LogoIcon } from "../../components/ui/Logo.jsx";

export default function PrimeiroLocal() {
  const { user, switchLocal, logout } = useAuth();
  const [form, setForm] = useState({
    nome: "", descricao: "", telefone: "", cep: "",
    endereco: "", cidade: "", estado: "", pais: "", observacoes: "",
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  async function handleSubmit(e) {
    e.preventDefault();
    try {
      setSaving(true);
      setError(null);
      const novoLocal = await localService.criar(form);
      // O backend já vinculou este usuário (Administrador) ao Local recém
      // criado — agora só precisamos trocar para ele para obter um token
      // com o local_id correto e liberar o resto do sistema.
      const ok = await switchLocal(novoLocal.id);
      if (ok) window.location.reload();
      else setError("Local criado, mas não foi possível ativá-lo automaticamente. Tente relogar.");
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  return (
    <div style={{
      minHeight: "100vh", display: "flex", alignItems: "center", justifyContent: "center",
      background: "#f4f7f9", padding: 16,
    }}>
      <div style={{ width: "100%", maxWidth: 560 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10, justifyContent: "center", marginBottom: 20 }}>
          <LogoIcon height={32} />
          <span style={{ fontWeight: 700, fontSize: 18, color: "#0A1628" }}>Piscina Perfeita</span>
        </div>

        <Card>
          <h2 style={{ margin: "0 0 4px", fontSize: 18, color: "#0A1628" }}>
            Bem-vindo(a), {user?.nome ?? "Administrador"}!
          </h2>
          <p style={{ margin: "0 0 20px", fontSize: 13, color: "#5a6b7a" }}>
            Você ainda não tem nenhum condomínio/unidade vinculado. Como Administrador,
            você pode cadastrar quantos locais precisar administrar — vamos começar pelo primeiro.
          </p>

          {error && <ErrorMessage message={error} />}

          <form onSubmit={handleSubmit}>
            <FormGrid>
              <FormField label="Nome do local *" fullWidth>
                <input required type="text" placeholder="Ex.: Condomínio Vila das Palmeiras"
                  style={inputStyle} value={form.nome} onChange={set("nome")} />
              </FormField>
              <FormField label="Descrição" fullWidth>
                <input type="text" placeholder="Ex.: Torre A"
                  style={inputStyle} value={form.descricao} onChange={set("descricao")} />
              </FormField>
              <FormField label="Telefone">
                <input type="text" placeholder="(00) 00000-0000"
                  style={inputStyle} value={form.telefone} onChange={set("telefone")} />
              </FormField>
              <FormField label="CEP">
                <input type="text" placeholder="00000-000"
                  style={inputStyle} value={form.cep} onChange={set("cep")} />
              </FormField>
              <FormField label="Endereço" fullWidth>
                <input type="text" placeholder="Rua, número, bairro"
                  style={inputStyle} value={form.endereco} onChange={set("endereco")} />
              </FormField>
              <FormField label="Cidade">
                <input type="text" style={inputStyle} value={form.cidade} onChange={set("cidade")} />
              </FormField>
              <FormField label="Estado">
                <input type="text" placeholder="UF"
                  style={inputStyle} value={form.estado} onChange={set("estado")} />
              </FormField>
            </FormGrid>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: 20 }}>
              <button
                type="button" onClick={logout}
                style={{ background: "none", border: "none", color: "#5a6b7a", fontSize: 13, cursor: "pointer" }}
              >
                Sair
              </button>
              <Button variant="primary" type="submit" disabled={saving}>
                {saving ? "Criando…" : "Criar meu primeiro local"}
              </Button>
            </div>
          </form>
        </Card>
      </div>
    </div>
  );
}
