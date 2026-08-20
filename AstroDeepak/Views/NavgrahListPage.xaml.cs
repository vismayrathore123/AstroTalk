using AstroDeepak.Application.DTOs;
using AstroDeepak.Domain.Abstractions;

namespace AstroDeepak.Views
{
    public class NavgrahOption
    {
        public string Name { get; set; } = string.Empty;
    }

    public partial class NavgrahListPage : ContentPage, IQueryAttributable
    {
        private readonly IMasterDataRepository _masterDataRepository;
        private PersonDto? _draft;
        private NavgrahOption? _selected;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("PersonDraft", out var value) && value is PersonDto dto)
                _draft = dto;
        }

        public NavgrahListPage(IMasterDataRepository masterDataRepository)
        {
            InitializeComponent();
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

        async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");

        async void OnConfirmClicked(object sender, EventArgs e)
        {
            if (_selected == null || _draft == null) return;

            var navParams = new Dictionary<string, object>
            {
                { "NavgrahName", _selected.Name },
                { "PersonDraft", _draft }
            };

            await Shell.Current.GoToAsync("remedies", navParams);
        }
    }
}