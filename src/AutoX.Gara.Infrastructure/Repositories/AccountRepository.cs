using AutoX.Gara.Application.Abstractions.Repositories;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
namespace AutoX.Gara.Infrastructure.Repositories;
public sealed class AccountRepository(AutoXDbContext context) : IAccountRepository
{
    private readonly AutoXDbContext _context = context;
    public System.Threading.Tasks.Task<Account> GetByUsernameAsync(string username, System.Threading.CancellationToken ct = default)
        => _context.Set<Account>().FirstOrDefaultAsync(a => a.DeletedAt == null && a.Username == username, ct);
    public System.Threading.Tasks.Task<bool> ExistsByUsernameAsync(string username, System.Threading.CancellationToken ct = default)
        => _context.Set<Account>().AnyAsync(a => a.DeletedAt == null && a.Username == username, ct);
    public System.Threading.Tasks.Task AddAsync(Account account, System.Threading.CancellationToken ct = default)
        => _context.Set<Account>().AddAsync(account, ct).AsTask();
    public System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
