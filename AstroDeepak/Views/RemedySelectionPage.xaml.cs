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

            _draft.Grah = _navgrahName;

            // Save the person first so a brand-new record gets a real Id.
            var personId = await _personService.SaveAsync(_draft);

            await _userRemedyService.SaveSelectedRemediesAsync(
                personId,
                _navgrahId,
                selectedNames,
                WhatsAppSwitch.IsToggled);

            await DisplayAlert("Saved", "Kundli saved successfully.", "OK");
            await Shell.Current.GoToAsync("//search");
        }
    }
}