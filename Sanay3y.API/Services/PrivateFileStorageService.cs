
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Sanay3y.API.Services
{
    public sealed class PrivateFileStorageService
    {
        public const long MaxFileSizeBytes = 5 * 1024 * 1024;

        private readonly string _rootPath;

        public PrivateFileStorageService(
            IConfiguration configuration)
        {
            var configuredPath =
                configuration["PrivateFileStorage:RootPath"];

            var defaultPath = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Sanay3y",
                "private-uploads");

            if (string.IsNullOrWhiteSpace(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData)))
            {
                defaultPath = Path.Combine(
                    Path.GetTempPath(),
                    "Sanay3y",
                    "private-uploads");
            }

            _rootPath = Path.GetFullPath(
                string.IsNullOrWhiteSpace(configuredPath)
                    ? defaultPath
                    : configuredPath);
        }

        public async Task<string> SaveImageAsync(
            IFormFile file,
            string category,
            CancellationToken cancellationToken = default)
        {
            if (file is null || file.Length <= 0)
            {
                throw new InvalidDataException(
                    "The uploaded image is empty.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                throw new InvalidDataException(
                    "Each image must be 5 MB or smaller.");
            }

            if (category != "profiles" &&
                category != "identity")
            {
                throw new ArgumentException(
                    "Unsupported file category.",
                    nameof(category));
            }

            var extension = await DetectImageExtensionAsync(
                file,
                cancellationToken);

            var directory = Path.Combine(_rootPath, category);
            Directory.CreateDirectory(directory);

            var fileName = Guid.NewGuid().ToString("N") + extension;
            var fullPath = Path.Combine(directory, fileName);
            var storageKey = $"{category}/{fileName}";

            try
            {
                await using var destination = new FileStream(
                    fullPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    FileOptions.Asynchronous);

                await file.CopyToAsync(
                    destination,
                    cancellationToken);

                return storageKey;
            }
            catch
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                throw;
            }
        }

        private static async Task<string> DetectImageExtensionAsync(
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var header = new byte[8];
            var totalRead = 0;

            await using var input = file.OpenReadStream();

            while (totalRead < header.Length)
            {
                var bytesRead = await input.ReadAsync(
                    header.AsMemory(
                        totalRead,
                        header.Length - totalRead),
                    cancellationToken);

                if (bytesRead == 0)
                {
                    break;
                }

                totalRead += bytesRead;
            }

            // JPEG signature.
            if (totalRead >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF)
            {
                return ".jpg";
            }

            // PNG signature.
            if (totalRead >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47 &&
                header[4] == 0x0D &&
                header[5] == 0x0A &&
                header[6] == 0x1A &&
                header[7] == 0x0A)
            {
                return ".png";
            }

            throw new InvalidDataException(
                "Only valid JPEG and PNG image signatures are accepted.");
        }

        public Task DeleteAsync(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                return Task.CompletedTask;
            }

            var relativePath = storageKey.Replace(
                '/',
                Path.DirectorySeparatorChar);

            var fullPath = Path.GetFullPath(
                Path.Combine(_rootPath, relativePath));

            var rootPrefix = _rootPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (!fullPath.StartsWith(rootPrefix, comparison))
            {
                throw new InvalidOperationException(
                    "Invalid private storage key.");
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }
    }
}
