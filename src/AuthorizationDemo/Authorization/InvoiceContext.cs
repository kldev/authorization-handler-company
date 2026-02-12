namespace AuthorizationDemo.Authorization;

/// <summary>
/// Resource passed to authorization handlers when creating an invoice.
/// </summary>
public sealed record InvoiceContext(decimal Amount);
