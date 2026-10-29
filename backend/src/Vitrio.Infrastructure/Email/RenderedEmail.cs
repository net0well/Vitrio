namespace Vitrio.Infrastructure.Email;

public sealed record RenderedEmail(string Subject, string Html, string Text);
