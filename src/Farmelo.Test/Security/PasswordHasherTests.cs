using Farmelo.Business.Services.Security;

namespace Farmelo.Test.Security;

[TestFixture]
public sealed class PasswordHasherTests
{
    [Test]
    public void HashPassword_CreatesVerifiableHash()
    {
        var hasher = new Pbkdf2PasswordHasher();

        var hash = hasher.HashPassword("Farmelo@123");

        Assert.That(hash, Does.StartWith("PBKDF2-SHA256$"));
        Assert.That(hasher.VerifyPassword("Farmelo@123", hash), Is.True);
    }

    [Test]
    public void VerifyPassword_RejectsWrongPassword()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.HashPassword("Farmelo@123");

        Assert.That(hasher.VerifyPassword("wrong-password", hash), Is.False);
    }
}
