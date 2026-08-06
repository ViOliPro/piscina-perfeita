// ============================================================
//  Piscina Perfeita — Funções de serviço para conta/senha
//
//  Este arquivo era importado por EsqueciSenha.jsx, RedefinirSenha.jsx,
//  AlterarSenhaForm.jsx e MeuPerfil.jsx mas nunca existiu de fato — por
//  isso essas telas nunca compilavam e ficaram órfãs, sem uso no app.
//  Aqui só encapsulamos chamadas que já existem em config/services.js,
//  ajustando os nomes de campo para o que a API espera (PT-BR).
// ============================================================
import { authService, usuarioService } from "../../config/services.js";

// ----------------------------------------------------------
// Esqueci minha senha / Redefinir senha
// ----------------------------------------------------------
export function solicitarRedefinicaoSenha({ email }) {
  return authService.forgotPassword({ email });
}

export function redefinirSenha({ token, novaSenha }) {
  return authService.resetPassword({ token, novaSenha });
}

// ----------------------------------------------------------
// Meu Perfil / Alterar senha
//
// ATENÇÃO: o backend ainda não tem o endpoint PUT /usuarios/me/senha
// (verificação de senha atual + troca). buscarMeuPerfil/atualizarMeuPerfil
// já têm rota real (GET/PUT /usuarios/me); alterarSenha() vai falhar com
// 404 até esse endpoint ser implementado — é o item "Meu perfil" do
// roadmap (v1.4.4), ainda não este.
// ----------------------------------------------------------
export function buscarMeuPerfil() {
  return usuarioService.meuPerfil();
}

export function atualizarMeuPerfil(form) {
  return usuarioService.atualizarMeuPerfil(form);
}

export function alterarSenha({ senhaAtual, novaSenha }) {
  return usuarioService.alterarSenha({ senhaAtual, novaSenha });
}
