using System.Collections.ObjectModel;
using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;

namespace AstroDeepak.Views
{
    public partial class RemedySelectionPage : ContentPage, IQueryAttributable
    {
        private readonly IRemedyRepository _remedyRepository;
        private readonly IPersonService _personService;
        private readonly IUserRemedyService _userRemedyService;

        private readonly List<RemedyCheckItem> _allItems = new();       // full set (filter never loses selection)
        private readonly ObservableCollection<RemedyCheckItem> _items = new(); // what's shown after filtering

        private PersonDto? _draft;
        private int _navgrahId;
        private string _navgrahName = string.Empty;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("PersonDraft", out var value) && value is PersonDto dto)
                _draft = dto;

            if (query.TryGetValue("NavgrahId", out var navId) && navId is int navIdInt)
                _navgrahId = navIdInt;

            if (query.TryGetValue("NavgrahName", out var nav) && nav is string navStr)
                _navgrahName = navStr;
        }

        public RemedySelectionPage(IRemedyRepository remedyRepository, IPersonService personService, IUserRemedyService userRemedyService)
        {
            InitializeComponent();
            _remedyRepository = remedyRepository;
            _personService = personService;
            _userRemedyService = userRemedyService;
            RemedyList.ItemsSource = _items;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            HeaderLabel.Text = $"Remedies for {_navgrahName}";
            SubHeaderLabel.Text = "Select remedies for this Kundli";
            SearchEntry.Text = string.Empty;

            await LoadRemediesAsync();
        }

        async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");

        async Task LoadRemediesAsync()
        {
            var remedies = await _remedyRepository.GetRemediesByNavgrahIdAsync(_navgrahId);

            _allItems.Clear();
            foreach (var r in remedies)
            {
                _allItems.Add(new RemedyCheckItem
                {
                    Name = r.Name,
                    IsChecked = false
                });
            }

            ApplyFilter(SearchEntry.Text);
        }

        void ApplyFilter(string? term)
        {
            _items.Clear();

            var filtered = string.IsNullOrWhiteSpace(term)
                ? _allItems
                : _allItems.Where(i => i.Name.Contains(term, StringComparison.OrdinalIgnoreCase));

            foreach (var item in filtered)
                _items.Add(item);
        }

        void OnSearchTextChanged(object sender, TextChangedEventArgs e)
            => ApplyFilter(e.NewTextValue);

        async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_draft == null) return;

            // Selection is read from _allItems so a match hidden by the search
            // filter doesn't lose its checked state.
            var selectedNames = _allItems.Where(i => i.IsChecked).Select(i => i.Name).ToList();
            if (selectedNames.Count == 0)
            {
                await DisplayAlert("No remedies selected", "Please select at least one remedy.", "OK");
                return;
            }

            _draft.Grah = _navgrahName;

            var personId = await _personService.SaveAsync(_draft);
            _draft.Id = personId; // keep updating the same person on subsequent Grah rounds

            await _userRemedyService.SaveSelectedRemediesAsync(
                personId,
                _navgrahId,
                selectedNames,
                WhatsAppSwitch.IsToggled);

            await DisplayAlert("Saved", "Kundli saved successfully.", "OK");

            // Go back to Grah picker (not Open Kundli) so the user can add
            // remedies for another Grah for the same person if they want.
            var navParams = new Dictionary<string, object> { { "PersonDraft", _draft }, { "Mode", "Person" } };
            await Shell.Current.GoToAsync("navgrah", navParams);
        }
    }
}