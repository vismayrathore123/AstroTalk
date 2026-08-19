using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;

namespace AstroDeepak.Views
{
    // Small view-only model, just for rendering the tiles - not a domain entity.
    public class NavgrahOption
    {
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
    }

    public partial class NavgrahListPage : ContentPage, IQueryAttributable
    {
        private readonly IPersonService _personService;
        private readonly IMasterDataRepository _masterDataRepository;
        private PersonDto? _draft;
        private NavgrahOption? _selected;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("PersonDraft", out var value) && value is PersonDto dto)
                _draft = dto;
        }

        public NavgrahListPage(IPersonService personService, IMasterDataRepository masterDataRepository)
        {
            InitializeComponent();
            _personService = personService;
            _masterDataRepository = masterDataRepository;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var navgrahs = await _masterDataRepository.GetNavgrahsAsync();
            GrahList.ItemsSource = navgrahs
                .Select(n => new NavgrahOption { Name = n.Name, Symbol = n.Symbol })
                .ToList();
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = e.CurrentSelection.FirstOrDefault() as NavgrahOption;
            ConfirmButton.IsEnabled = _selected != null;
        }

        async void OnConfirmClicked(object sender, EventArgs e)
        {
            if (_draft == null || _selected == null) return;

            _draft.SelectedGrah = _selected.Name;
            await _personService.SaveAsync(_draft);

            await DisplayAlert("Saved", $"Kundli saved with {_selected.Name}.", "OK");

            await Shell.Current.GoToAsync("//main");
        }
    }
}