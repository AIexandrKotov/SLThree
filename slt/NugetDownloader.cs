using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging;

namespace slt
{
    internal class NugetDownloader
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> DownloadAndExtractLatestPackage(string packageId, string targetFramework, string outputPath)
        {
            // 1. Получаем метаданные последней версии пакета.
            var packageInfo = await GetLatestPackageInfo(packageId);

            if (packageInfo == null)
            {
                Console.WriteLine($"Не удалось получить информацию о пакете {packageId}.");
                return null;
            }

            // 2. Скачиваем NuGet пакет
            var packagePath = await DownloadPackage(packageId, packageInfo.Value.Version, outputPath);

            if (string.IsNullOrEmpty(packagePath))
            {
                Console.WriteLine($"Ошибка при скачивании пакета {packageId}.{packageInfo.Value.Version}");
                return null;
            }

            // 3. Извлекаем DLL для целевого фреймворка
            var dllPath = await ExtractDllForTargetFramework(packagePath, targetFramework, outputPath);

            if (string.IsNullOrEmpty(dllPath))
            {
                Console.WriteLine($"Не найдена DLL для {targetFramework} в пакете {packageId}.{packageInfo.Value.Version}");
                return null;
            }

            return dllPath;
        }

        private static async Task<(string Version, string DownloadUrl)?> GetLatestPackageInfo(string packageId)
        {
            var url = $"https://api.nuget.org/v3/registration5-semver1/{packageId.ToLower()}/index.json";

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                using (var document = JsonDocument.Parse(json))
                {
                    var root = document.RootElement;
                    var items = root.GetProperty("items");
                    var lastItem = items[items.GetArrayLength() - 1];
                    var upper = lastItem.GetProperty("upper");

                    // Берем последний элемент (предположительно, это последняя версия)
                    if (items.GetArrayLength() == 0) return null;

                    var packageDetails = lastItem.GetProperty("items")[0];

                    var version = upper.GetString();
                    var downloadUrl = packageDetails.GetProperty("packageContent").GetString();

                    return (version, downloadUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении метаданных пакета: {ex.Message}");
                return null;
            }
        }


        private static async Task<string> DownloadPackage(string packageId, string version, string outputPath)
        {
            try
            {
                Directory.CreateDirectory(outputPath);

                var packageUrl = $"https://www.nuget.org/api/v2/package/{packageId}/{version}";  // Альтернативный URL (может быть более стабильным)

                var packageFilePath = Path.Combine(outputPath, $"{packageId}.{version}.nupkg");

                using (var response = await client.GetAsync(packageUrl))
                {
                    response.EnsureSuccessStatusCode();
                    using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    {
                        using (var fileStream = new FileStream(packageFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await streamToReadFrom.CopyToAsync(fileStream);
                        }
                    }
                }

                return packageFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при скачивании пакета: {ex.Message}");
                return null;
            }
        }

        private static async Task<string> ExtractDllForTargetFramework(string packagePath, string targetFramework, string outputPath)
        {
            try
            {
                using (var packageArchiveReader = new PackageArchiveReader(packagePath))
                {
                    var frameworkSpecificGroup = packageArchiveReader.GetLibItems().FirstOrDefault(group => group.TargetFramework.Framework == NuGet.Frameworks.NuGetFramework.ParseFolder(targetFramework).Framework);

                    if (frameworkSpecificGroup == null)
                    {
                        Console.WriteLine($"Предупреждение: Не найдена точная соответствие фреймворку.  Попытка найти ближайший (это может быть небезопасно!).");
                        frameworkSpecificGroup = packageArchiveReader.GetLibItems().FirstOrDefault(); // Берем первый попавшийся.
                        if (frameworkSpecificGroup == null) return null; // Нет подходящего фреймворка
                    }


                    var dllFile = frameworkSpecificGroup.Items.FirstOrDefault(file => file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));

                    if (string.IsNullOrEmpty(dllFile))
                    {
                        return null; // DLL не найдена
                    }

                    var extractedDllPath = Path.Combine(outputPath, Path.GetFileName(dllFile));


                    using (var stream = packageArchiveReader.GetStream(dllFile))
                    {
                        if (stream == null)
                        {
                            Console.WriteLine($"Не удалось получить поток для файла: {dllFile}");
                            return null;
                        }

                        using (var fileStream = new FileStream(extractedDllPath, FileMode.Create, FileAccess.Write))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }

                    return extractedDllPath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при извлечении DLL: {ex.Message}");
                return null;
            }
        }
    }
}
