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
        private int _editingId = 0;
        private List<GrahanMaster> _grahanOptions = new();

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

        public PersonFormPage(IPersonService personService, IMasterDataRepository masterDataRepository)
        {
            InitializeComponent();
            _personService = personService;
            _masterDataRepository = masterDataRepository;
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
            }

            ClearForm();
        }

        void Populate(PersonDto dto)
        {
            NameEntry.Text = dto.Name;
            FatherNameEntry.Text = dto.FatherName;
            GotraEntry.Text = dto.Gotra;
            DobPicker.Date = dto.DOB ?? DateTime.Today;
            TimeEntry.Text = dto.Time;
            BirthPlaceEntry.Text = dto.BirthPlace;
            PhoneEntry.Text = dto.PhoneNo;
            AddressEntry.Text = dto.Address;

            var match = _grahanOptions.FirstOrDefault(g => g.Name == dto.SelectedGrahan);
            GrahanPicker.SelectedItem = match;
        }

        void ClearForm()
        {
            NameEntry.Text = string.Empty;
            FatherNameEntry.Text = string.Empty;
            GotraEntry.Text = string.Empty;
            DobPicker.Date = DateTime.Today;
            TimeEntry.Text = string.Empty;
            BirthPlaceEntry.Text = string.Empty;
            PhoneEntry.Text = string.Empty;
            AddressEntry.Text = string.Empty;
            GrahanPicker.SelectedItem = _grahanOptions.FirstOrDefault(g => g.Name == "None");
        }

        async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("Missing name", "Please enter the person's name.", "OK");
                return;
            }

            var selectedGrahan = GrahanPicker.SelectedItem as GrahanMaster;

            var dto = new PersonDto
            {
                Id = _editingId,
                Name = NameEntry.Text,
                FatherName = FatherNameEntry.Text,
                Gotra = GotraEntry.Text,
                DOB = DobPicker.Date,
                Time = TimeEntry.Text,
                BirthPlace = BirthPlaceEntry.Text,
                PhoneNo = PhoneEntry.Text,
                Address = AddressEntry.Text,
                SelectedGrahan = selectedGrahan?.Name ?? "None",
                CreatedAt = DateTime.Now
            };

            var navParams = new Dictionary<string, object> { { "PersonDraft", dto } };
            await Shell.Current.GoToAsync("navgrah", navParams);
        }

        async void OnHamburgerClicked(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Menu", "Cancel", null, "Add Remedies");

            if (action == "Add Remedies")
                await Shell.Current.GoToAsync("navgrah?AdminMode=true");
        }
    }
}