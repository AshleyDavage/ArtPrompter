using ArtPrompter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArtPrompter.Services
{
    public class ThemeService
    {
        private const string ThemeMetadataFileName = "theme.json";
        private const string PrefixFileName = "prefix.json";
        private const string SubjectFileName = "subjects.json";
        private const string SuffixFileName = "suffix.json";

        private readonly string _dataDirectory;

        public ThemeService(string? dataDirectory = null)
        {
            _dataDirectory = DataDirectoryResolver.Resolve(dataDirectory);
            Directory.CreateDirectory(_dataDirectory);
        }

        public async Task<IReadOnlyList<ThemeInfo>> LoadThemesAsync()
        {
            var themes = new List<ThemeInfo>();
            var directories = Directory.GetDirectories(_dataDirectory);

            foreach (var directory in directories)
            {
                if (!HasPromptFiles(directory))
                {
                    continue;
                }

                var folderName = Path.GetFileName(directory);
                var name = await GetThemeNameAsync(directory) ?? folderName;
                var id = folderName.ToLowerInvariant();
                themes.Add(new ThemeInfo(id, name, directory));
            }

            return themes.OrderBy(theme => theme.Name).ToList();
        }

        private static bool HasPromptFiles(string directory)
        {
            return File.Exists(Path.Combine(directory, SubjectFileName))
                || File.Exists(Path.Combine(directory, PrefixFileName))
                || File.Exists(Path.Combine(directory, SuffixFileName));
        }

        private static async Task<string?> GetThemeNameAsync(string directory)
        {
            var path = Path.Combine(directory, ThemeMetadataFileName);
            if (!File.Exists(path))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(path);
            var metadata = JsonSerializer.Deserialize<ThemeMetadata>(json);
            return string.IsNullOrWhiteSpace(metadata?.Name) ? null : metadata.Name;
        }
    }
}
