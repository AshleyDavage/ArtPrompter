using ArtPrompter.Models;
using ArtPrompter.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ArtPrompter.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly PromptDataService _promptDataService;
        private readonly ThemeService _themeService;
        private readonly Random _random = new();

        private List<string> _prefixes = new();
        private List<string> _subjects = new();
        private List<string> _suffixes = new();
        private bool _isLoaded;
        private AddPromptViewModel? _activeAddPromptViewModel;
        private AffixLibraryViewModel? _activeAffixLibraryViewModel;
        private PendingDuplicatePrompt? _pendingDuplicatePrompt;

        public MainWindowViewModel(PromptDataService promptDataService, ThemeService themeService)
        {
            _promptDataService = promptDataService;
            _themeService = themeService;
            Themes = new ObservableCollection<ThemeInfo>();
            _ = LoadThemesAsync();
        }

        public ObservableCollection<ThemeInfo> Themes { get; }

        private ThemeInfo? _selectedTheme;

        public ThemeInfo? SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    _isLoaded = false;
                    _ = LoadDataAsync();
                }
            }
        }

        [ObservableProperty]
        private string _currentPrefix = "";

        [ObservableProperty]
        private string _currentSubject = "";

        [ObservableProperty]
        private string _currentSuffix = "";

        [ObservableProperty]
        private bool _hasPrefix;

        [ObservableProperty]
        private bool _hasSuffix;

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
                CurrentPrefix = string.Empty;
                CurrentSuffix = string.Empty;
                CurrentSubject = SelectedTheme is null
                    ? "No themes available. Add a theme folder to the Data directory."
                    : "No prompts available. Add a prompt to get started.";
                UpdatePromptText();
                return;
            }

            var subject = GetRandomItem(_subjects) ?? "subject";
            var prefix = GetRandomItem(_prefixes) ?? string.Empty;
            var suffix = GetRandomItem(_suffixes) ?? string.Empty;

            CurrentPrefix = prefix;
            CurrentSubject = subject;
            CurrentSuffix = suffix;
            UpdatePromptText();
        }

        [RelayCommand]
        private void RerollPrefix()
        {
            if (_prefixes.Count == 0)
            {
                return;
            }

            CurrentPrefix = GetRandomItem(_prefixes) ?? string.Empty;
            UpdatePromptText();
        }

        [RelayCommand]
        private void RerollSubject()
        {
            if (_subjects.Count == 0)
            {
                return;
            }

            CurrentSubject = GetRandomItem(_subjects) ?? "subject";
            UpdatePromptText();
        }

        [RelayCommand]
        private void RerollSuffix()
        {
            if (_suffixes.Count == 0)
            {
                return;
            }

            CurrentSuffix = GetRandomItem(_suffixes) ?? string.Empty;
            UpdatePromptText();
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
            if (SelectedTheme is null)
            {
                return;
            }

            var viewModel = new AffixLibraryViewModel(_promptDataService, SelectedTheme, ClosePopupCommand);
            viewModel.ThemeDataChanged += OnThemeDataChanged;
            _activeAffixLibraryViewModel = viewModel;
            ShowPopup(viewModel);
        }

        [RelayCommand]
        private void ClosePopup()
        {
            CloseActivePopup();
        }

        [RelayCommand]
        private async Task ConfirmDuplicateAsync()
        {
            if (_pendingDuplicatePrompt is null)
            {
                CloseActivePopup();
                return;
            }

            await _promptDataService.AddPromptAsync(_pendingDuplicatePrompt.Type, _pendingDuplicatePrompt.Value, _pendingDuplicatePrompt.ThemeDirectory);
            AddPromptToCache(_pendingDuplicatePrompt.Type, _pendingDuplicatePrompt.Value);
            _pendingDuplicatePrompt = null;
            CloseActivePopup();
            GeneratePrompt();
        }

        [RelayCommand]
        private void CancelDuplicate()
        {
            _pendingDuplicatePrompt = null;
            CloseActivePopup();
        }

        private async Task LoadDataAsync()
        {
            if (SelectedTheme is null)
            {
                _prefixes = new List<string>();
                _subjects = new List<string>();
                _suffixes = new List<string>();
                _isLoaded = true;
                GeneratePrompt();
                return;
            }

            _prefixes = await _promptDataService.LoadAsync(PromptType.Prefix, SelectedTheme.DirectoryPath);
            _subjects = await _promptDataService.LoadAsync(PromptType.Subject, SelectedTheme.DirectoryPath);
            _suffixes = await _promptDataService.LoadAsync(PromptType.Suffix, SelectedTheme.DirectoryPath);
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

            if (entry is not null && SelectedTheme is not null)
            {
                var trimmed = entry.Text.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    if (IsDuplicate(entry.Type, trimmed))
                    {
                        _pendingDuplicatePrompt = new PendingDuplicatePrompt(entry.Type, trimmed, SelectedTheme.DirectoryPath);
                        ShowPopup(new ConfirmDuplicateViewModel(SelectedTheme, entry.Type, trimmed, ConfirmDuplicateCommand, CancelDuplicateCommand));
                        return;
                    }

                    await _promptDataService.AddPromptAsync(entry.Type, trimmed, SelectedTheme.DirectoryPath);
                    AddPromptToCache(entry.Type, trimmed);
                }
            }

            CloseActivePopup();
            GeneratePrompt();
        }

        private async Task LoadThemesAsync()
        {
            var themes = await _themeService.LoadThemesAsync();
            Themes.Clear();
            foreach (var theme in themes)
            {
                Themes.Add(theme);
            }

            SelectedTheme = Themes.FirstOrDefault();
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

            if (_activeAffixLibraryViewModel is not null)
            {
                _activeAffixLibraryViewModel.ThemeDataChanged -= OnThemeDataChanged;
                _activeAffixLibraryViewModel = null;
            }

            _pendingDuplicatePrompt = null;

            ActivePopup = null;
            IsPopupOpen = false;
        }

        private void UpdatePromptText()
        {
            HasPrefix = !string.IsNullOrWhiteSpace(CurrentPrefix);
            HasSuffix = !string.IsNullOrWhiteSpace(CurrentSuffix);
        }

        private void OnThemeDataChanged(object? sender, EventArgs e)
        {
            _isLoaded = false;
            _ = LoadDataAsync();
        }

        private void AddPromptToCache(PromptType type, string value)
        {
            switch (type)
            {
                case PromptType.Prefix:
                    _prefixes.Add(value);
                    break;
                case PromptType.Suffix:
                    _suffixes.Add(value);
                    break;
                default:
                    _subjects.Add(value);
                    break;
            }
        }

        private bool IsDuplicate(PromptType type, string value)
        {
            return type switch
            {
                PromptType.Prefix => _prefixes.Any(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase)),
                PromptType.Suffix => _suffixes.Any(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase)),
                _ => _subjects.Any(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase))
            };
        }

        private sealed record PendingDuplicatePrompt(PromptType Type, string Value, string ThemeDirectory);
    }
}
