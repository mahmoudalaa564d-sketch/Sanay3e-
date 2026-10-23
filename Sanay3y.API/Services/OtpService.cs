
using System;
using System.Security.Cryptography;
using System.Text;

namespace Sanay3y.API.Services
{
    public class OtpService
    {
        public string GenerateCode()
        {
            // إنشاء رقم عشوائي آمن مكون من 6 أرقام.
            var number = RandomNumberGenerator.GetInt32(
                0, 1_000_000);

            return number.ToString("D6");
        }

        public string HashCode(string code)
        {
            var bytes = Encoding.UTF8.GetBytes(code);

            var hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash);
        }

        public bool VerifyCode(
            string providedCode,
            string storedHash)
        {
            var providedHash = HashCode(providedCode);

            var providedBytes =
                Convert.FromHexString(providedHash);

            var storedBytes =
                Convert.FromHexString(storedHash);

            return CryptographicOperations.FixedTimeEquals(
                providedBytes,
                storedBytes);
        }
    }
}
