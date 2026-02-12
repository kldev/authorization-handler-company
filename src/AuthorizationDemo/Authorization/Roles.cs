namespace AuthorizationDemo.Authorization;

public static class Roles
{
    public const string Root = nameof(Root);
    public const string PolandManager = nameof(PolandManager);
    public const string InternationalManager = nameof(InternationalManager);
    public const string FinancePerson = nameof(FinancePerson);

    public static readonly string[] All = [Root, PolandManager, InternationalManager, FinancePerson];
}
