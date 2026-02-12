using AuthorizationDemo.Domain;

namespace AuthorizationDemo.Repositories;

public sealed class FakeCompanyRepository : ICompanyRepository
{
    private static readonly List<Company> Companies =
    [
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001"),
            Name = "Orlen S.A.",
            Country = "PL",
            TaxId = "7740001454",
            City = "Plock"
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0002-0000-0000-000000000002"),
            Name = "CD Projekt S.A.",
            Country = "PL",
            TaxId = "7342867148",
            City = "Warszawa"
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0003-0000-0000-000000000003"),
            Name = "KGHM Polska Miedz S.A.",
            Country = "PL",
            TaxId = "6920000013",
            City = "Lubin"
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0004-0000-0000-000000000004"),
            Name = "Allegro S.A.",
            Country = "PL",
            TaxId = "5272525995",
            City = "Warszawa"
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0005-0000-0000-000000000005"),
            Name = "Siemens AG",
            Country = "DE",
            TaxId = "DE129274202",
            City = "Munchen"
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0006-0000-0000-000000000006"),
            Name = "Microsoft Corp.",
            Country = "US",
            TaxId = "91-1144442",
            City = "Redmond"
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0007-0000-0000-000000000007"),
            Name = "Toyota Motor Corp.",
            Country = "JP",
            TaxId = "JP2080401050855",
            City = "Toyota City"
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0008-0000-0000-000000000008"),
            Name = "Samsung Electronics Co.",
            Country = "KR",
            TaxId = "KR1301110006246",
            City = "Suwon"
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0009-0000-0000-000000000009"),
            Name = "SAP SE",
            Country = "DE",
            TaxId = "DE143455400",
            City = "Walldorf"
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0010-0000-0000-000000000010"),
            Name = "InPost S.A.",
            Country = "PL",
            TaxId = "6793087624",
            City = "Krakow"
        }
    ];

    public Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Company>>(Companies);

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Companies.FirstOrDefault(c => c.Id == id));
}
