namespace AuthorizationDemo.Domain;

public sealed class Company
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Country { get; init; }
    public required string TaxId { get; init; }
    public required string City { get; init; }
}
