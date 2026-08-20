using System.Collections.ObjectModel;
using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;

namespace AstroDeepak.Views
{
    public partial class RemedySelectionPage : ContentPage, IQueryAttributable
    {
        private readonly IRemedyRepository _remedyRepository;
        private readonly IUserRemedyService _userRemedyService;
        private readonly IUserRemedyStagingService _stagingService;

        private readonly List<RemedyCheckItem> _allItems = new();
        private readonly ObservableCollection<RemedyCheckItem> _items = new();

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

        public RemedySelectionPage(IRemedyRepository remedyRepository, IUserRemedyService userRemedyService, IUserRemedyStagingService stagingService)
        {
            InitializeComponent();
            _remedyRepository = remedyRepository;
            _userRemedyService = userRemedyService;
            _stagingService = stagingService;
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

            // Pre-select remedies this person already has saved for this Grah, if any.
            var alreadySelected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_draft != null && _draft.Id > 0)
            {
                var existing = await _userRemedyService.GetAsync(_draft.Id, _navgrahId);
                if (existing != null && !string.IsNullOrWhiteSpace(existing.CurrentSuggestedRemedy))
                {
                    foreach (var name in existing.CurrentSuggestedRemedy.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                        alreadySelected.Add(name);

                    SubHeaderLabel.Text = "Editing previously selected remedies for this Kundli";
                }
            }

            _allItems.Clear();
            foreach (var r in remedies)
            {
                _allItems.Add(new RemedyCheckItem
                {
                    Name = r.Name,
                    IsChecked = alreadySelected.Contains(r.Name)
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

            // Read selection from _allItems so a match hidden by the search filter
            // doesn't lose its checked state.
            var selectedNames = _allItems.Where(i => i.IsChecked).Select(i => i.Name).ToList();
            if (selectedNames.Count == 0)
            {
                await DisplayAlert("No remedies selected", "Please select at least one remedy.", "OK");
                return;
            }

            var stagingDto = new UserRemedyStagingDto
            {
                PersonId = _draft.Id,
                Name = _draft.Name,
                FatherName = _draft.FatherName,
                Gotra = _draft.Gotra,
                DOB = _draft.DOB ?? DateTime.Today,
                Time = _draft.Time,
                BirthPlace = _draft.BirthPlace,
                PhoneNo = _draft.PhoneNo,
                Address = _draft.Address,
                Grahan = _draft.Grahan,
                NavgrahId = _navgrahId,
                NavgrahName = _navgrahName,
                SelectedRemedies = selectedNames
            };

            var stagingId = await _stagingService.SaveAsync(stagingDto);

            await Shell.Current.GoToAsync($"preview?StagingId={stagingId}");
        }
    }
}