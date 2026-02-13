using AuthorizationDemo.Repositories;
using Npgsql;

namespace AuthorizationDemo.Extensions;

public static class PostgresExtensions
{
    public static IServiceCollection AddPostgresCompanyRepository(this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.UseLoggerFactory(loggerFactory);
            return dataSourceBuilder.Build();
        });

        services.AddSingleton<ICompanyRepository, PostgresCompanyRepository>();

        return services;
    }

    public static IServiceCollection AddFakeCompanyRepository(this IServiceCollection services)
    {
        services.AddSingleton<ICompanyRepository, FakeCompanyRepository>();
        return services;
    }

    public static async Task SeedPostgresAsync(this WebApplication app, int maxRetries = 10)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("PostgresSeed");
        var dataSource = app.Services.GetRequiredService<NpgsqlDataSource>();

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await using var conn = await dataSource.OpenConnectionAsync();
                logger.LogInformation("Connected to PostgreSQL (attempt {Attempt})", attempt);
                break;
            }
            catch (NpgsqlException) when (attempt < maxRetries)
            {
                logger.LogWarning("PostgreSQL not ready, retrying in 2s... (attempt {Attempt}/{Max})", attempt, maxRetries);
                await Task.Delay(2000);
            }
        }

        logger.LogInformation("Seeding PostgreSQL database...");

        await using var cmd = dataSource.CreateCommand(
            """
            CREATE TABLE IF NOT EXISTS companies (
                id UUID PRIMARY KEY,
                name VARCHAR(200) NOT NULL,
                country VARCHAR(10) NOT NULL,
                tax_id VARCHAR(50) NOT NULL,
                city VARCHAR(100) NOT NULL
            );

            INSERT INTO companies (id, name, country, tax_id, city) VALUES
                ('a1b2c3d4-0001-0000-0000-000000000001', 'Orlen S.A.',                'PL', '7740001454',       'Plock'),
                ('a1b2c3d4-0002-0000-0000-000000000002', 'CD Projekt S.A.',            'PL', '7342867148',       'Warszawa'),
                ('a1b2c3d4-0003-0000-0000-000000000003', 'KGHM Polska Miedz S.A.',     'PL', '6920000013',       'Lubin'),
                ('a1b2c3d4-0004-0000-0000-000000000004', 'Allegro S.A.',               'PL', '5272525995',       'Warszawa'),
                ('a1b2c3d4-0005-0000-0000-000000000005', 'Siemens AG',                 'DE', 'DE129274202',      'Munchen'),
                ('a1b2c3d4-0006-0000-0000-000000000006', 'Microsoft Corp.',            'US', '91-1144442',       'Redmond'),
                ('a1b2c3d4-0007-0000-0000-000000000007', 'Toyota Motor Corp.',         'JP', 'JP2080401050855',  'Toyota City'),
                ('a1b2c3d4-0008-0000-0000-000000000008', 'Samsung Electronics Co.',    'KR', 'KR1301110006246',  'Suwon'),
                ('a1b2c3d4-0009-0000-0000-000000000009', 'SAP SE',                     'DE', 'DE143455400',      'Walldorf'),
                ('a1b2c3d4-0010-0000-0000-000000000010', 'InPost S.A.',                'PL', '6793087624',       'Krakow')
            ON CONFLICT (id) DO NOTHING;
            """);

        await cmd.ExecuteNonQueryAsync();

        logger.LogInformation("PostgreSQL seed completed.");
    }
}
