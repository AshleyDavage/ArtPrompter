using System;
using System.IO;

namespace ArtPrompter.Services
{
    public static class DataDirectoryResolver
    {
        public static string Resolve(string? overridePath = null)
        {
            if (!string.IsNullOrWhiteSpace(overridePath))
            {
                return overridePath;
            }

            var baseDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
            return baseDirectory;
        }
    }
}
