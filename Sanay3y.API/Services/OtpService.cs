
using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Sanay3y.API.Services
{
    public class OtpService
    {
        private readonly byte[] _hashKey;

        public OtpService(IConfiguration configuration)
        {
            var configuredKey = configuration["Otp:HashKey"];

            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                throw new InvalidOperationException(
                    "OTP hashing key is missing. Configure Otp:HashKey.");
            }

            _hashKey = Encoding.UTF8.GetBytes(configuredKey);

            if (_hashKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "OTP hashing key must be at least 32 bytes long.");
            }
        }

        public string GenerateCode()
        {
            // Generate a cryptographically secure six-digit code.
            var number = RandomNumberGenerator.GetInt32(
                0, 1_000_000);

            return number.ToString("D6");
        }

        public string HashCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(
                    "OTP code cannot be empty.",
                    nameof(code));
            }

            var codeBytes = Encoding.UTF8.GetBytes(code);

            // Use a server-side secret to protect the low-entropy OTP.
            var hash = HMACSHA256.HashData(
                _hashKey,
                codeBytes);

            return Convert.ToHexString(hash);
        }

        public bool VerifyCode(
            string providedCode,
            string storedHash)
        {
            // OTP codes must contain exactly six ASCII digits.
            if (string.IsNullOrEmpty(providedCode) ||
                providedCode.Length != 6)
            {
                return false;
            }

            foreach (var character in providedCode)
            {
                if (character < '0' || character > '9')
                {
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            byte[] storedBytes;

            try
            {
                storedBytes = Convert.FromHexString(storedHash);
            }
            catch (FormatException)
            {
                return false;
            }

            // HMAC-SHA256 produces a 32-byte hash.
            if (storedBytes.Length != 32)
            {
                return false;
            }

            var providedBytes = HMACSHA256.HashData(
                _hashKey,
                Encoding.UTF8.GetBytes(providedCode));

            return CryptographicOperations.FixedTimeEquals(
                providedBytes,
                storedBytes);
        }
    }
}
