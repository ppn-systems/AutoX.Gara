using AutoX.Gara.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;
namespace AutoX.Gara.Infrastructure.Persistence;
public sealed class DataSessionTransaction(IDbContextTransaction tx) : IDataSessionTransaction
{
    private readonly IDbContextTransaction _tx = tx;
    public System.Threading.Tasks.Task CommitAsync(System.Threading.CancellationToken ct = default)
        => _tx.CommitAsync(ct);
    public System.Threading.Tasks.Task RollbackAsync(System.Threading.CancellationToken ct = default)
        => _tx.RollbackAsync(ct);
    public async ValueTask DisposeAsync() => await _tx.DisposeAsync();
}
