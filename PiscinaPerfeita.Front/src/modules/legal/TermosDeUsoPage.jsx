import LegalLayout, { h2Style, pStyle, ulStyle } from "./LegalLayout.jsx";

// Conteúdo derivado de Docs/Termos de Uso.md — mantenha os dois em
// sincronia ao revisar.
export default function TermosDeUsoPage() {
  return (
    <LegalLayout title="Termos de Uso" updatedAt="[preencher na publicação]">
      <p style={pStyle}>
        Ao criar uma conta ou utilizar o <strong>Piscina Perfeita</strong>,
        você concorda com estes Termos de Uso e com a nossa{" "}
        <a href="/politica-de-privacidade">Política de Privacidade</a>.
      </p>
      <p style={pStyle}>
        O serviço é operado por Vinicius Oliveira da Silva, com sede em
        Belo Horizonte, MG, Brasil. Contato: viniciusoliveira199619@gmail.com.
      </p>

      <h2 style={h2Style}>Descrição do serviço</h2>
      <p style={pStyle}>
        O Piscina Perfeita é um sistema de gestão voltado a condomínios com
        piscina, oferecendo controle de estoque de produtos químicos e de
        limpeza, registro de análises de qualidade da água, aplicação de
        produtos e histórico de movimentações, com controle de acesso por
        perfil e isolamento total de dados entre condomínios distintos.
      </p>
      <p style={pStyle}>
        O serviço é destinado a síndicos moradores e síndicos profissionais.
      </p>

      <h2 style={h2Style}>Cadastro e conta</h2>
      <ul style={ulStyle}>
        <li>Você deve fornecer informações verdadeiras e mantê-las atualizadas.</li>
        <li>Você é responsável por manter sua senha em sigilo e por toda atividade na sua conta.</li>
        <li>Contas são criadas por convite de um Administrador do condomínio.</li>
        <li>Seu acesso é limitado ao(s) condomínio(s) ao(s) qual(is) está vinculado, conforme seu perfil de permissão.</li>
      </ul>

      <h2 style={h2Style}>Planos e cobrança</h2>
      <p style={pStyle}>
        Atualmente, o modelo de cobrança <strong>ainda não está definido</strong>{" "}
        — o serviço pode ser oferecido de forma gratuita, paga, ou em modelo
        híbrido. Caso um modelo pago seja introduzido, os usuários existentes
        serão comunicados com antecedência razoável antes de qualquer
        cobrança começar.
      </p>

      <h2 style={h2Style}>Uso aceitável</h2>
      <p style={pStyle}>Ao utilizar o sistema, você concorda em não:</p>
      <ul style={ulStyle}>
        <li>Tentar acessar dados de condomínios aos quais você não está vinculado.</li>
        <li>Utilizar o sistema para fins ilícitos ou inserir informações falsas.</li>
        <li>Tentar comprometer a segurança, disponibilidade ou integridade do sistema.</li>
        <li>Compartilhar suas credenciais de acesso com terceiros não autorizados.</li>
      </ul>

      <h2 style={h2Style}>Propriedade intelectual</h2>
      <p style={pStyle}>
        O sistema, seu código-fonte, design e marca pertencem ao operador
        do serviço. Os dados que você insere (análises, estoque,
        movimentações) continuam sendo seus/do condomínio — não
        reivindicamos propriedade sobre esse conteúdo.
      </p>

      <h2 style={h2Style}>Limitação de responsabilidade</h2>
      <p style={pStyle}>
        O Piscina Perfeita é uma ferramenta de apoio à gestão — os registros
        dependem da precisão dos dados inseridos pelos próprios usuários. O
        sistema não substitui o julgamento técnico profissional em decisões
        relacionadas à segurança da água ou à saúde dos frequentadores.
      </p>

      <h2 style={h2Style}>Cancelamento</h2>
      <p style={pStyle}>
        Você pode solicitar o cancelamento da sua conta e a exclusão dos
        seus dados pessoais a qualquer momento — veja a{" "}
        <a href="/politica-de-privacidade">Política de Privacidade</a>.
      </p>

      <h2 style={h2Style}>Lei aplicável e foro</h2>
      <p style={pStyle}>
        Estes Termos são regidos pelas leis da República Federativa do
        Brasil. Fica eleito o foro da comarca de Belo Horizonte, MG.
      </p>

      <h2 style={h2Style}>Contato</h2>
      <p style={pStyle}>viniciusoliveira199619@gmail.com</p>
    </LegalLayout>
  );
}
