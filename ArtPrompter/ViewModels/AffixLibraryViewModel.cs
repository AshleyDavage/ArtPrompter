using ArtPrompter.Models;
using ArtPrompter.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ArtPrompter.ViewModels
{
    public partial class AffixLibraryViewModel : ViewModelBase
    {
        private readonly PromptDataService _promptDataService;

        public AffixLibraryViewModel(PromptDataService promptDataService)
        {
            _promptDataService = promptDataService;
            _ = LoadAsync();
        }

        public ObservableCollection<string> Prefixes { get; } = new();

        public ObservableCollection<string> Subjects { get; } = new();

        public ObservableCollection<string> Suffixes { get; } = new();

        private async Task LoadAsync()
        {
            var prefixes = await _promptDataService.LoadAsync(PromptType.Prefix);
            var subjects = await _promptDataService.LoadAsync(PromptType.Subject);
            var suffixes = await _promptDataService.LoadAsync(PromptType.Suffix);

            UpdateCollection(Prefixes, prefixes);
            UpdateCollection(Subjects, subjects);
            UpdateCollection(Suffixes, suffixes);
        }

        private static void UpdateCollection(ObservableCollection<string> target, System.Collections.Generic.IEnumerable<string> items)
        {
            target.Clear();
            foreach (var item in items)
            {
                target.Add(item);
            }
        }
    }
}
