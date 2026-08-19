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

        async void OnAddNewClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//form");

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

        async void OnHamburgerClicked(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Menu", "Cancel", null, "Add Remedies");

            if (action == "Add Remedies")
                await Shell.Current.GoToAsync("navgrah?AdminMode=true");
        }
    }
}