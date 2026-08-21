using System.Text.RegularExpressions;
using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Views
{
    [QueryProperty(nameof(PersonId), "PersonId")]
    public partial class PersonFormPage : ContentPage
    {
        private readonly IPersonService _personService;
        private readonly IMasterDataRepository _masterDataRepository;
        private readonly IAppLogger _logger;

        private int _editingId = 0;
        private List<GrahanMaster> _grahanOptions = new();
        private static readonly List<string> AmPmOptions = new() { "AM", "PM" };

        // 12-hour time like "10:30" or "1:05" - what the Time field must match.
        private static readonly Regex TimeRegex = new(@"^(0?[1-9]|1[0-2]):[0-5][0-9]$");

        public string PersonId
        {
            set
            {
                if (int.TryParse(value, out var id) && id > 0)
                    _editingId = id;
                else
                    _editingId = 0;
            }
        }

        public PersonFormPage(IPersonService personService, IMasterDataRepository masterDataRepository, IAppLogger logger)
        {
            InitializeComponent();
            _personService = personService;
            _masterDataRepository = masterDataRepository;
            _logger = logger;
            AmPmPicker.ItemsSource = AmPmOptions;

            CountryCodePicker.ItemsSource = CountryCodes.All;
            CountryCodePicker.ItemDisplayBinding = new Binding(nameof(CountryCodeOption.Display));

            // Live filtering - strip bad characters as the user types instead of
            // only complaining after Submit.
            TimeEntry.TextChanged += OnTimeTextChanged;
            PhoneEntry.TextChanged += OnPhoneTextChanged;
        }

        // Only digits and one colon allowed, e.g. "10:30". Anything else typed
        // (letters, symbols, a second colon) is silently dropped.
        void OnTimeTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = e.NewTextValue ?? string.Empty;
            var cleaned = new string(text.Where(c => char.IsDigit(c) || c == ':').ToArray());

            var firstColon = cleaned.IndexOf(':');
            if (firstColon >= 0)
            {
                var before = cleaned[..(firstColon + 1)];
                var after = cleaned[(firstColon + 1)..].Replace(":", "");
                cleaned = before + after;
            }

            if (cleaned != text)
                TimeEntry.Text = cleaned;
        }

        // Digits only for phone number - letters and symbols are dropped as typed.
        void OnPhoneTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = e.NewTextValue ?? string.Empty;
            var cleaned = new string(text.Where(char.IsDigit).ToArray());

            if (cleaned != text)
                PhoneEntry.Text = cleaned;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _grahanOptions = await _masterDataRepository.GetGrahansAsync();
            GrahanPicker.ItemsSource = _grahanOptions;

            if (_editingId > 0)
            {
                var dto = await _personService.GetByIdAsync(_editingId);
                if (dto != null)
                {
                    Populate(dto);
                    return;
                }

                _logger.LogWarning($"PersonFormPage: PersonId={_editingId} no longer found, showing blank form.");
                _editingId = 0;
            }

            ClearForm();
        }

        void Populate(PersonDto dto)
        {
            NameEntry.Text = dto.Name;
            FatherNameEntry.Text = dto.FatherName;
            GotraEntry.Text = dto.Gotra;
            DobPicker.Date = dto.DOB ?? DateTime.Today;

            var timeParts = (dto.Time ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            TimeEntry.Text = timeParts.Length > 0 ? timeParts[0] : string.Empty;
            AmPmPicker.SelectedItem = timeParts.Length > 1 && AmPmOptions.Contains(timeParts[1].ToUpperInvariant())
                ? timeParts[1].ToUpperInvariant()
                : "AM";

            BirthPlaceEntry.Text = dto.BirthPlace;

            CountryCodePicker.SelectedItem = string.IsNullOrWhiteSpace(dto.CountryCode)
                ? DefaultCountry()
                : CountryCodes.All.FirstOrDefault(c => c.DialCode == dto.CountryCode) ?? DefaultCountry();

            PhoneEntry.Text = dto.PhoneNo;
            AddressEntry.Text = dto.Address;

            var match = _grahanOptions.FirstOrDefault(g => g.Name == dto.Grahan);
            GrahanPicker.SelectedItem = match;
        }

        void ClearForm()
        {
            NameEntry.Text = string.Empty;
            FatherNameEntry.Text = string.Empty;
            GotraEntry.Text = string.Empty;
            DobPicker.Date = DateTime.Today;
            TimeEntry.Text = string.Empty;
            AmPmPicker.SelectedItem = "AM";
            BirthPlaceEntry.Text = string.Empty;
            CountryCodePicker.SelectedItem = DefaultCountry();
            PhoneEntry.Text = string.Empty;
            AddressEntry.Text = string.Empty;
            GrahanPicker.SelectedItem = _grahanOptions.FirstOrDefault(g => g.Name == "None");
        }

        static CountryCodeOption? DefaultCountry()
            => CountryCodes.All.FirstOrDefault(c => c.CountryName == "India");

        async void OnSubmitClicked(object sender, EventArgs e)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(NameEntry.Text))
                errors.Add("Name is required.");

            if (DobPicker.Date > DateTime.Today)
                errors.Add("Date of birth cannot be in the future.");

            if (string.IsNullOrWhiteSpace(TimeEntry.Text))
                errors.Add("Time of birth is required.");
            else if (!TimeRegex.IsMatch(TimeEntry.Text.Trim()))
                errors.Add("Time of birth must be a valid time like 10:30.");

            if (string.IsNullOrWhiteSpace(BirthPlaceEntry.Text))
                errors.Add("Birth place is required.");

            if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
            {
                errors.Add("Phone / WhatsApp number is required.");
            }
            else
            {
                var digits = PhoneEntry.Text.Trim();
                if (!digits.All(char.IsDigit))
                    errors.Add("Phone number can only contain digits.");
                else if (digits.Length < 6 || digits.Length > 15)
                    errors.Add("Phone number length looks invalid.");

                if (CountryCodePicker.SelectedItem == null)
                    errors.Add("Please select a country code for the phone number.");
            }

            if (errors.Count > 0)
            {
                await DisplayAlert("Please fix the following", string.Join("\n", errors), "OK");
                return;
            }

            var selectedGrahan = GrahanPicker.SelectedItem as GrahanMaster;
            var amPm = AmPmPicker.SelectedItem as string ?? "AM";
            var timeText = $"{TimeEntry.Text.Trim()} {amPm}";
            var selectedCountry = CountryCodePicker.SelectedItem as CountryCodeOption;

            var dto = new PersonDto
            {
                Id = _editingId,
                Name = NameEntry.Text.Trim(),
                FatherName = FatherNameEntry.Text,
                Gotra = GotraEntry.Text,
                DOB = DobPicker.Date,
                Time = timeText,
                BirthPlace = BirthPlaceEntry.Text.Trim(),
                CountryCode = selectedCountry?.DialCode ?? string.Empty,
                PhoneNo = PhoneEntry.Text,
                Address = AddressEntry.Text,
                Grahan = selectedGrahan?.Name ?? "None"
            };

            try
            {
                var savedId = await _personService.SaveAsync(dto);
                _editingId = savedId;
                dto.Id = savedId;

                var navParams = new Dictionary<string, object> { { "PersonDraft", dto }, { "Mode", "Person" } };
                await Shell.Current.GoToAsync("navgrah", navParams);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save person from PersonFormPage", ex);
                await DisplayAlert("Error", "Could not save this Kundli. Please try again.", "OK");
            }
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