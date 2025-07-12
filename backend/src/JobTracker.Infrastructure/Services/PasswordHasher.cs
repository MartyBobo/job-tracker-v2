using System.Security.Cryptography;
using JobTracker.Application.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace JobTracker.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 128 / 8; // 128 bits
    private const int KeySize = 256 / 8; // 256 bits
    private const int Iterations = 10000;
    private const char Delimiter = ';';

    public string HashPassword(string password)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);
        
        return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var elements = passwordHash.Split(Delimiter);
        
        if (elements.Length != 2)
            return false;

        var salt = Convert.FromBase64String(elements[0]);
        var hash = Convert.FromBase64String(elements[1]);

        var computedHash = HashPassword(password, salt);
        
        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }

    private static byte[] GenerateSalt()
    {
        var salt = new byte[SaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        return KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize);
    }
}