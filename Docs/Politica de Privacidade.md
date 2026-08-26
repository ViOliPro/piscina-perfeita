# Política de Privacidade — Piscina Perfeita

_Última atualização: [19/08/2026]_

---

## 1. Quem somos

O **Piscina Perfeita** é um sistema de gerenciamento para condomínios, com controle de estoque de produtos químicos e de limpeza e análise da qualidade da água de piscinas.

O serviço é operado por **Vinicius Oliveira da Silva**, com sede em Belo Horizonte, MG, Brasil (doravante "nós" ou "Piscina Perfeita").

**Contato:** viniciusoliveira199619@gmail.com

Esta Política de Privacidade explica quais dados pessoais coletamos, por que coletamos, como os protegemos e quais direitos você tem sobre eles, em conformidade com a Lei Geral de Proteção de Dados (Lei nº 13.709/2018 — LGPD).

## 2. Quem pode utilizar o sistema

O Piscina Perfeita é destinado a síndicos moradores e síndicos profissionais responsáveis pela administração de condomínios com piscina. Cada cliente (condomínio) é tratado como um "Local" dentro do sistema, com seus dados completamente isolados dos demais.

## 3. Quais dados pessoais coletamos

### 3.1 Dados fornecidos diretamente por você

| Dado                                                     | Finalidade                                                       |
| -------------------------------------------------------- | ---------------------------------------------------------------- |
| Nome                                                     | Identificação do usuário dentro do sistema                       |
| E-mail                                                   | Login, comunicação e recuperação de conta                        |
| Senha                                                    | Autenticação (armazenada sempre como hash — nunca em texto puro) |
| Perfil de acesso (Administrador, Operador, Visualizador) | Controle de permissões dentro do Local                           |
| Vínculo com o Local/condomínio                           | Isolamento de dados entre clientes (multi-tenant)                |

### 3.2 Dados gerados pelo uso do sistema

| Dado                                                                        | Finalidade                                       |
| --------------------------------------------------------------------------- | ------------------------------------------------ |
| Registros de análises de água (pH, cloro, alcalinidade, temperatura)        | Histórico técnico da(s) piscina(s) do condomínio |
| Movimentações de estoque (entradas, saídas, aplicações, descartes, ajustes) | Controle de inventário e rastreabilidade         |

**Não coletamos, hoje, registros de log de acesso** (IP, horário de login, dispositivo). Essa funcionalidade está prevista para uma versão futura, como parte do reforço de segurança e auditoria do sistema. Caso seja implementada, esta política será atualizada com antecedência para refletir a nova coleta antes de entrar em vigor.

### 3.3 O que não coletamos

Não utilizamos cookies ou ferramentas de rastreamento para estatística ou marketing no momento (ex.: Google Analytics, Meta Pixel, Hotjar). Veja a seção 8 e a [Política de Cookies](./Politica%20de%20Cookies.md) para mais detalhes, incluindo a possibilidade de uso futuro.

## 4. Base legal para o tratamento

Tratamos seus dados pessoais com base nas seguintes hipóteses legais da LGPD:

- **Execução de contrato** (art. 7º, V): dados necessários para fornecer o serviço que você contratou/utiliza.
- **Legítimo interesse** (art. 7º, IX): melhorias de segurança, prevenção a fraudes e manutenção da integridade do sistema.

## 5. Compartilhamento de dados

**Não compartilhamos seus dados pessoais com terceiros** (como contadores, administradoras externas ou empresas parceiras). Os dados de cada condomínio são acessíveis apenas pelos usuários vinculados àquele Local dentro do próprio sistema.

Podemos utilizar prestadores de serviço estritamente técnicos, necessários para operar o sistema (infraestrutura e envio de e-mail — ver seção 6). Esses prestadores têm acesso aos dados apenas na medida necessária para prestar o serviço técnico contratado, não para fins próprios.

## 6. Onde e como seus dados são armazenados

- **Hospedagem da aplicação (API e frontend):** Render.
- **Banco de dados:** PostgreSQL, hospedado na Neon.
- **Envio de e-mails transacionais** (ex.: redefinição de senha, convites): Resend.
- **Conexão:** todo o tráfego entre seu navegador e o sistema é criptografado via HTTPS.
- **Senhas:** armazenadas exclusivamente como hash (BCrypt) — nunca em texto legível, nem por nós.
- **Autenticação:** feita via token JWT, com escopo restrito ao Local ativo do usuário.

## 7. Por quanto tempo guardamos seus dados

Mantemos seus dados enquanto sua conta estiver ativa e for necessário para prestar o serviço. Caso solicite a exclusão da conta (seção 8), seus dados pessoais são removidos, exceto quando a lei exigir retenção por período determinado.

## 8. Seus direitos como titular dos dados

Você pode, mediante solicitação ao e-mail de contato:

- **Solicitar a exclusão da sua conta** e dos dados pessoais associados.
- **Solicitar a correção/alteração de dados** incorretos ou desatualizados.

Esses dois direitos já contam com fluxo de atendimento no lançamento atual. Os demais direitos previstos na LGPD (art. 18) — como confirmação de tratamento, acesso aos dados, portabilidade e informação sobre compartilhamento — também podem ser exercidos mediante solicitação por e-mail, e serão atendidos manualmente até que existam telas de autoatendimento para isso.

## 9. Cookies

Hoje utilizamos apenas cookies estritamente necessários para o funcionamento do login (sessão de autenticação). Não usamos cookies de estatística ou marketing no momento, mas esta política pode ser atualizada caso isso mude no futuro. Detalhes completos na [Política de Cookies](./Politica%20de%20Cookies.md).

## 10. Segurança da informação

Adotamos medidas técnicas para proteger seus dados, incluindo:

- Isolamento de dados entre condomínios (Locais), garantido no backend — um cliente nunca acessa dados de outro.
- Autenticação via JWT e senhas com hash (BCrypt).
- Tráfego criptografado via HTTPS.
- Controle de acesso por perfil (Administrador, Operador, Visualizador).

Nenhum sistema é 100% imune a incidentes de segurança. Em caso de incidente que afete dados pessoais, comunicaremos os titulares afetados e a Autoridade Nacional de Proteção de Dados (ANPD), conforme exigido pela LGPD.

## 11. Alterações nesta política

Podemos atualizar esta Política de Privacidade periodicamente, especialmente à medida que o sistema evolui (ex.: novos módulos, mudança no modelo de cobrança, novos dados coletados). Alterações relevantes serão comunicadas com antecedência razoável. A data da última atualização está sempre indicada no topo deste documento.

## 12. Contato

Dúvidas, solicitações sobre seus dados ou qualquer assunto relacionado a esta política podem ser enviados para:

📧 **viniciusoliveira199619@gmail.com**
