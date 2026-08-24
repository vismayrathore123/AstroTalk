using AstroDeepak.Application.Interfaces;

namespace AstroDeepak.Views
{
    public partial class SearchPage : ContentPage
    {
        private readonly IPersonService _personService;

        public SearchPage(IPersonService personService)
        {
            InitializeComponent();
            _personService = personService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadRecentAsync();
        }

        async Task LoadRecentAsync()
        {
            SectionLabel.Text = "RECENTLY ADDED";
            ResultsList.ItemsSource = await _personService.GetRecentAsync(10);
        }

        async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var term = e.NewTextValue;
            if (string.IsNullOrWhiteSpace(term))
            {
                await LoadRecentAsync();
                return;
            }

            SectionLabel.Text = "SEARCH RESULTS";
            ResultsList.ItemsSource = await _personService.SearchAsync(term);
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int id)
                await Shell.Current.GoToAsync($"//form?PersonId={id}");
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.CommandParameter is not int id) return;

            var confirm = await DisplayAlert("Delete", "Delete this record? This cannot be undone.", "Delete", "Cancel");
            if (!confirm) return;

            await _personService.DeleteAsync(id);

            if (string.IsNullOrWhiteSpace(SearchEntry.Text))
                await LoadRecentAsync();
            else
                ResultsList.ItemsSource = await _personService.SearchAsync(SearchEntry.Text);
        }

        // Toggles the dropdown open/closed, and flips the button itself between
        // ☰ and ✕ so there is only ever one icon - never a hamburger showing
        // "through" a separate close button.
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

        void CloseMenu()
        {
            MenuDropdown.IsVisible = false;
            MenuOverlayBackground.IsVisible = false;
            HamburgerButton.Text = "☰";
        }
        async void OnPrecautionsTapped(object sender, EventArgs e)
        {
            CloseMenu();
            await Shell.Current.GoToAsync("precautions");
        }

    }
}