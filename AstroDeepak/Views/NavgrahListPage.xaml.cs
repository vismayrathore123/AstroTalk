using AstroDeepak.Application.DTOs;
using AstroDeepak.Domain.Abstractions;

namespace AstroDeepak.Views
{
    public class NavgrahOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool AlreadyAdded { get; set; }
        public string StatusText => AlreadyAdded ? "✓ Added" : string.Empty;
    }

    public partial class NavgrahListPage : ContentPage, IQueryAttributable
    {
        private readonly IMasterDataRepository _masterDataRepository;
        private readonly IUsersRemedyRepository _usersRemedyRepository;
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

        public NavgrahListPage(IMasterDataRepository masterDataRepository, IUsersRemedyRepository usersRemedyRepository)
        {
            InitializeComponent();
            _masterDataRepository = masterDataRepository;
            _usersRemedyRepository = usersRemedyRepository;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            ConfirmButton.Text = _mode == "Master" ? "Manage Remedies →" : "Add Remedies →";
            ConfirmButton.IsEnabled = false;
            _selected = null;

            var navgrahs = await _masterDataRepository.GetNavgrahsAsync();

            // If editing an existing person, mark Grahs that already have saved remedies.
            var alreadyAddedIds = new HashSet<int>();
            if (_mode == "Person" && _draft != null && _draft.Id > 0)
            {
                var existingRemedies = await _usersRemedyRepository.GetByPersonIdAsync(_draft.Id);
                alreadyAddedIds = existingRemedies.Select(r => r.NavgrahId).ToHashSet();
            }

            GrahList.ItemsSource = navgrahs
                .Select(n => new NavgrahOption
                {
                    Id = n.Id,
                    Name = n.Name,
                    AlreadyAdded = alreadyAddedIds.Contains(n.Id)
                })
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