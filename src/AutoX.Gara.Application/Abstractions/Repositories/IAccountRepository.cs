using AutoX.Gara.Domain.Entities.Identity;
namespace AutoX.Gara.Application.Repositories;
public interface IAccountRepository
{
    System.Threading.Tasks.Task<Account> GetByUsernameAsync(string username, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<bool> ExistsByUsernameAsync(string username, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(Account account, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}

