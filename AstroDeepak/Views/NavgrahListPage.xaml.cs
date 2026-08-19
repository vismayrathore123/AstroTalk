using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Views
{
    // Small view-only model, just for rendering the 9 tiles - not a domain entity.
    public class NavgrahOption
    {
        public NavgrahType Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
    }

    public partial class NavgrahListPage : ContentPage, IQueryAttributable
    {
        private readonly IPersonService _personService;
        private PersonDto? _draft;
        private NavgrahOption? _selected;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("PersonDraft", out var value) && value is PersonDto dto)
                _draft = dto;
        }

        public NavgrahListPage(IPersonService personService)
        {
            InitializeComponent();
            _personService = personService;
            GrahList.ItemsSource = BuildOptions();
        }

        static List<NavgrahOption> BuildOptions() => new()
        {
            new() { Id = NavgrahType.Surya,   Name = "Surya",   Symbol = "☀️" },
            new() { Id = NavgrahType.Chandra, Name = "Chandra", Symbol = "🌙" },
            new() { Id = NavgrahType.Mangal,  Name = "Mangal",  Symbol = "🔴" },
            new() { Id = NavgrahType.Budh,    Name = "Budh",    Symbol = "💚" },
            new() { Id = NavgrahType.Guru,    Name = "Guru",    Symbol = "🟡" },
            new() { Id = NavgrahType.Shukra,  Name = "Shukra",  Symbol = "🤍" },
            new() { Id = NavgrahType.Shani,   Name = "Shani",   Symbol = "⚫" },
            new() { Id = NavgrahType.Rahu,    Name = "Rahu",    Symbol = "🐉" },
            new() { Id = NavgrahType.Ketu,    Name = "Ketu",    Symbol = "🌫️" },
        };

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // CollectionView SelectionMode="Single" already guarantees only
            // one item can be highlighted at a time - we just read it here.
            _selected = e.CurrentSelection.FirstOrDefault() as NavgrahOption;
            ConfirmButton.IsEnabled = _selected != null;
        }

        async void OnConfirmClicked(object sender, EventArgs e)
        {
            if (_draft == null || _selected == null) return;

            _draft.SelectedGrah = _selected.Id.ToString();
            await _personService.SaveAsync(_draft);

            await DisplayAlert("Saved", $"Kundli saved with {_selected.Name}.", "OK");

            // Clear the whole navigation stack and go back to the landing page.
            await Shell.Current.GoToAsync("//main");
        }
    }
}