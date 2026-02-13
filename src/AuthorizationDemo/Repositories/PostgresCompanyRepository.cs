using AuthorizationDemo.Domain;
using Npgsql;

namespace AuthorizationDemo.Repositories;

public sealed class PostgresCompanyRepository(NpgsqlDataSource dataSource) : ICompanyRepository
{
    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken ct = default)
    {
        await using var cmd = dataSource.CreateCommand("SELECT id, name, country, tax_id, city FROM companies ORDER BY name");
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var companies = new List<Company>();
        while (await reader.ReadAsync(ct))
        {
            companies.Add(MapCompany(reader));
        }

        return companies;
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var cmd = dataSource.CreateCommand("SELECT id, name, country, tax_id, city FROM companies WHERE id = $1");
        cmd.Parameters.AddWithValue(id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        return await reader.ReadAsync(ct) ? MapCompany(reader) : null;
    }

    private static Company MapCompany(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        Name = reader.GetString(1),
        Country = reader.GetString(2),
        TaxId = reader.GetString(3),
        City = reader.GetString(4)
    };
}
