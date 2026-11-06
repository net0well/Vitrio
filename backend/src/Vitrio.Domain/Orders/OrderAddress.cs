namespace Vitrio.Domain.Orders;

public sealed record OrderAddress(string Street, string Number, string? Complement, string Cep);
