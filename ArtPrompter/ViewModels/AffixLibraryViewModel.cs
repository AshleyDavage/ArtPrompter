using ArtPrompter.Models;
using ArtPrompter.Services;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ArtPrompter.ViewModels
{
    public partial class AffixLibraryViewModel : ViewModelBase
    {
        private readonly PromptDataService _promptDataService;
        private readonly ThemeInfo _theme;

        public AffixLibraryViewModel(PromptDataService promptDataService, ThemeInfo theme, ICommand closeCommand)
        {
            _promptDataService = promptDataService;
            _theme = theme;
            CloseCommand = closeCommand;
            _ = LoadAsync();
        }

        public ObservableCollection<PromptItem> Prefixes { get; } = new();

        public ObservableCollection<PromptItem> Subjects { get; } = new();

        public ObservableCollection<PromptItem> Suffixes { get; } = new();

        public ICommand CloseCommand { get; }

        public event EventHandler? ThemeDataChanged;

        [RelayCommand]
        private async Task RemovePrefixAsync(PromptItem? item)
        {
            await RemoveItemAsync(PromptType.Prefix, Prefixes, item);
        }

        [RelayCommand]
        private async Task RemoveSubjectAsync(PromptItem? item)
        {
            await RemoveItemAsync(PromptType.Subject, Subjects, item);
        }

        [RelayCommand]
        private async Task RemoveSuffixAsync(PromptItem? item)
        {
            await RemoveItemAsync(PromptType.Suffix, Suffixes, item);
        }

        private async Task LoadAsync()
        {
            var prefixes = await _promptDataService.LoadAsync(PromptType.Prefix, _theme.DirectoryPath);
            var subjects = await _promptDataService.LoadAsync(PromptType.Subject, _theme.DirectoryPath);
            var suffixes = await _promptDataService.LoadAsync(PromptType.Suffix, _theme.DirectoryPath);

            UpdateCollection(Prefixes, prefixes);
            UpdateCollection(Subjects, subjects);
            UpdateCollection(Suffixes, suffixes);
        }

        private static void UpdateCollection(ObservableCollection<PromptItem> target, System.Collections.Generic.IReadOnlyList<string> items)
        {
            target.Clear();
            for (var index = 0; index < items.Count; index++)
            {
                target.Add(new PromptItem(index, items[index]));
            }
        }

        private async Task RemoveItemAsync(PromptType type, ObservableCollection<PromptItem> target, PromptItem? item)
        {
            if (item is null)
            {
                return;
            }

            await _promptDataService.RemovePromptAtAsync(type, item.Index, _theme.DirectoryPath);
            target.Remove(item);
            Reindex(target);
            ThemeDataChanged?.Invoke(this, EventArgs.Empty);
        }

        private static void Reindex(ObservableCollection<PromptItem> target)
        {
            for (var index = 0; index < target.Count; index++)
            {
                target[index].Index = index;
            }
        }
    }
}
