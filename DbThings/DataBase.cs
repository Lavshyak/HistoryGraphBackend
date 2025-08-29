using DbThings.PureEntities;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;

namespace DbThings;

public class DataBase
{
    private readonly IAsyncSession _session;

    public DataBase(IAsyncSession session)
    {
        _session = session;
    }

    public async Task<IAsyncTransaction> BeginTransactionAsync() => await _session.BeginTransactionAsync();
}