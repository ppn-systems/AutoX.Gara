using System;
namespace AutoX.Gara.Application.Abstractions.Persistence;

public interface IDataSessionTransaction : System.IAsyncDisposable
{
    System.Threading.Tasks.Task CommitAsync(System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task RollbackAsync(System.Threading.CancellationToken ct = default);
}
