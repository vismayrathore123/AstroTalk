using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;

namespace AstroDeepak.Views
{
    public class NavgrahOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool AlreadyAdded { get; set; }
    }

    public partial class NavgrahListPage : ContentPage, IQueryAttributable
    {
        private readonly IMasterDataRepository _masterDataRepository;
        private readonly IUsersRemedyRepository _usersRemedyRepository;
        private readonly IRemedyRepository _remedyRepository;
        private readonly IUserRemedyService _userRemedyService;
        private readonly IUserRemedyStagingService _stagingService;
        private readonly IPrecautionRepository _precautionRepository;
        private readonly IPermanentRemedyRepository _permanentRemedyRepository;

        private const int ColumnsPerRow = 3;

        private readonly Dictionary<NavgrahOption, Border> _grahTiles = new();
        private readonly Dictionary<NavgrahOption, Label> _statusLabels = new();
        private readonly Dictionary<NavgrahOption, VerticalStackLayout> _rowSlots = new();

        private readonly List<RemedyCheckItem> _currentRemedyItems = new();
        private VerticalStackLayout? _currentRemedyListHost;

        // Every Grah's chosen remedies for THIS session, keyed by NavgrahId.
        // Each entry keeps Name + independent Permanent/Yearly flags.
        private readonly Dictionary<int, List<RemedyChoiceDto>> _selectionsByGrah = new();
        private NavgrahOption? _openOption;

        // Precaution checkboxes, built once per OnAppearing.
        private readonly List<RemedyCheckItem> _precautionItems = new();

        private PersonDto? _draft;
        private NavgrahOption? _selected; // Master mode only
        private string _mode = "Person";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("PersonDraft", out var value) && value is PersonDto dto)
                _draft = dto;

            _mode = query.TryGetValue("Mode", out var modeObj) && modeObj is string modeStr
                ? modeStr
                : "Person";
        }

        public NavgrahListPage(
            IMasterDataRepository masterDataRepository,
            IUsersRemedyRepository usersRemedyRepository,
            IRemedyRepository remedyRepository,
            IUserRemedyService userRemedyService,
            IUserRemedyStagingService stagingService,
            IPrecautionRepository precautionRepository,
            IPermanentRemedyRepository permanentRemedyRepository)
        {
            InitializeComponent();
            _masterDataRepository = masterDataRepository;
            _usersRemedyRepository = usersRemedyRepository;
            _remedyRepository = remedyRepository;
            _userRemedyService = userRemedyService;
            _stagingService = stagingService;
            _precautionRepository = precautionRepository;
            _permanentRemedyRepository = permanentRemedyRepository;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            ConfirmButton.Text = _mode == "Master" ? "Manage Remedies →" : "Save";
            _selected = null;
            _openOption = null;
            _grahTiles.Clear();
            _statusLabels.Clear();
            _rowSlots.Clear();
            _currentRemedyItems.Clear();
            _currentRemedyListHost = null;
            GrahGridHost.Children.Clear();

            var navgrahs = await _masterDataRepository.GetNavgrahsAsync();

            var alreadyAddedIds = new HashSet<int>();
            if (_mode == "Person" && _draft != null && _draft.Id > 0)
            {
                var existingRemedies = await _usersRemedyRepository.GetByPersonIdAsync(_draft.Id);
                alreadyAddedIds = existingRemedies.Select(r => r.NavgrahId).ToHashSet();
            }

            var options = navgrahs
                .Select(n => new NavgrahOption
                {
                    Id = n.Id,
                    Name = n.Name,
                    AlreadyAdded = alreadyAddedIds.Contains(n.Id)
                })
                .ToList();

            BuildGrahGrid(options);
            RefreshTileHighlights();

            if (_mode == "Person" && _draft != null && !string.IsNullOrWhiteSpace(_draft.Name))
                HeaderLabel.Text = $"Choose Grah for {_draft.Name}";
            else
                HeaderLabel.Text = "Choose ONE Grah";

            if (_mode == "Person")
                await LoadPrecautionsAsync();
            else
                PrecautionsHost.Children.Clear();

            ConfirmButton.IsEnabled = _mode == "Master"
                ? false
                : _selectionsByGrah.Values.Any(list => list.Count > 0);
        }

        async Task LoadPrecautionsAsync()
        {
            PrecautionsHost.Children.Clear();
            _precautionItems.Clear();

            var master = await _precautionRepository.GetAllAsync();

            var already = string.IsNullOrWhiteSpace(_draft?.Precautions)
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(_draft!.Precautions.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

            foreach (var p in master)
            {
                var item = new RemedyCheckItem { Name = p.Text, IsChecked = already.Contains(p.Text) };
                _precautionItems.Add(item);

                var checkBox = new CheckBox();
                checkBox.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(RemedyCheckItem.IsChecked), source: item));

                var label = new Label { Text = item.Name, FontSize = 15, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(8, 0, 0, 0) };

                var row = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                    Children = { checkBox, label }
                };
                Grid.SetColumn(label, 1);

                PrecautionsHost.Children.Add(new Border
                {
                    Style = (Style)Microsoft.Maui.Controls.Application.Current!.Resources["CardBorder"],
                    Padding = 12,
                    Content = row
                });
            }
        }

        void BuildGrahGrid(List<NavgrahOption> options)
        {
            for (int rowStart = 0; rowStart < options.Count; rowStart += ColumnsPerRow)
            {
                var rowItems = options.Skip(rowStart).Take(ColumnsPerRow).ToList();

                var tilesGrid = new Grid
                {
                    ColumnSpacing = 12,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Star)
                    }
                };

                var rowSlot = new VerticalStackLayout { Spacing = 10, IsVisible = false, Margin = new Thickness(0, 10, 0, 0) };

                for (int col = 0; col < rowItems.Count; col++)
                {
                    var option = rowItems[col];
                    var border = CreateGrahTile(option);
                    Grid.SetColumn(border, col);
                    tilesGrid.Children.Add(border);

                    _rowSlots[option] = rowSlot;
                }

                var rowContainer = new VerticalStackLayout { Spacing = 0, Children = { tilesGrid, rowSlot } };
                GrahGridHost.Children.Add(rowContainer);
            }
        }

        Border CreateGrahTile(NavgrahOption option)
        {
            var nameLabel = new Label
            {
                Text = option.Name,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };

            var statusLabel = new Label
            {
                Text = option.AlreadyAdded ? "✓ Added" : string.Empty,
                FontSize = 10,
                TextColor = (Color)Microsoft.Maui.Controls.Application.Current!.Resources["Gold"],
                HorizontalOptions = LayoutOptions.Center
            };

            var border = new Border
            {
                Style = (Style)Microsoft.Maui.Controls.Application.Current!.Resources["CardBorder"],
                Padding = 8,
                HeightRequest = 80,
                Content = new VerticalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = { nameLabel, statusLabel }
                }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => OnGrahTileTapped(option);
            border.GestureRecognizers.Add(tap);

            _grahTiles[option] = border;
            _statusLabels[option] = statusLabel;
            return border;
        }

        void RefreshTileHighlights()
        {
            foreach (var kvp in _grahTiles)
            {
                var option = kvp.Key;
                bool hasPending = _selectionsByGrah.TryGetValue(option.Id, out var list) && list.Count > 0;
                bool isOpen = _openOption == option || (_mode != "Person" && _selected == option);
                bool highlight = hasPending || isOpen;

                kvp.Value.Stroke = highlight
                    ? (Color)Microsoft.Maui.Controls.Application.Current!.Resources["Gold"]
                    : (Color)Microsoft.Maui.Controls.Application.Current!.Resources["Violet"];
                kvp.Value.StrokeThickness = highlight ? 2 : 1;

                if (_statusLabels.TryGetValue(option, out var statusLabel))
                {
                    statusLabel.Text = hasPending
                        ? "• Selected"
                        : (option.AlreadyAdded ? "✓ Added" : string.Empty);
                }
            }
        }

        // Reads the CURRENT on-screen checkbox state for the open Grah and stores
        // it into _selectionsByGrah, so switching to another Grah (or Confirm)
        // never loses what was just ticked.
        void CommitCurrentSelectionToMemory()
        {
            if (_openOption == null) return;

            var chosen = _currentRemedyItems
                .Where(i => i.IsChecked)
                .Select(i => new RemedyChoiceDto
                {
                    Name = i.Name,
                    IsPermanent = i.IsPermanent,
                    IsYearly = i.IsYearly
                })
                .ToList();

            if (chosen.Count > 0)
                _selectionsByGrah[_openOption.Id] = chosen;
            else
                _selectionsByGrah.Remove(_openOption.Id);
        }

        async void OnGrahTileTapped(NavgrahOption option)
        {
            if (_mode != "Person")
            {
                _selected = option;
                RefreshTileHighlights();
                ConfirmButton.IsEnabled = true;
                return;
            }

            CommitCurrentSelectionToMemory();

            bool reopeningSame = _openOption == option;

            foreach (var slot in _rowSlots.Values.Distinct())
            {
                slot.IsVisible = false;
                slot.Children.Clear();
            }
            _currentRemedyItems.Clear();
            _currentRemedyListHost = null;

            _openOption = reopeningSame ? null : option;
            RefreshTileHighlights();
            ConfirmButton.IsEnabled = _selectionsByGrah.Values.Any(list => list.Count > 0);

            if (_openOption == null) return;
            if (!_rowSlots.TryGetValue(option, out var slotToOpen)) return;

            await OpenRemedyAccordionAsync(option, slotToOpen);
        }

        async Task OpenRemedyAccordionAsync(NavgrahOption option, VerticalStackLayout slot)
        {
            var headerLabel = new Label
            {
                Text = $"Remedies for {option.Name}",
                Style = (Style)Microsoft.Maui.Controls.Application.Current!.Resources["TitleLabel"],
                FontSize = 18
            };

            var searchEntry = new Entry { Placeholder = "Search remedies..." };
            var searchIcon = new Label { Text = "🔍", VerticalOptions = LayoutOptions.Center, FontSize = 18, Margin = new Thickness(0, 0, 8, 0) };
            var searchGrid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                Children = { searchIcon, searchEntry }
            };
            Grid.SetColumn(searchEntry, 1);

            var searchBorder = new Border
            {
                Style = (Style)Microsoft.Maui.Controls.Application.Current!.Resources["CardBorder"],
                Padding = 10,
                Content = searchGrid
            };

            var hintLabel = new Label
            {
                Text = "Tick to include. Permanent and Yearly are independent - a remedy can be either or both.",
                FontSize = 11,
                Style = (Style)Microsoft.Maui.Controls.Application.Current!.Resources["SubtitleLabel"]
            };

            var remedyListHost = new VerticalStackLayout { Spacing = 8 };

            slot.Children.Add(headerLabel);
            slot.Children.Add(searchBorder);
            slot.Children.Add(hintLabel);
            slot.Children.Add(remedyListHost);
            slot.IsVisible = true;

            _currentRemedyListHost = remedyListHost;

            var remedies = await _remedyRepository.GetRemediesByNavgrahIdAsync(option.Id);

            // Pull prior state for this Grah, in priority order:
            // 1) still-pending choices from this same editing session (most authoritative -
            //    reflects exactly what the user last set, incl. unchecked boxes).
            // 2) otherwise, reconstruct from saved data (CurrentSuggestedRemedy => Yearly,
            //    PermanentRemedy table => Permanent).
            Dictionary<string, RemedyChoiceDto>? pendingByName = null;
            if (_selectionsByGrah.TryGetValue(option.Id, out var pending))
            {
                pendingByName = pending.ToDictionary(r => r.Name, StringComparer.OrdinalIgnoreCase);
            }

            var yearlyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var permanentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (pendingByName == null && _draft != null && _draft.Id > 0)
            {
                var existing = await _userRemedyService.GetAsync(_draft.Id, option.Id);
                if (existing != null && !string.IsNullOrWhiteSpace(existing.CurrentSuggestedRemedy))
                {
                    foreach (var name in existing.CurrentSuggestedRemedy.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                        yearlyNames.Add(name);
                }
            }

            if (_draft != null && _draft.Id > 0)
            {
                var permanent = await _permanentRemedyRepository.GetByPersonAndNavgrahAsync(_draft.Id, option.Id);
                foreach (var p in permanent) permanentNames.Add(p.RemedyName);
            }

            _currentRemedyItems.Clear();
            foreach (var r in remedies)
            {
                if (pendingByName != null && pendingByName.TryGetValue(r.Name, out var pendingChoice))
                {
                    _currentRemedyItems.Add(new RemedyCheckItem
                    {
                        Name = r.Name,
                        IsChecked = true,
                        IsPermanent = pendingChoice.IsPermanent,
                        IsYearly = pendingChoice.IsYearly
                    });
                }
                else
                {
                    bool wasYearly = yearlyNames.Contains(r.Name);
                    bool wasPermanent = permanentNames.Contains(r.Name);

                    _currentRemedyItems.Add(new RemedyCheckItem
                    {
                        Name = r.Name,
                        IsChecked = wasYearly || wasPermanent,
                        IsPermanent = wasPermanent,
                        IsYearly = wasYearly
                    });
                }
            }

            RenderRemedyItems(_currentRemedyItems);

            searchEntry.TextChanged += (s, e) =>
            {
                var term = e.NewTextValue;
                var filtered = string.IsNullOrWhiteSpace(term)
                    ? _currentRemedyItems
                    : _currentRemedyItems.Where(i => i.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
                RenderRemedyItems(filtered);
            };
        }

        void RenderRemedyItems(IEnumerable<RemedyCheckItem> items)
        {
            if (_currentRemedyListHost == null) return;
            _currentRemedyListHost.Children.Clear();

            foreach (var item in items)
            {
                var includeCheckBox = new CheckBox();
                includeCheckBox.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(RemedyCheckItem.IsChecked), source: item));
                includeCheckBox.CheckedChanged += (s, e) =>
                {
                    ConfirmButton.IsEnabled = _selectionsByGrah.Values.Any(l => l.Count > 0)
                        || _currentRemedyItems.Any(i => i.IsChecked);
                };

                var nameLabel = new Label
                {
                    Text = item.Name,
                    FontSize = 15,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(8, 0, 0, 0)
                };

                // Two independent checkboxes - a remedy can be Permanent, Yearly, or both.
                var permanentCheckBox = new CheckBox();
                permanentCheckBox.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(RemedyCheckItem.IsPermanent), source: item));
                var permanentLabel = new Label { Text = "Permanent", FontSize = 12, VerticalOptions = LayoutOptions.Center };

                var yearlyCheckBox = new CheckBox();
                yearlyCheckBox.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(RemedyCheckItem.IsYearly), source: item));
                var yearlyLabel = new Label { Text = "Yearly", FontSize = 12, VerticalOptions = LayoutOptions.Center };

                var tagsStack = new HorizontalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.Center,
                    Children = { yearlyCheckBox, yearlyLabel, permanentCheckBox, permanentLabel }
                };

                var row = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    },
                    Children = { includeCheckBox, nameLabel, tagsStack }
                };
                Grid.SetColumn(nameLabel, 1);
                Grid.SetColumn(tagsStack, 2);

                var border = new Border
                {
                    Style = (Style)Microsoft.Maui.Controls.Application.Current!.Resources["CardBorder"],
                    Padding = 12,
                    Content = row
                };

                _currentRemedyListHost.Children.Add(border);
            }
        }

        async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");

        async void OnConfirmClicked(object sender, EventArgs e)
        {
            if (_mode == "Master")
            {
                if (_selected == null) return;

                var masterNavParams = new Dictionary<string, object>
                {
                    { "NavgrahId", _selected.Id },
                    { "NavgrahName", _selected.Name }
                };
                await Shell.Current.GoToAsync("masterremedies", masterNavParams);
                return;
            }

            if (_draft == null) return;

            CommitCurrentSelectionToMemory();

            var selections = new List<GrahRemedySelectionDto>();
            foreach (var kvp in _selectionsByGrah)
            {
                if (kvp.Value.Count == 0) continue;
                var option = _grahTiles.Keys.FirstOrDefault(o => o.Id == kvp.Key);
                if (option == null) continue;

                selections.Add(new GrahRemedySelectionDto
                {
                    NavgrahId = option.Id,
                    NavgrahName = option.Name,
                    Remedies = kvp.Value
                });
            }

            if (selections.Count == 0)
            {
                await DisplayAlert("No remedies selected", "Please select at least one remedy for at least one Grah.", "OK");
                return;
            }

            var selectedPrecautions = _precautionItems.Where(i => i.IsChecked).Select(i => i.Name).ToList();

            var stagingDto = new UserRemedyStagingDto
            {
                PersonId = _draft.Id,
                Name = _draft.Name,
                FatherName = _draft.FatherName,
                Gotra = _draft.Gotra,
                DOB = _draft.DOB ?? DateTime.Today,
                Time = _draft.Time,
                BirthPlace = _draft.BirthPlace,
                CountryCode = _draft.CountryCode,
                PhoneNo = _draft.PhoneNo,
                Address = _draft.Address,
                Grahan = _draft.Grahan,
                Selections = selections,
                SelectedPrecautions = selectedPrecautions
            };

            var stagingId = await _stagingService.SaveAsync(stagingDto);
            await Shell.Current.GoToAsync($"preview?StagingId={stagingId}");
        }

        void OnHamburgerClicked(object sender, EventArgs e)
        {
            bool opening = !MenuDropdown.IsVisible;
            MenuDropdown.IsVisible = opening;
            MenuOverlayBackground.IsVisible = opening;
            HamburgerButton.Text = opening ? "✕" : "☰";
        }

        void OnMenuOverlayTapped(object sender, EventArgs e)
            => CloseMenu();

        async void OnAddRemediesTapped(object sender, EventArgs e)
        {
            CloseMenu();
            var navParams = new Dictionary<string, object> { { "Mode", "Master" } };
            await Shell.Current.GoToAsync("navgrah", navParams);
        }

        async void OnPrecautionsTapped(object sender, EventArgs e)
        {
            CloseMenu();
            await Shell.Current.GoToAsync("precautions");
        }

        async void OnContactUsTapped(object sender, EventArgs e)
        {
            CloseMenu();
            await DisplayAlert("Contact Us", "Contact us feature coming soon.", "OK");
        }

        void CloseMenu()
        {
            MenuDropdown.IsVisible = false;
            MenuOverlayBackground.IsVisible = false;
            HamburgerButton.Text = "☰";
        }
    }
}