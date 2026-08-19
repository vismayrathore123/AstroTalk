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
        private readonly ObservableCollection<RemedyCheckItem> _items = new();

        private PersonDto? _draft;
        private string _navgrahName = string.Empty;
        private string _mode = "Person";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("PersonDraft", out var value) && value is PersonDto dto)
                _draft = dto;

            if (query.TryGetValue("NavgrahName", out var nav) && nav is string navStr)
                _navgrahName = navStr;

            if (query.TryGetValue("Mode", out var mode) && mode is string modeStr)
                _mode = modeStr;
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
            SubHeaderLabel.Text = _mode == "Admin"
                ? "Select remedies that apply to this Grah"
                : "Select remedies for this Kundli";

            await LoadRemediesAsync();
        }

        async Task LoadRemediesAsync()
        {
            var allRemedies = await _remedyRepository.GetAllRemediesAsync();
            var preselected = await _remedyRepository.GetRemediesForNavgrahAsync(_navgrahName);

            _items.Clear();
            foreach (var r in allRemedies)
            {
                _items.Add(new RemedyCheckItem
                {
                    Name = r.Name,
                    IsChecked = preselected.Contains(r.Name)
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
            var selectedNames = _items.Where(i => i.IsChecked).Select(i => i.Name).ToList();

            if (_mode == "Admin")
            {
                await _remedyRepository.SaveNavgrahRemediesAsync(_navgrahName, selectedNames);
                await DisplayAlert("Saved", $"Remedies updated for {_navgrahName}.", "OK");
                await Shell.Current.GoToAsync("//search");
                return;
            }

            if (_draft == null) return;

            _draft.SelectedGrah = _navgrahName;
            _draft.SelectedRemedies = string.Join(", ", selectedNames);
            await _personService.SaveAsync(_draft);

            await DisplayAlert("Saved", "Kundli saved successfully.", "OK");
            await Shell.Current.GoToAsync("navgrah");
        }
    }
}