using ArtPrompter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArtPrompter.Services
{
    public class PromptDataService
    {
        private const string PrefixFileName = "prefix.json";
        private const string SubjectFileName = "subjects.json";
        private const string SuffixFileName = "suffix.json";

        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public PromptDataService(string? dataDirectory = null)
        {
            var resolvedDirectory = DataDirectoryResolver.Resolve(dataDirectory);
            Directory.CreateDirectory(resolvedDirectory);
        }

        public async Task<List<string>> LoadAsync(PromptType type, string themeDirectory)
        {
            var path = GetFilePath(type, themeDirectory);
            if (!File.Exists(path))
            {
                return new List<string>();
            }

            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        public async Task AddPromptAsync(PromptType type, string text, string themeDirectory)
        {
            var items = await LoadAsync(type, themeDirectory);
            items.Add(text);
            await SaveAsync(type, items, themeDirectory);
        }

        public async Task RemovePromptAtAsync(PromptType type, int index, string themeDirectory)
        {
            var items = await LoadAsync(type, themeDirectory);
            if (index < 0 || index >= items.Count)
            {
                return;
            }

            items.RemoveAt(index);
            await SaveAsync(type, items, themeDirectory);
        }

        public async Task SaveAsync(PromptType type, List<string> items, string themeDirectory)
        {
            var path = GetFilePath(type, themeDirectory);
            var json = JsonSerializer.Serialize(items, _serializerOptions);
            await File.WriteAllTextAsync(path, json);
        }

        private string GetFilePath(PromptType type, string themeDirectory)
        {
            var fileName = type switch
            {
                PromptType.Prefix => PrefixFileName,
                PromptType.Suffix => SuffixFileName,
                _ => SubjectFileName
            };

            return Path.Combine(themeDirectory, fileName);
        }
    }
}
