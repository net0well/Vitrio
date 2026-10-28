# Padrões

Como o código do Vitrio é escrito. Quando o dotnet-claude-kit sugerir algo diferente, este documento e o [06-regras-de-desenvolvimento.md](06-regras-de-desenvolvimento.md) vencem.

## 1. C# em geral

- .NET 10, C# 14, `Nullable` habilitado, avisos como erro, namespaces por arquivo.
- Classes `sealed` por padrão; `record` para DTOs, comandos, consultas e value objects; construtores primários para injeção.
- Assíncrono de ponta a ponta, com `CancellationToken` em todo método assíncrono público.
- Tempo sempre de `TimeProvider` (injetado), nunca `DateTime.Now`/`UtcNow`.
- `IHttpClientFactory` para HTTP; `IOptions<T>` validado no startup (`ValidateDataAnnotations().ValidateOnStart()`).
- Nomes em inglês, claros e completos (`CustomerId`, não `CustId`). Termos de domínio seguem o glossário de [README.md](README.md).
- Comentários só quando o porquê não é óbvio. Ver [06-regras-de-desenvolvimento.md](06-regras-de-desenvolvimento.md).

## 2. Domínio

Entidades protegem as próprias regras. Construtor privado, fábrica estática, métodos que mudam estado e devolvem `Result`.

```csharp
public sealed class Order : AggregateRoot
{
    private readonly List<PaymentConfirmation> _paymentConfirmations = [];

    private Order() { }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProviderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Quote? Quote { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyList<PaymentConfirmation> PaymentConfirmations => _paymentConfirmations;

    public static Result<Order> Create(Guid customerId, Guid providerId, OrderRequestDetails details, TimeProvider clock)
    {
        if (customerId == Guid.Empty || providerId == Guid.Empty)
        {
            return OrderErrors.InvalidParticipants;
        }

        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerId = customerId,
            ProviderId = providerId,
            Status = OrderStatus.Sent,
            CreatedAt = clock.GetUtcNow()
        };
        order.Raise(new OrderCreated(order.Id, providerId));
        return order;
    }

    public Result SendQuote(Quote quote, TimeProvider clock)
    {
        if (Status is not (OrderStatus.Sent or OrderStatus.Quoted))
        {
            return OrderErrors.CannotQuoteInStatus(Status);
        }

        Quote = quote with { SentAt = clock.GetUtcNow() };
        Status = OrderStatus.Quoted;
        Raise(new QuoteSent(Id, CustomerId, quote.Amount));
        return Result.Success();
    }

    public Result ConfirmPayment(Guid providerId, Money amount, PaymentMethod method, DateTimeOffset receivedAt, string? differenceReason, TimeProvider clock)
    {
        if (providerId != ProviderId)
        {
            return OrderErrors.NotFound;
        }

        if (Status != OrderStatus.Completed)
        {
            return OrderErrors.CannotConfirmPaymentInStatus(Status);
        }

        if (receivedAt > clock.GetUtcNow() || receivedAt < CreatedAt)
        {
            return PaymentErrors.InvalidReceivedAt;
        }

        if (Quote is not null && amount != Quote.Amount && string.IsNullOrWhiteSpace(differenceReason))
        {
            return PaymentErrors.DifferenceReasonRequired;
        }

        _paymentConfirmations.Add(PaymentConfirmation.Create(Id, amount, method, receivedAt, differenceReason, clock));
        Status = OrderStatus.PaymentConfirmed;
        Raise(new PaymentConfirmed(Id, CustomerId, amount, method));
        return Result.Success();
    }
}
```

- Erros de domínio ficam em classes estáticas por agregado (`OrderErrors`, `PaymentErrors`) com código estável (`order.cannot_quote`) e tipo (`Validation`, `Conflict`, `NotFound`).
- Value objects validam na criação e são imutáveis (`Money.Create(decimal)` devolve `Result<Money>`).
- Eventos de domínio são `record` no passado (`QuoteSent`), sem dados pessoais além de ids.

## 3. Result

```csharp
public enum ErrorType { Validation, NotFound, Conflict, Forbidden, Unauthorized, RateLimited, Failure }

public sealed record Error(string Code, string Message, ErrorType Type);

public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static implicit operator Result(Error error) => Failure(error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, null) => _value = value;
    private Result(Error error) : base(false, error) { }

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Result has no value.");

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
}
```

Handlers devolvem `Result`/`Result<T>`; o controller traduz. Exceções ficam para o que não é regra de negócio (banco fora, bug).

## 4. Application: comandos e consultas

Um caso de uso por pasta, com `Mediator` (interfaces `ICommand<T>`, `IQuery<T>`, handlers com `ValueTask`).

```csharp
public sealed record SendQuoteCommand(
    Guid OrderId,
    QuoteType Type,
    decimal Amount,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool MaterialIncluded,
    string? Message) : ICommand<Result<QuoteResponse>>;

public sealed class SendQuoteValidator : AbstractValidator<SendQuoteCommand>
{
    public SendQuoteValidator()
    {
        RuleFor(x => x.Amount).InclusiveBetween(1m, 100_000m);
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
        RuleFor(x => x.Message).MaximumLength(500);
    }
}

public sealed class SendQuoteHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : ICommandHandler<SendQuoteCommand, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(SendQuoteCommand command, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .Where(o => o.Id == command.OrderId && o.ProviderId == currentUser.ProviderId)
            .SingleOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return OrderErrors.NotFound;
        }

        var quote = Quote.Create(command.Type, command.Amount, command.Date, command.StartTime, command.EndTime, command.MaterialIncluded, command.Message);
        if (quote.IsFailure)
        {
            return quote.Error!;
        }

        var result = order.SendQuote(quote.Value, clock);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        return QuoteResponse.From(order);
    }
}
```

- Filtro de dono **dentro da consulta** (`o.ProviderId == currentUser.ProviderId`), nunca buscar e comparar depois.
- Consultas de leitura usam `AsNoTracking()` e projetam direto para DTO com `Select`; nada de carregar agregado inteiro para listar.
- Pipeline behaviors, nesta ordem: log com duração → validação (devolve `Result` com erros por campo) → transação (só comandos).
- Mapeamento manual (`QuoteResponse.From(order)`), sem AutoMapper.

## 5. Controllers

- Um controller por recurso, rota no plural em kebab-case, sub-recursos para ações (`/quote/accept`, `/payment-confirmation`).
- Herdam `ApiControllerBase` (tradução de `Result` em ProblemDetails) e recebem só `ISender`.
- Todo action tem `[ProducesResponseType]` para cada status possível, `[EndpointSummary]` em português e `CancellationToken`.
- Request DTO separado do comando (`SendQuoteRequest.ToCommand(orderId)`), para que ids de rota e usuário nunca venham do corpo.
- Status: 200 com corpo, 201 com `Location` ao criar, 204 sem corpo, 400 validação, 401, 403 só quando o recurso é visível mas a ação não é permitida, 404 para o que não existe ou não é do usuário, 409 conflito de estado ou concorrência, 422 chave de idempotência reutilizada, 429.
- `type` dos ProblemDetails: `https://vitrio.app/problems/<codigo>` (o código do `Error`).

## 6. EF Core

- `IEntityTypeConfiguration<T>` por entidade em `Infrastructure/Persistence/Configurations`; nomes de tabela e coluna em snake_case.
- Value objects como `ComplexProperty` ou conversões; `Money` como `numeric(12,2)`.
- Sem lazy loading; `Include` explícito só em comandos que precisam do agregado completo.
- `xmin` como token de concorrência nos agregados que mudam de estado.
- Interceptor de `SaveChanges` grava eventos na outbox e campos de auditoria.
- Migrações com nome descritivo do que mudam (`AddPaymentConfirmations`), revisadas no PR; SQL gerado (`dotnet ef migrations script --idempotent`) vai para o artefato de deploy.

## 7. Logs e observabilidade

- Logs estruturados com template fixo e propriedades (`logger.LogInformation("Quote sent for order {OrderId}", orderId)`), nunca interpolação.
- Nunca logar dados pessoais, tokens, endereços ou coordenadas. Ids e `traceId` bastam.
- Uma `Activity` por caso de uso (nome do comando), com tags de resultado (`result.error_code`).
- Métricas de negócio com `Meter("Vitrio")`: pedidos criados, orçamentos enviados, confirmações, contestações.

## 8. Testes

| Tipo | Projeto | O que cobre |
| --- | --- | --- |
| Domínio | `Vitrio.Domain.Tests` | Invariantes e transições de estado, value objects |
| Aplicação | `Vitrio.Application.Tests` | Handlers com banco real em Testcontainers quando há consulta; validadores |
| Integração | `Vitrio.Api.IntegrationTests` | HTTP de ponta a ponta com `WebApplicationFactory` + PostGIS em Testcontainers: status, ProblemDetails, autorização, idempotência |
| Arquitetura | `Vitrio.ArchitectureTests` | Direção das dependências, controllers só com `ISender`, handlers `sealed`, nada de `DateTime.Now` |

- Nome do teste descreve o comportamento: `SendQuote_WhenOrderIsScheduled_ReturnsConflict`. **Nunca** número de critério de aceite ou de spec.
- Asserts nativos do xUnit; dublês com NSubstitute só para fronteiras externas (FCM, BrasilAPI, storage).
- Todo endpoint protegido tem teste de 401 sem token e de 404 com recurso de outro usuário.
- Dados de teste com builders (`OrderBuilder.Scheduled().ForProvider(id).Build()`), sem fixtures compartilhadas mutáveis.

## 9. App Flutter

- Pastas por funcionalidade (`features/<nome>/{data,domain,presentation}`), núcleo em `core/`.
- Estado com Riverpod (`AsyncNotifier`/`Notifier` gerados); a tela observa um único estado com `loading`, `data`, `empty` e `error` explícitos.
- Repositórios chamam o `ApiClient` e devolvem um resultado selado (`ApiSuccess`/`ApiFailure` com o ProblemDetails convertido); a camada de apresentação nunca vê `http.Response`.
- Só widgets e tokens do design system (`context.vitrio`, `VitrioText`, `VitrioSpace`, `VitrioRadius`); nenhuma cor, tamanho ou raio literal.
- Textos em arquivos de localização (`app_pt.arb`), mesmo com um idioma só.
- Acessibilidade: `Semantics` em controles customizados, alvos de 44 px, `tooltip` em botões de ícone, textos escaláveis até 200% sem quebrar layout.
- Rotas tipadas do go_router; redirecionamento central para sessão, modo e permissão de localização.
- Formatação brasileira centralizada (`formatMoney`, `formatDistance`, `formatRating`).

## 10. Git e revisão

- Trunk-based: `main` sempre publicável; branches curtas a partir da `main`.
- Branch: `<tipo>/<descricao-em-kebab-case>` (tipos: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `perf`). Ex.: `feat/confirmacao-de-pagamento`.
- Commit: uma frase no imperativo dizendo o que foi feito, em português. Ex.: `Adiciona confirmação de pagamento pelo prestador`.
- PR pequeno, com descrição do que mudou e como testar; `/verify` e `/code-review` do kit antes de pedir revisão.
- Regras completas (inclusive o que nunca entra em branch, commit e comentário) em [06-regras-de-desenvolvimento.md](06-regras-de-desenvolvimento.md).
