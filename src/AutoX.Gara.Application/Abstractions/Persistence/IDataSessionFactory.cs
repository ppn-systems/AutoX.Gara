namespace AutoX.Gara.Application.Abstractions.Persistence;

public interface IDataSessionFactory
{
    IDataSession Create();
}
