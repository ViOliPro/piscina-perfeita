import LegalLayout, {
  h2Style,
  pStyle,
  ulStyle,
  tableStyle,
  thStyle,
  tdStyle,
} from "./LegalLayout.jsx";

// Conteúdo derivado de Docs/Politica de Privacidade.md — mantenha os dois
// em sincronia ao revisar. Ao publicar uma alteração relevante, atualize
// LegalConstants.VersaoTermosAtual (backend) e a data abaixo.
export default function PoliticaDePrivacidadePage() {
  return (
    <LegalLayout title="Política de Privacidade" updatedAt="[preencher na publicação]">
      <p style={pStyle}>
        O <strong>Piscina Perfeita</strong> é um sistema de gerenciamento
        para condomínios, com controle de estoque de produtos químicos e de
        limpeza e análise da qualidade da água de piscinas.
      </p>
      <p style={pStyle}>
        O serviço é operado por <strong>Vinicius Oliveira da Silva</strong>,
        com sede em Belo Horizonte, MG, Brasil. Esta Política explica quais
        dados pessoais coletamos, por que coletamos, como os protegemos e
        quais direitos você tem sobre eles, em conformidade com a Lei Geral
        de Proteção de Dados (Lei nº 13.709/2018 — LGPD).
      </p>

      <h2 style={h2Style}>Quais dados pessoais coletamos</h2>
      <table style={tableStyle}>
        <thead>
          <tr>
            <th style={thStyle}>Dado</th>
            <th style={thStyle}>Finalidade</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td style={tdStyle}>Nome</td>
            <td style={tdStyle}>Identificação do usuário dentro do sistema</td>
          </tr>
          <tr>
            <td style={tdStyle}>E-mail</td>
            <td style={tdStyle}>Login, comunicação e recuperação de conta</td>
          </tr>
          <tr>
            <td style={tdStyle}>Senha</td>
            <td style={tdStyle}>Autenticação (armazenada sempre como hash)</td>
          </tr>
          <tr>
            <td style={tdStyle}>Perfil de acesso</td>
            <td style={tdStyle}>Controle de permissões dentro do condomínio</td>
          </tr>
          <tr>
            <td style={tdStyle}>Vínculo com o condomínio</td>
            <td style={tdStyle}>Isolamento de dados entre clientes</td>
          </tr>
          <tr>
            <td style={tdStyle}>Análises de água e movimentações de estoque</td>
            <td style={tdStyle}>Histórico técnico do condomínio</td>
          </tr>
        </tbody>
      </table>
      <p style={pStyle}>
        <strong>Não coletamos, hoje, registros de log de acesso</strong> (IP,
        horário de login, dispositivo). Essa funcionalidade está prevista
        para uma versão futura, como parte do reforço de segurança e
        auditoria do sistema. Caso seja implementada, esta política será
        atualizada com antecedência, antes da nova coleta entrar em vigor.
      </p>
      <p style={pStyle}>
        Não utilizamos cookies ou ferramentas de rastreamento para
        estatística ou marketing no momento. Veja a{" "}
        <a href="/politica-de-cookies">Política de Cookies</a> para mais
        detalhes, incluindo a possibilidade de uso futuro.
      </p>

      <h2 style={h2Style}>Compartilhamento de dados</h2>
      <p style={pStyle}>
        <strong>Não compartilhamos seus dados pessoais com terceiros</strong>{" "}
        (como contadores, administradoras externas ou empresas parceiras).
        Os dados de cada condomínio são acessíveis apenas pelos usuários
        vinculados àquele condomínio dentro do próprio sistema.
      </p>

      <h2 style={h2Style}>Onde e como seus dados são armazenados</h2>
      <ul style={ulStyle}>
        <li>Hospedagem da aplicação: Render.</li>
        <li>Banco de dados: PostgreSQL, hospedado na Neon.</li>
        <li>Envio de e-mails transacionais: Resend.</li>
        <li>Todo o tráfego é criptografado via HTTPS.</li>
        <li>Senhas armazenadas exclusivamente como hash (BCrypt).</li>
        <li>Autenticação via token JWT, restrita ao condomínio ativo do usuário.</li>
      </ul>

      <h2 style={h2Style}>Seus direitos como titular dos dados</h2>
      <p style={pStyle}>Você pode, mediante solicitação ao e-mail de contato:</p>
      <ul style={ulStyle}>
        <li>Solicitar a exclusão da sua conta e dos dados pessoais associados.</li>
        <li>Solicitar a correção/alteração de dados incorretos ou desatualizados.</li>
      </ul>
      <p style={pStyle}>
        Os demais direitos previstos na LGPD (art. 18) também podem ser
        exercidos mediante solicitação por e-mail, e serão atendidos
        manualmente até que existam telas de autoatendimento para isso.
      </p>

      <h2 style={h2Style}>Segurança da informação</h2>
      <p style={pStyle}>
        Isolamos completamente os dados entre condomínios no backend,
        usamos autenticação via JWT com senhas em hash, tráfego
        criptografado via HTTPS e controle de acesso por perfil. Nenhum
        sistema é 100% imune a incidentes — em caso de incidente que afete
        dados pessoais, comunicaremos os titulares afetados e a Autoridade
        Nacional de Proteção de Dados (ANPD), conforme exigido pela LGPD.
      </p>

      <h2 style={h2Style}>Alterações nesta política</h2>
      <p style={pStyle}>
        Podemos atualizar esta Política periodicamente, à medida que o
        sistema evolui. Alterações relevantes serão comunicadas com
        antecedência razoável.
      </p>

      <h2 style={h2Style}>Contato</h2>
      <p style={pStyle}>viniciusoliveira199619@gmail.com</p>
    </LegalLayout>
  );
}
