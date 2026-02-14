using ArtPrompter.Models;
using System.Windows.Input;

namespace ArtPrompter.ViewModels
{
    public class ConfirmDuplicateViewModel : ViewModelBase
    {
        public ConfirmDuplicateViewModel(ThemeInfo theme, PromptType type, string value, ICommand confirmCommand, ICommand cancelCommand)
        {
            ThemeName = theme.Name;
            AttributeType = type.ToString();
            Value = value;
            ConfirmCommand = confirmCommand;
            CancelCommand = cancelCommand;
        }

        public string ThemeName { get; }

        public string AttributeType { get; }

        public string Value { get; }

        public ICommand ConfirmCommand { get; }

        public ICommand CancelCommand { get; }
    }
}
