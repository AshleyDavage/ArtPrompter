using ArtPrompter.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace ArtPrompter.ViewModels
{
    public partial class AddPromptViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _promptText = string.Empty;

        [ObservableProperty]
        private PromptType _selectedType = PromptType.Subject;

        public event EventHandler<PromptEntry?>? CloseRequested;

        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            var trimmed = PromptText.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return;
            }

            CloseRequested?.Invoke(this, new PromptEntry(SelectedType, trimmed));
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, null);
        }

        private bool CanSave() => !string.IsNullOrWhiteSpace(PromptText);

        partial void OnPromptTextChanged(string value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
}
