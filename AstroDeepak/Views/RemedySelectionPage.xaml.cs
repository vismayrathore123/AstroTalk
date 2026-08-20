using System.Collections.ObjectModel;
using System.Text.Json;
using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;

namespace AstroDeepak.Views
{
    public partial class RemedySelectionPage : ContentPage, IQueryAttributable
    {
        private readonly IRemedyRepository _remedyRepository;
        private readonly IPersonService _personService;
        private readonly ObservableCollection<RemedyCheckItem> _items = new();

        private PersonDto? _draft;
        private string _navgrahName = string.Empty;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("PersonDraft", out var value) && value is PersonDto dto)
                _draft = dto;

            if (query.TryGetValue("NavgrahName", out var nav) && nav is string navStr)
                _navgrahName = navStr;
        }

        public RemedySelectionPage(IRemedyRepository remedyRepository, IPersonService personService)
        {
            InitializeComponent();
            _remedyRepository = remedyRepository;
            _personService = personService;
            RemedyList.ItemsSource = _items;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            HeaderLabel.Text = $"Remedies for {_navgrahName}";
            SubHeaderLabel.Text = "Select remedies for this Kundli";

            await LoadRemediesAsync();
        }

        async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");

        async Task LoadRemediesAsync()
        {
            var allRemedies = await _remedyRepository.GetAllRemediesAsync();

            _items.Clear();
            foreach (var r in allRemedies)
            {
                _items.Add(new RemedyCheckItem
                {
                    Name = r.Name,
                    IsChecked = false
                });
            }
        }

        async void OnAddRemedyClicked(object sender, EventArgs e)
        {
            var name = NewRemedyEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            await _remedyRepository.AddRemedyMasterAsync(name);
            NewRemedyEntry.Text = string.Empty;
            await LoadRemediesAsync();
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_draft == null) return;

            var selectedNames = _items.Where(i => i.IsChecked).Select(i => i.Name).ToList();
            if (selectedNames.Count == 0)
            {
                await DisplayAlert("No remedies selected", "Please select at least one remedy.", "OK");
                return;
            }

            // Load whatever history already exists for this person, and append a new entry.
            // Duplicates across entries are fine - nothing is deduped.
            var history = string.IsNullOrWhiteSpace(_draft.RemediesJson)
                ? new List<RemedyHistoryEntry>()
                : (JsonSerializer.Deserialize<List<RemedyHistoryEntry>>(_draft.RemediesJson) ?? new List<RemedyHistoryEntry>());

            history.Add(new RemedyHistoryEntry
            {
                CreatedAt = DateTime.Now,
                Remedies = selectedNames
            });

            _draft.SelectedGrah = _navgrahName;
            _draft.SelectedRemedies = string.Join(", ", selectedNames); // quick flat display value
            _draft.RemediesJson = JsonSerializer.Serialize(history);

            await _personService.SaveAsync(_draft);

            await DisplayAlert("Saved", "Kundli saved successfully.", "OK");
            await Shell.Current.GoToAsync("//search");
        }
    }
}