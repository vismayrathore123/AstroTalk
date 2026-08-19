using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;

namespace AstroDeepak.Views
{
    // IQueryAttributable lets Shell hand this page an "Id" when we navigate
    // to "form?Id=5" from the search/recent list (edit mode).
    [QueryProperty(nameof(Id), "Id")]
    public partial class PersonFormPage : ContentPage
    {
        private readonly IPersonService _personService;
        private int _editingId = 0;

        public string Id
        {
            set
            {
                if (int.TryParse(value, out var id) && id > 0)
                    _editingId = id;
            }
        }

        public PersonFormPage(IPersonService personService)
        {
            InitializeComponent();
            _personService = personService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_editingId > 0)
            {
                var dto = await _personService.GetByIdAsync(_editingId);
                if (dto != null) Populate(dto);
            }
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
        }

        async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("Missing name", "Please enter the person's name.", "OK");
                return;
            }

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
                CreatedAt = DateTime.Now
            };

            // Hand the not-yet-saved draft over to the Navgrah picker page.
            // Shell lets you pass real objects (not just strings) this way.
            var navParams = new Dictionary<string, object> { { "PersonDraft", dto } };
            await Shell.Current.GoToAsync("navgrah", navParams);
        }
    }
}
