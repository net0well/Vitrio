using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Common.Idempotency;
using Vitrio.Application.Common.Repositories;
using Vitrio.Shared.Exceptions;

namespace Vitrio.Api.IntegrationTests.Persistence;

public sealed class IdempotencyKeyStoreTests(ApiFactory factory)
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task StoredKey_IsFoundByTheSameUserOnly()
    {
        var owner = await factory.SeedActiveUserAsync($"dona-{Guid.NewGuid():N}@exemplo.com.br");
        var other = await factory.SeedActiveUserAsync($"outra-{Guid.NewGuid():N}@exemplo.com.br");
        var key = Guid.NewGuid();

        await StoreAsync(new IdempotencyKeyEntry(owner.Id, key, "hash-1", """{"id":"1"}""", Now));

        await using var scope = factory.Services.CreateAsyncScope();
        var keys = scope.ServiceProvider.GetRequiredService<IIdempotencyKeySearchableRepository>();
        var stored = await keys.FindAsync(owner.Id, key, TestContext.Current.CancellationToken);
        Assert.Equal("hash-1", stored!.RequestHash);
        Assert.Equal("""{"id": "1"}""", stored.ResponseBody);
        Assert.Null(await keys.FindAsync(other.Id, key, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SameKeyTwice_FailsWithUniqueConstraint()
    {
        var owner = await factory.SeedActiveUserAsync($"dona-{Guid.NewGuid():N}@exemplo.com.br");
        var key = Guid.NewGuid();
        await StoreAsync(new IdempotencyKeyEntry(owner.Id, key, "hash-1", "null", Now));

        await Assert.ThrowsAsync<UniqueConstraintException>(() =>
            StoreAsync(new IdempotencyKeyEntry(owner.Id, key, "hash-1", "null", Now)));
    }

    [Fact]
    public async Task TransactionNotCommitted_LeavesNoKey()
    {
        var owner = await factory.SeedActiveUserAsync($"dona-{Guid.NewGuid():N}@exemplo.com.br");
        var key = Guid.NewGuid();

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var transactions = scope.ServiceProvider.GetRequiredService<IDatabaseTransactionFactory>();
            await using var transaction = await transactions.BeginAsync(TestContext.Current.CancellationToken);
            scope.ServiceProvider.GetRequiredService<IIdempotencyKeyWritableRepository>()
                .Add(new IdempotencyKeyEntry(owner.Id, key, "hash-1", "null", Now));
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var verification = factory.Services.CreateAsyncScope();
        var stored = await verification.ServiceProvider.GetRequiredService<IIdempotencyKeySearchableRepository>()
            .FindAsync(owner.Id, key, TestContext.Current.CancellationToken);
        Assert.Null(stored);
    }

    [Fact]
    public async Task DeleteCreatedBefore_RemovesOnlyOldKeys()
    {
        var owner = await factory.SeedActiveUserAsync($"dona-{Guid.NewGuid():N}@exemplo.com.br");
        var oldKey = Guid.NewGuid();
        var recentKey = Guid.NewGuid();
        await StoreAsync(new IdempotencyKeyEntry(owner.Id, oldKey, "hash-1", "null", Now.AddHours(-25)));
        await StoreAsync(new IdempotencyKeyEntry(owner.Id, recentKey, "hash-2", "null", Now.AddHours(-1)));

        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IIdempotencyKeyWritableRepository>()
            .DeleteCreatedBeforeAsync(Now.AddHours(-24), TestContext.Current.CancellationToken);

        var keys = scope.ServiceProvider.GetRequiredService<IIdempotencyKeySearchableRepository>();
        Assert.Null(await keys.FindAsync(owner.Id, oldKey, TestContext.Current.CancellationToken));
        Assert.NotNull(await keys.FindAsync(owner.Id, recentKey, TestContext.Current.CancellationToken));
    }

    private async Task StoreAsync(IdempotencyKeyEntry entry)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IIdempotencyKeyWritableRepository>().Add(entry);
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
