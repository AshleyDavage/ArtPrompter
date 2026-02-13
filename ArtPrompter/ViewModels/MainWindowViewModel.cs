using ArtPrompter.Models;
using ArtPrompter.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtPrompter.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly PromptDataService _promptDataService;
        private readonly Random _random = new();

        private List<string> _prefixes = new();
        private List<string> _subjects = new();
        private List<string> _suffixes = new();
        private bool _isLoaded;
        private AddPromptViewModel? _activeAddPromptViewModel;

        public MainWindowViewModel(PromptDataService promptDataService)
        {
            _promptDataService = promptDataService;
            _ = LoadDataAsync();
        }

        [ObservableProperty]
        private string _promptText = "";

        [ObservableProperty]
        private ViewModelBase? _activePopup;

        [ObservableProperty]
        private bool _isPopupOpen;

        [RelayCommand]
        private void GeneratePrompt()
        {
            if (!_isLoaded)
            {
                return;
            }

            if (_subjects.Count == 0)
            {
                PromptText = "No prompts available. Add a prompt to get started.";
                return;
            }

            var subject = GetRandomItem(_subjects) ?? "subject";
            var prefix = GetRandomItem(_prefixes);
            var suffix = GetRandomItem(_suffixes);

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                parts.Add(prefix);
            }

            parts.Add(subject);

            if (!string.IsNullOrWhiteSpace(suffix))
            {
                parts.Add(suffix);
            }

            PromptText = string.Join(' ', parts);
        }

        [RelayCommand]
        private void AddNewPrompt()
        {
            var viewModel = new AddPromptViewModel();
            viewModel.CloseRequested += OnAddPromptCloseRequested;
            _activeAddPromptViewModel = viewModel;
            ShowPopup(viewModel);
        }

        [RelayCommand]
        private void OpenAffixLibrary()
        {
            var viewModel = new AffixLibraryViewModel(_promptDataService, ClosePopupCommand);
            ShowPopup(viewModel);
        }

        [RelayCommand]
        private void ClosePopup()
        {
            CloseActivePopup();
        }

        private async Task LoadDataAsync()
        {
            _prefixes = await _promptDataService.LoadAsync(PromptType.Prefix);
            _subjects = await _promptDataService.LoadAsync(PromptType.Subject);
            _suffixes = await _promptDataService.LoadAsync(PromptType.Suffix);
            _isLoaded = true;
            GeneratePrompt();
        }

        private string? GetRandomItem(IReadOnlyList<string> items)
        {
            if (items.Count == 0)
            {
                return null;
            }

            return items[_random.Next(items.Count)];
        }

        private async void OnAddPromptCloseRequested(object? sender, PromptEntry? entry)
        {
            if (sender is AddPromptViewModel viewModel)
            {
                viewModel.CloseRequested -= OnAddPromptCloseRequested;
                _activeAddPromptViewModel = null;
            }

            if (entry is not null)
            {
                var trimmed = entry.Text.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    await _promptDataService.AddPromptAsync(entry.Type, trimmed);

                    switch (entry.Type)
                    {
                        case PromptType.Prefix:
                            _prefixes.Add(trimmed);
                            break;
                        case PromptType.Suffix:
                            _suffixes.Add(trimmed);
                            break;
                        default:
                            _subjects.Add(trimmed);
                            break;
                    }
                }
            }

            CloseActivePopup();
            GeneratePrompt();
        }

        private void ShowPopup(ViewModelBase viewModel)
        {
            ActivePopup = viewModel;
            IsPopupOpen = true;
        }

        private void CloseActivePopup()
        {
            if (_activeAddPromptViewModel is not null)
            {
                _activeAddPromptViewModel.CloseRequested -= OnAddPromptCloseRequested;
                _activeAddPromptViewModel = null;
            }

            ActivePopup = null;
            IsPopupOpen = false;
        }
    }
}
