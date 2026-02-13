using ArtPrompter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArtPrompter.Services
{
    public class PromptDataService
    {
        private const string PrefixFileName = "prefix.json";
        private const string SubjectFileName = "subjects.json";
        private const string SuffixFileName = "suffix.json";

        private readonly string _dataDirectory;
        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public PromptDataService(string? dataDirectory = null)
        {
            _dataDirectory = dataDirectory ?? Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(_dataDirectory);
        }

        public async Task<List<string>> LoadAsync(PromptType type)
        {
            var path = GetFilePath(type);
            if (!File.Exists(path))
            {
                return new List<string>();
            }

            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        public async Task AddPromptAsync(PromptType type, string text)
        {
            var items = await LoadAsync(type);
            items.Add(text);
            await SaveAsync(type, items);
        }

        public async Task SaveAsync(PromptType type, List<string> items)
        {
            var path = GetFilePath(type);
            var json = JsonSerializer.Serialize(items, _serializerOptions);
            await File.WriteAllTextAsync(path, json);
        }

        private string GetFilePath(PromptType type)
        {
            var fileName = type switch
            {
                PromptType.Prefix => PrefixFileName,
                PromptType.Suffix => SuffixFileName,
                _ => SubjectFileName
            };

            return Path.Combine(_dataDirectory, fileName);
        }
    }
}
