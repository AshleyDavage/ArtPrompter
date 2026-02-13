using ArtPrompter.Models;
using ArtPrompter.Services;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ArtPrompter.ViewModels
{
    public partial class AffixLibraryViewModel : ViewModelBase
    {
        private readonly PromptDataService _promptDataService;

        public AffixLibraryViewModel(PromptDataService promptDataService, ICommand closeCommand)
        {
            _promptDataService = promptDataService;
            CloseCommand = closeCommand;
            _ = LoadAsync();
        }

        public ObservableCollection<string> Prefixes { get; } = new();

        public ObservableCollection<string> Subjects { get; } = new();

        public ObservableCollection<string> Suffixes { get; } = new();

        public ICommand CloseCommand { get; }

        [RelayCommand]
        private async Task RemovePrefixAsync(string? value)
        {
            await RemoveItemAsync(PromptType.Prefix, Prefixes, value);
        }

        [RelayCommand]
        private async Task RemoveSubjectAsync(string? value)
        {
            await RemoveItemAsync(PromptType.Subject, Subjects, value);
        }

        [RelayCommand]
        private async Task RemoveSuffixAsync(string? value)
        {
            await RemoveItemAsync(PromptType.Suffix, Suffixes, value);
        }

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

        private async Task RemoveItemAsync(PromptType type, ObservableCollection<string> target, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            await _promptDataService.RemovePromptAsync(type, value);
            target.Remove(value);
        }
    }
}
