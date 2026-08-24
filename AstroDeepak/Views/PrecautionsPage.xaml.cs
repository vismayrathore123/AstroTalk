using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Views
{
    public partial class PrecautionsPage : ContentPage
    {
        private readonly IPrecautionRepository _precautionRepository;

        public PrecautionsPage(IPrecautionRepository precautionRepository)
        {
            InitializeComponent();
            _precautionRepository = precautionRepository;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAsync();
        }

        async Task LoadAsync()
            => PrecautionList.ItemsSource = await _precautionRepository.GetAllAsync();

        async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");

        async void OnAddPrecautionClicked(object sender, EventArgs e)
        {
            var text = NewPrecautionEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            await _precautionRepository.AddPrecautionAsync(text);
            NewPrecautionEntry.Text = string.Empty;
            await LoadAsync();
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.CommandParameter is not PrecautionMaster precaution) return;

            var newText = await DisplayPromptAsync("Edit Precaution", "Update precaution text", "Save", "Cancel", initialValue: precaution.Text);
            if (string.IsNullOrWhiteSpace(newText)) return;

            await _precautionRepository.UpdatePrecautionAsync(precaution.Id, newText.Trim());
            await LoadAsync();
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.CommandParameter is not int id) return;

            var confirm = await DisplayAlert("Delete", "Delete this precaution?", "Delete", "Cancel");
            if (!confirm) return;

            await _precautionRepository.DeletePrecautionAsync(id);
            await LoadAsync();
        }
    }
}