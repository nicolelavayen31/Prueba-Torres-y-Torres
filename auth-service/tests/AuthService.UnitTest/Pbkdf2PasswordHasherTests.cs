using AuthService.Infrastructure.Security;
using Xunit;

namespace AuthService.UnitTest;

public sealed class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_CreatesIndependentHashesForSamePassword()
    {
        const string password = "SecurePassword123";

        var firstHash = _hasher.Hash(password);
        var secondHash = _hasher.Hash(password);

        Assert.NotEqual(firstHash, secondHash);
        Assert.True(_hasher.Verify(password, firstHash));
        Assert.True(_hasher.Verify(password, secondHash));
    }

    [Fact]
    public void Verify_RejectsWrongPasswordAndMalformedHash()
    {
        var passwordHash = _hasher.Hash("SecurePassword123");

        Assert.False(_hasher.Verify("IncorrectPassword123", passwordHash));
        Assert.False(_hasher.Verify("SecurePassword123", "not-a-password-hash"));
    }
}