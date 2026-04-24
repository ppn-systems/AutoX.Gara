using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Infrastructure.Database;
namespace AutoX.Gara.Infrastructure.Persistence;
public sealed class DataSessionFactory(AutoXDbContextFactory dbContextFactory) : IDataSessionFactory
{
    private readonly AutoXDbContextFactory _dbContextFactory = dbContextFactory;
    public IDataSession Create() => new DataSession(_dbContextFactory.CreateDbContext());
}
