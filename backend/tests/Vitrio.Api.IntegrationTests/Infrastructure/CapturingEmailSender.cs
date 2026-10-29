using System.Collections.Concurrent;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class CapturingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<AccountEmail> _sent = new();
    private readonly ConcurrentDictionary<string, bool> _unreachable = new(StringComparer.OrdinalIgnoreCase);

    public Task<Result> SendAsync(AccountEmail email, CancellationToken cancellationToken)
    {
        if (_unreachable.ContainsKey(email.To))
        {
            return Task.FromResult<Result>(AccountErrors.EmailUnavailable);
        }

        _sent.Enqueue(email);
        return Task.FromResult(Result.Success());
    }

    public void FailDeliveriesTo(string email) => _unreachable[email] = true;

    public IReadOnlyList<AccountEmail> SentTo(string email) =>
        _sent.Where(message => string.Equals(message.To, email, StringComparison.OrdinalIgnoreCase)).ToList();

    public string LastCodeFor(string email) => SentTo(email).OfType<VerificationCodeEmail>().Last().Code;

    public string LastResetCodeFor(string email) => SentTo(email).OfType<PasswordResetCodeEmail>().Last().Code;
}
