
using Microsoft.AspNetCore.Identity;

namespace Sanay3y.API.Services
{
    public class PasswordService
    {
        private readonly PasswordHasher<object> _passwordHasher = new();

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null!, password);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                null!,
                hashedPassword,
                providedPassword);

            return result != PasswordVerificationResult.Failed;
        }
    }
}
