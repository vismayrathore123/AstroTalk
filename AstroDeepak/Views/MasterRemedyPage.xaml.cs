using AstroDeepak.Domain.Abstractions;

namespace AstroDeepak.Views
{
    public partial class MasterRemedyPage : ContentPage
    {
        private readonly IRemedyRepository _remedyRepository;

        public MasterRemedyPage(IRemedyRepository remedyRepository)
        {
            InitializeComponent();
            _remedyRepository = remedyRepository;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAsync();
        }

        async Task LoadAsync()
        {
            var all = await _remedyRepository.GetAllRemediesAsync();
            RemedyList.ItemsSource = all;
        }

        async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");

        async void OnAddRemedyClicked(object sender, EventArgs e)
        {
            var name = NewRemedyEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            await _remedyRepository.AddRemedyMasterAsync(name);
            NewRemedyEntry.Text = string.Empty;
            await LoadAsync();
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.CommandParameter is not int id) return;

            var confirm = await DisplayAlert("Delete", "Delete this remedy?", "Delete", "Cancel");
            if (!confirm) return;

            await _remedyRepository.DeleteRemedyMasterAsync(id);
            await LoadAsync();
        }
    }
}