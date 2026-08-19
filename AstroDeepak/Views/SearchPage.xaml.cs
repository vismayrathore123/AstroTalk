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
            => await Shell.Current.GoToAsync("form");

        async void OnRecordTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is int id)
                await Shell.Current.GoToAsync($"form?Id={id}");
        }
    }
}
