import LegalLayout, {
  h2Style,
  pStyle,
  tableStyle,
  thStyle,
  tdStyle,
} from "./LegalLayout.jsx";

// Conteúdo derivado de Docs/Politica de Cookies.md — mantenha os dois em
// sincronia ao revisar.
export default function PoliticaDeCookiesPage() {
  return (
    <LegalLayout title="Política de Cookies" updatedAt="[preencher na publicação]">
      <p style={pStyle}>
        Cookies são pequenos arquivos de texto armazenados no seu
        navegador quando você utiliza um site ou sistema web. Eles servem
        para lembrar informações sobre sua visita, como manter você
        conectado(a) entre uma página e outra.
      </p>

      <h2 style={h2Style}>Quais cookies utilizamos hoje</h2>
      <p style={pStyle}>
        Atualmente, utilizamos apenas cookies estritamente necessários,
        indispensáveis para o funcionamento básico do sistema:
      </p>
      <table style={tableStyle}>
        <thead>
          <tr>
            <th style={thStyle}>Cookie</th>
            <th style={thStyle}>Finalidade</th>
            <th style={thStyle}>Necessário?</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td style={tdStyle}>Sessão/autenticação (pp_refresh)</td>
            <td style={tdStyle}>
              Manter você conectado(a) e renovar sua sessão sem exigir
              login repetido
            </td>
            <td style={tdStyle}>Sim</td>
          </tr>
        </tbody>
      </table>
      <p style={pStyle}>
        Esse cookie é <code>HttpOnly</code> (não acessível via JavaScript) e
        transmitido apenas por conexão segura (HTTPS). Não utilizamos, no
        momento, cookies de estatística, publicidade ou rastreamento entre
        sites.
      </p>

      <h2 style={h2Style}>Uso futuro de cookies não essenciais</h2>
      <p style={pStyle}>
        Podemos, no futuro, passar a utilizar cookies de estatística ou
        marketing. Caso isso ocorra, esta política será atualizada antes
        da mudança entrar em vigor, e solicitaremos seu consentimento por
        meio de um aviso no próprio sistema, quando exigido pela
        legislação.
      </p>

      <h2 style={h2Style}>Como gerenciar cookies</h2>
      <p style={pStyle}>
        Como hoje utilizamos apenas o cookie necessário para autenticação,
        desativá-lo no navegador impede o funcionamento do login. A
        maioria dos navegadores permite visualizar e apagar cookies nas
        configurações de privacidade.
      </p>

      <h2 style={h2Style}>Contato</h2>
      <p style={pStyle}>viniciusoliveira199619@gmail.com</p>
    </LegalLayout>
  );
}
