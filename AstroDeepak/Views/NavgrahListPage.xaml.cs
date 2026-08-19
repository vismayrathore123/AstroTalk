using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;

namespace AstroDeepak.Views
{
    // Small view-only model, just for rendering the tiles - not a domain entity.
    public class NavgrahOption
    {
        public string Name { get; set; } = string.Empty;
    }

    public partial class NavgrahListPage : ContentPage, IQueryAttributable
    {
        private readonly IPersonService _personService;
        private readonly IMasterDataRepository _masterDataRepository;
        private PersonDto? _draft;
        private NavgrahOption? _selected;
        private bool _isAdminMode;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("PersonDraft", out var value) && value is PersonDto dto)
                _draft = dto;

            if (query.TryGetValue("AdminMode", out var adminVal) &&
                string.Equals(adminVal?.ToString(), "true", StringComparison.OrdinalIgnoreCase))
                _isAdminMode = true;
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
                .Select(n => new NavgrahOption { Name = n.Name })
                .ToList();
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = e.CurrentSelection.FirstOrDefault() as NavgrahOption;
            ConfirmButton.IsEnabled = _selected != null;
        }

        async void OnConfirmClicked(object sender, EventArgs e)
        {
            if (_selected == null) return;
            if (!_isAdminMode && _draft == null) return;

            var navParams = new Dictionary<string, object>
            {
                { "NavgrahName", _selected.Name },
                { "Mode", _isAdminMode ? "Admin" : "Person" }
            };

            if (!_isAdminMode)
                navParams["PersonDraft"] = _draft!;

            await Shell.Current.GoToAsync("remedies", navParams);
        }
    }
}