using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;

namespace AstroDeepak.Views
{
    [QueryProperty(nameof(StagingId), "StagingId")]
    public partial class PreviewPage : ContentPage
    {
        private readonly IUserRemedyStagingService _stagingService;
        private readonly IPersonService _personService;
        private readonly IUserRemedyService _userRemedyService;

        private int _stagingId;
        private UserRemedyStagingDto? _staging;

        public string StagingId
        {
            set
            {
                if (int.TryParse(value, out var id))
                    _stagingId = id;
            }
        }

        public PreviewPage(IUserRemedyStagingService stagingService, IPersonService personService, IUserRemedyService userRemedyService)
        {
            InitializeComponent();
            _stagingService = stagingService;
            _personService = personService;
            _userRemedyService = userRemedyService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _staging = await _stagingService.GetByIdAsync(_stagingId);
            if (_staging == null)
            {
                await DisplayAlert("Not found", "This draft is no longer available.", "OK");
                await Shell.Current.GoToAsync("//search");
                return;
            }

            NameLabel.Text = _staging.Name;
            FatherNameLabel.Text = string.IsNullOrWhiteSpace(_staging.FatherName) ? "-" : _staging.FatherName;
            DobLabel.Text = _staging.DOB.ToString("dd MMM yyyy");
            GrahHeaderLabel.Text = $"REMEDIES FOR {_staging.NavgrahName?.ToUpperInvariant()}";
            RemediesLabel.Text = string.Join(", ", _staging.SelectedRemedies);
        }

        async void OnEditClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");

        async void OnConfirmClicked(object sender, EventArgs e)
        {
            if (_staging == null) return;

            var personDto = new PersonDto
            {
                Id = _staging.PersonId,
                Name = _staging.Name,
                FatherName = _staging.FatherName,
                Gotra = _staging.Gotra,
                DOB = _staging.DOB,
                Time = _staging.Time,
                BirthPlace = _staging.BirthPlace,
                PhoneNo = _staging.PhoneNo,
                Address = _staging.Address,
                Grahan = _staging.Grahan,
                Grah = _staging.NavgrahName
            };

            var personId = await _personService.SaveAsync(personDto);

            await _userRemedyService.SaveSelectedRemediesAsync(
                personId,
                _staging.NavgrahId,
                _staging.SelectedRemedies);

            await _stagingService.DeleteAsync(_staging.Id);

            await DisplayAlert("Saved", "Kundli saved successfully.", "OK");
            await Shell.Current.GoToAsync("//search");
        }
    }
}