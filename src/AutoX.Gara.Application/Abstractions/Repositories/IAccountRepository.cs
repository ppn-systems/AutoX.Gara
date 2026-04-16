using AutoX.Gara.Domain.Entities.Identity;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IAccountRepository
{
    System.Threading.Tasks.Task<Account> GetByUsernameAsync(System.String username, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<System.Boolean> ExistsByUsernameAsync(System.String username, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(Account account, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}
