# Cache

Regra de ouro: **cache nunca é fonte da verdade**. Tudo que precisa sobreviver a um reinício ou valer igual para todas as instâncias da API fica no banco. O cache só acelera leituras e pode ser perdido a qualquer momento sem mudar o comportamento do sistema, só a latência.

## 1. Camadas

| Camada | Onde | Para quê |
| --- | --- | --- |
| L1 | Memória de cada instância da API (`HybridCache`) | Leituras quentes com TTL curto |
| L2 | Redis (`HybridCache` sobre `IDistributedCache`) | Compartilhado entre instâncias e entre reinícios |
| HTTP | Cabeçalho `Cache-Control` | Só dados públicos de referência (categorias e cidades) |
| App | Memória (Riverpod) e, para poucas coisas, armazenamento do aparelho | Ver §8 |

- `HybridCache` faz uma única carga por chave e instância quando várias requisições pedem o mesmo dado ao mesmo tempo (proteção contra estouro).
- O `HybridCache` não avisa as outras instâncias quando uma delas invalida uma chave: a remoção limpa o Redis e o L1 de quem invalidou. Por isso o **L1 tem TTL curto (até 30 s)** em tudo que pode mudar; essa é a desatualização máxima entre instâncias. Se um dia isso não bastar, entra um canal de invalidação pelo Redis (pub/sub).
- **Redis fora do ar:** a API continua funcionando com L1 e banco, e registra alerta. O `/health/ready` não depende do Redis, porque cache não é dependência crítica.
- **Local:** Redis no `infra/docker-compose.yml`. **Produção inicial:** container Redis no mesmo servidor, sem persistência em disco, com `maxmemory` e política `allkeys-lru`. Com mais de uma instância da API, Redis gerenciado.
- Chaves nunca contêm dado pessoal: coordenadas viram o id de uma célula da grade, termos de busca são normalizados.

## 2. O que fica em cache

| Dado | Chave | TTL no Redis | TTL local | Invalidação |
| --- | --- | --- | --- | --- |
| Perfil público (`GET /providers/{id}`) | `provider:{id}` | 5 min | 30 s | Eventos do prestador, da moderação e da conta (§3); tag `provider:{id}` |
| Fotos do perfil (`GET /providers/{id}/photos`) | `provider:{id}:photos:{page}` | 5 min | 30 s | Tag `provider:{id}` |
| Primeira página de avaliações sem filtro | `provider:{id}:reviews:1` | 5 min | 30 s | Tag `provider:{id}` |
| Início (`GET /home`) | `home:cell:{celula2km}` ou `home:city:{ibge}` | 60 s | 30 s | Só por tempo |
| Categorias (`GET /categories`) | `categories` | 1 h | 5 min | Evento `category.updated` remove a tag `categories` (§3) |
| Busca de cidades (`GET /cities?search=`) | `cities:search:{uf}:{limite}:{termo}` | 24 h | 1 h | Carga anual do IBGE remove a tag `cities` |
| Cidade por código (`GET /cities/{ibge}`) | `cities:get:{ibge}` | 24 h | 1 h | Tag `cities` |
| Localizar cidade (`GET /cities/locate`) | `cities:locate:{celula1km}`, calculada pelo ponto da célula | 10 min | 1 min | Tag `cities` |
| Sessão revogada | `session:{sid}:revoked` | 1 min | 30 s | Logout, suspensão e exclusão removem a chave (§4) |

**Não ficam em cache:** busca de prestadores (`GET /providers`, sempre do banco com índices), pedidos, pagamentos, avaliações filtradas, notificações, tudo que é privado (`/me/*`, `/provider/*`), chaves de idempotência (§5), contadores de rate limit (§6) e tudo do login: códigos de e-mail, tentativas de senha e bloqueios ficam no banco, para valerem igual em todas as instâncias e depois de reinício.

## 3. Invalidação do perfil do prestador

O perfil público junta dados de várias origens, e muitas mudanças não vêm do prestador editando a vitrine. Por isso a invalidação tem **um ponto único**: o handler de evento `InvalidateProviderProfileCache`, que remove a tag `provider:{id}`. Os eventos que o acionam estão no catálogo de [10-eventos.md](10-eventos.md) §9:

- mudança feita pelo dono (`provider.profile_updated`, `provider.published`, `provider.paused`);
- números recalculados por handler ou job (`provider.stats_updated`: nota, tempo de resposta, serviços feitos);
- selo MEI (`mei.verified`, `mei.verification_lost`);
- moderação (`content.hidden`, `content.restored`) e conta (`user.suspended`, `user.reactivated`, `user.deleted`).

Regras:

- Como o evento sai pela outbox, a invalidação acontece cerca de 1 a 2 s depois do commit, inclusive para mudanças vindas de jobs e da moderação. Nenhum handler ou job precisa lembrar de invalidar.
- `ExecuteUpdate`/`ExecuteDelete` são proibidos em agregado que gera evento ([10-eventos.md](10-eventos.md) §4), então nenhuma mudança escapa do evento.
- **Dados que dependem da hora não ficam congelados no cache.** "Disponível hoje" é calculado ao montar a resposta, a partir de `AvailableToday`, `AvailableTodaySetAt` e do fuso da cidade; o cache guarda os campos, não o selo. A distância também é calculada por requisição.
- Pausa, suspensão e remoção passam a responder 404 em até cerca de 2 s (entrega do evento), mais até 30 s do cache local nas outras instâncias.
- Categorias seguem a mesma ideia: `category.updated` aciona `InvalidateCategoriesCache`, que remove a tag `categories`.

## 4. Sessões revogadas

- **Fonte da verdade:** `sessions.revoked_at` e o `status` da conta no banco.
- Em cada requisição autenticada, depois de validar o JWT, a API consulta `session:{sid}:revoked` no `HybridCache`; na falta, lê o banco (`revoked_at is not null` ou conta não ativa).
- Com TTL de 1 min no Redis e 30 s local, um token revogado é recusado em até 1 min, mesmo depois de reinício e com várias instâncias.
- Ao revogar (logout, "sair de todos", suspensão, exclusão, reuso de refresh): grava no banco e depois remove a chave.

## 5. Idempotência

- Tabela `idempotency_keys` (`user_id`, `key`, `request_hash`, `status_code`, `response_body`, `created_at`), única por (`user_id`, `key`), gravada **na mesma transação** do comando.
- Repetição com o mesmo corpo devolve a resposta guardada; corpo diferente devolve 422.
- Job diário apaga chaves com mais de 24 h. Nunca no cache: um reinício ou uma segunda instância permitiria pedido duplicado.

## 6. Rate limit com várias instâncias

O limitador nativo do ASP.NET Core guarda os contadores na memória de cada instância. Com uma instância (produção inicial), os limites de [08-seguranca.md §5](08-seguranca.md) valem como escritos. Com N instâncias, o limite efetivo vira N vezes maior; ao escalar, os limitadores passam para o Redis.

## 7. Cabeçalhos HTTP

- **Padrão:** `Cache-Control: no-store` em toda resposta da API. Nenhum endpoint fica guardado no aparelho, em proxy ou em CDN sem decisão explícita.
- **Exceções explícitas no endpoint**, só para dados públicos de referência:
  - `GET /categories`: `public, max-age=3600`;
  - `GET /cities?search=` e `GET /cities/{ibge}`: `public, max-age=86400`.
- Perfil, busca e Início ficam `no-store`: mudam com frequência e o app controla o próprio cache.
- Resposta autenticada é sempre `no-store`, mesmo que o endpoint peça outra coisa.

## 8. App

| O quê | Onde | Por quanto tempo |
| --- | --- | --- |
| Telas e listas (Início, busca, perfil, pedidos) | Memória (Riverpod) | Enquanto o app está aberto; recarrega ao voltar para a tela, ao puxar para atualizar e depois de cada ação |
| Tokens de sessão | `flutter_secure_storage` | Até sair ou a sessão expirar |
| Boas-vindas vista, modo ativo, cidades recentes | `shared_preferences` | Até desinstalar |
| Endereços, pedidos, pagamentos, dados da conta | Nunca persistidos no aparelho | — |
| Fotos públicas | Cache de imagens em memória e cache HTTP do CDN | Conforme o CDN |
| Fotos de pedido (URL assinada de 10 min) | Só memória | Enquanto a tela está aberta |

- **Sem internet:** a tela mostra o que já tem em memória com a faixa "Sem conexão. Tentar de novo"; sem nada em memória, mostra o estado de erro. Não há modo offline no MVP.

## 9. Como testar

- Editar a vitrine, ocultar uma foto ou recalcular a nota e consultar `GET /providers/{id}` em seguida: a resposta já reflete a mudança.
- Revogar uma sessão e chamar um endpoint protegido: 401 em até 1 min, inclusive depois de reiniciar a API.
- Reenviar uma criação com a mesma `Idempotency-Key` depois de reiniciar a API: mesma resposta, sem duplicar.
- Derrubar o Redis: a API continua respondendo e o `/health/ready` segue saudável.
