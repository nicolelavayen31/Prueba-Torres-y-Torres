using System.Net.Mail;
using AuthService.Domain.Exceptions;

namespace AuthService.Domain.ValueObjects;

public sealed record EmailAddress
{
    private EmailAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EmailAddress Create(string? value)
    {
        if (!TryCreate(value, out var emailAddress))
        {
            throw new InvalidEmailAddressException();
        }

        return emailAddress!;
    }

    public static bool TryCreate(string? value, out EmailAddress? emailAddress)
    {
        emailAddress = null;
        var candidate = value?.Trim();
        if (string.IsNullOrWhiteSpace(candidate) || candidate.Length > 254)
        {
            return false;
        }

        try
        {
            var parsed = new MailAddress(candidate);
            if (!string.Equals(parsed.Address, candidate, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            emailAddress = new EmailAddress(parsed.Address.ToLowerInvariant());
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public override string ToString() => Value;
}