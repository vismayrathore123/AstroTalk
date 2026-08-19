using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;

namespace AstroDeepak
{
    public partial class PersonPage : ContentPage
    {
        private readonly IPersonService _personService;
        private int _editingId = 0;

        public PersonPage(IPersonService personService)
        {
            InitializeComponent();
            _personService = personService;
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            var dto = new PersonDto
            {
                Id = _editingId,
                Name = NameEntry.Text,
                FatherName = FatherNameEntry.Text,
                Gotra = GotraEntry.Text,
                DOB = DobPicker.Date ?? DateTime.Today,
                Time = TimeEntry.Text,
                BirthPlace = BirthPlaceEntry.Text,
                PhoneNo = PhoneEntry.Text,
                Address = AddressEntry.Text,
                GrahanType = GrahanPicker.SelectedItem?.ToString()
            };

            await _personService.SaveAsync(dto);
            await DisplayAlert("Success", "Record saved!", "OK");
            ClearForm();
            await LoadRecords();
        }

        async void OnViewAllClicked(object sender, EventArgs e) => await LoadRecords();

        async Task LoadRecords()
        {
            RecordsList.ItemsSource = await _personService.GetAllAsync();
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            var id = (int)((Button)sender).CommandParameter;
            var dto = await _personService.GetByIdAsync(id);
            if (dto == null) return;

            _editingId = dto.Id;
            NameEntry.Text = dto.Name;
            FatherNameEntry.Text = dto.FatherName;
            GotraEntry.Text = dto.Gotra;
            DobPicker.Date = dto.DOB;
            TimeEntry.Text = dto.Time;
            BirthPlaceEntry.Text = dto.BirthPlace;
            PhoneEntry.Text = dto.PhoneNo;
            AddressEntry.Text = dto.Address;
            GrahanPicker.SelectedItem = dto.GrahanType;
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            var id = (int)((Button)sender).CommandParameter;
            bool confirm = await DisplayAlert("Confirm", "Delete this record?", "Yes", "No");
            if (confirm)
            {
                await _personService.DeleteAsync(id);
                await LoadRecords();
            }
        }

        void ClearForm()
        {
            _editingId = 0;
            NameEntry.Text = FatherNameEntry.Text = GotraEntry.Text = TimeEntry.Text =
                BirthPlaceEntry.Text = PhoneEntry.Text = AddressEntry.Text = string.Empty;
            GrahanPicker.SelectedItem = null;
            DobPicker.Date = DateTime.Today;
        }
    }
}