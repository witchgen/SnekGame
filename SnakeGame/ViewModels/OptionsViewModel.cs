using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace SnakeGame.ViewModels
{
    public partial class OptionsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _playerName = Preferences.Default.Get("PlayerName", string.Empty);
        [ObservableProperty]
        private bool _IsNameChangeEnabled = false; // Флаг переключения режима смены имени (редактировать / подтвердить)

        [ObservableProperty]
        private bool _showPencil = true; // флаг иконки картинки "Редактировать"

        [ObservableProperty]
        private bool _showCheckmark = false; // Флаг иконки "Подтвердить"

        [ObservableProperty]
        private Color _nameChangeColor = Color.FromRgba("#e2c11d");

        public OptionsViewModel()
        {

        }

        [RelayCommand]
        private async Task GoBack() // Возвращаемся на предыдущуюю страницу в стеке Shell
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private void ChangeName() // Задаем текущее имя игрока
        {
            var buffer = NameChangeColor;

            ShowPencil = !ShowPencil;
            ShowCheckmark = !ShowCheckmark;

            NameChangeColor = ShowCheckmark ? Color.FromRgba("#6bb86b") : Color.FromRgba("#e2c11d");
            //NameChangeEditImageSrc = IsNameChangeEnabled ? "checkmark.png" : "pencil.edit.png";

            if (!string.IsNullOrWhiteSpace(PlayerName))
            {
                Preferences.Default.Set("PlayerName", PlayerName);
            }
        }
    }
}
