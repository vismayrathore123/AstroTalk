using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Views
{
    public partial class GrahRemedyPage : ContentPage, IQueryAttributable
    {
        private readonly IRemedyRepository _remedyRepository;
        private int _navgrahId;
        private string _navgrahName = string.Empty;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("NavgrahId", out var idVal) && idVal is int id)
                _navgrahId = id;

            if (query.TryGetValue("NavgrahName", out var nameVal) && nameVal is string name)
                _navgrahName = name;
        }

        public GrahRemedyPage(IRemedyRepository remedyRepository)
        {
            InitializeComponent();
            _remedyRepository = remedyRepository;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            HeaderLabel.Text = $"Remedies for {_navgrahName}";
            Title = $"Remedies - {_navgrahName}";
            await LoadAsync();
        }

        async Task LoadAsync()
        {
            var remedies = await _remedyRepository.GetRemediesByNavgrahIdAsync(_navgrahId);
            RemedyList.ItemsSource = remedies;
        }

        async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");

        async void OnAddRemedyClicked(object sender, EventArgs e)
        {
            var name = NewRemedyEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            await _remedyRepository.AddRemedyMasterAsync(name, _navgrahId);
            NewRemedyEntry.Text = string.Empty;
            await LoadAsync();
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.CommandParameter is not RemedyMaster remedy) return;

            var newName = await DisplayPromptAsync("Edit Remedy", "Update remedy name", "Save", "Cancel", initialValue: remedy.Name);
            if (string.IsNullOrWhiteSpace(newName)) return;

            await _remedyRepository.UpdateRemedyMasterAsync(remedy.Id, newName.Trim());
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


        void OnHamburgerClicked(object sender, EventArgs e)
        {
            bool opening = !MenuDropdown.IsVisible;
            MenuDropdown.IsVisible = opening;
            MenuOverlayBackground.IsVisible = opening;
            HamburgerButton.Text = opening ? "✕" : "☰";
        }

        void OnMenuOverlayTapped(object sender, EventArgs e)
            => CloseMenu();

        async void OnAddRemediesTapped(object sender, EventArgs e)
        {
            CloseMenu();
            var navParams = new Dictionary<string, object> { { "Mode", "Master" } };
            await Shell.Current.GoToAsync("navgrah", navParams);
        }

        async void OnContactUsTapped(object sender, EventArgs e)
        {
            CloseMenu();
            await DisplayAlert("Contact Us", "Contact us feature coming soon.", "OK");
        }
        async void OnPrecautionsTapped(object sender, EventArgs e)
        {
            CloseMenu();
            await Shell.Current.GoToAsync("precautions");
        }
        void CloseMenu()
        {
            MenuDropdown.IsVisible = false;
            MenuOverlayBackground.IsVisible = false;
            HamburgerButton.Text = "☰";
        }
    }
}