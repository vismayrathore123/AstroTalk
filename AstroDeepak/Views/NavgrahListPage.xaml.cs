using AstroDeepak.Application.DTOs;
using AstroDeepak.Domain.Abstractions;

namespace AstroDeepak.Views
{
    public class NavgrahOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public partial class NavgrahListPage : ContentPage, IQueryAttributable
    {
        private readonly IMasterDataRepository _masterDataRepository;
        private PersonDto? _draft;
        private NavgrahOption? _selected;
        private string _mode = "Person"; // "Person": attach remedies to a Kundli. "Master": manage a Grah's remedy list.

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("PersonDraft", out var value) && value is PersonDto dto)
                _draft = dto;

            _mode = query.TryGetValue("Mode", out var modeObj) && modeObj is string modeStr
                ? modeStr
                : "Person";
        }

        public NavgrahListPage(IMasterDataRepository masterDataRepository)
        {
            InitializeComponent();
            _masterDataRepository = masterDataRepository;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            ConfirmButton.Text = _mode == "Master" ? "Manage Remedies →" : "Add Remedies →";
            ConfirmButton.IsEnabled = false;
            _selected = null;

            var navgrahs = await _masterDataRepository.GetNavgrahsAsync();
            GrahList.ItemsSource = navgrahs
                .Select(n => new NavgrahOption { Id = n.Id, Name = n.Name })
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
            if (_selected == null) return;

            if (_mode == "Master")
            {
                var masterNavParams = new Dictionary<string, object>
                {
                    { "NavgrahId", _selected.Id },
                    { "NavgrahName", _selected.Name }
                };
                await Shell.Current.GoToAsync("masterremedies", masterNavParams);
                return;
            }

            if (_draft == null) return;

            var navParams = new Dictionary<string, object>
            {
                { "NavgrahId", _selected.Id },
                { "NavgrahName", _selected.Name },
                { "PersonDraft", _draft }
            };

            await Shell.Current.GoToAsync("remedies", navParams);
        }
    }
}