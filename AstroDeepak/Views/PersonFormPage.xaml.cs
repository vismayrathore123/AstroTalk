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

        // 0 = brand-new person that hasn't been saved yet.
        // Set to a real value the moment Submit succeeds, so that if the user goes
        // "back" from the Grah-selection page, OnAppearing reloads the saved row
        // instead of wiping the form via ClearForm().
        private int _editingId = 0;

        private List<GrahanMaster> _grahanOptions = new();
        private static readonly List<string> AmPmOptions = new() { "AM", "PM" };

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

                // The id we were remembering no longer exists in the DB (e.g. it was
                // deleted elsewhere) - fall back to a clean form instead of crashing.
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

            // Match on stored dial code; default to India if nothing was ever saved
            // (covers both brand-new drafts and older records saved before CountryCode
            // was tracked).
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
            CountryCodePicker.SelectedItem = DefaultCountry(); // India by default
            PhoneEntry.Text = string.Empty;
            AddressEntry.Text = string.Empty;
            GrahanPicker.SelectedItem = _grahanOptions.FirstOrDefault(g => g.Name == "None");
        }

        static CountryCodeOption? DefaultCountry()
            => CountryCodes.All.FirstOrDefault(c => c.CountryName == "India");

        async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("Missing name", "Please enter the person's name.", "OK");
                return;
            }

            if (!string.IsNullOrWhiteSpace(PhoneEntry.Text) && CountryCodePicker.SelectedItem == null)
            {
                await DisplayAlert("Missing country code", "Please select a country code for the phone number.", "OK");
                return;
            }

            var selectedGrahan = GrahanPicker.SelectedItem as GrahanMaster;
            var amPm = AmPmPicker.SelectedItem as string ?? "AM";
            var timeText = string.IsNullOrWhiteSpace(TimeEntry.Text) ? string.Empty : $"{TimeEntry.Text.Trim()} {amPm}";
            var selectedCountry = CountryCodePicker.SelectedItem as CountryCodeOption;

            var dto = new PersonDto
            {
                Id = _editingId,
                Name = NameEntry.Text,
                FatherName = FatherNameEntry.Text,
                Gotra = GotraEntry.Text,
                DOB = DobPicker.Date,
                Time = timeText,
                BirthPlace = BirthPlaceEntry.Text,
                CountryCode = selectedCountry?.DialCode ?? string.Empty,
                PhoneNo = PhoneEntry.Text,
                Address = AddressEntry.Text,
                Grahan = selectedGrahan?.Name ?? "None"
            };

            try
            {
                // Persist immediately (insert if new, update if editing). This is the
                // "correct table" for this data - the existing Persons table - there is
                // no need for a separate draft table. Saving now means: (1) the record
                // exists in the DB straight away, and (2) if the user backs out of the
                // Grah-selection screen, OnAppearing above reloads real saved data
                // instead of clearing the form.
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

        async void OnHamburgerClicked(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Menu", "Cancel", null, "Add Remedies", "Contact Us");

            if (action == "Add Remedies")
            {
                var navParams = new Dictionary<string, object> { { "Mode", "Master" } };
                await Shell.Current.GoToAsync("navgrah", navParams);
            }
            else if (action == "Contact Us")
            {
                await DisplayAlert("Contact Us", "Contact us feature coming soon.", "OK");
            }
        }
    }
}