using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace AstroDeepak.Views
{
    [QueryProperty(nameof(StagingId), "StagingId")]
    public partial class PreviewPage : ContentPage
    {
        private readonly IUserRemedyStagingService _stagingService;
        private readonly IPersonService _personService;
        private readonly IUserRemedyService _userRemedyService;
        private readonly IPdfExportService _pdfExportService;

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

        public PreviewPage(
            IUserRemedyStagingService stagingService,
            IPersonService personService,
            IUserRemedyService userRemedyService,
            IPdfExportService pdfExportService)
        {
            InitializeComponent();
            _stagingService = stagingService;
            _personService = personService;
            _userRemedyService = userRemedyService;
            _pdfExportService = pdfExportService;
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
            PhoneLabel.Text = string.IsNullOrWhiteSpace(_staging.PhoneNo)
                ? "Not provided"
                : $"{_staging.CountryCode} {_staging.PhoneNo}";
        }

        async void OnEditClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");

        async void OnConfirmClicked(object sender, EventArgs e)
            => await SaveAsync(sendWhatsApp: false);

        async void OnConfirmAndWhatsAppClicked(object sender, EventArgs e)
        {
            if (_staging != null && string.IsNullOrWhiteSpace(_staging.PhoneNo))
            {
                await DisplayAlert("No phone number", "This person doesn't have a WhatsApp number on file. Saving without sending.", "OK");
                await SaveAsync(sendWhatsApp: false);
                return;
            }

            await SaveAsync(sendWhatsApp: true);
        }

        async Task SaveAsync(bool sendWhatsApp)
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
                CountryCode = _staging.CountryCode,
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

            bool whatsAppSent = false;
            if (sendWhatsApp)
                whatsAppSent = await TrySendWhatsAppAsync(_staging);

            await _userRemedyService.MarkWhatsAppStatusAsync(personId, _staging.NavgrahId, whatsAppSent);

            await _stagingService.DeleteAsync(_staging.Id);

            await DisplayAlert("Saved", whatsAppSent
                ? "Kundli saved. WhatsApp share sheet was opened with the PDF - complete the send from there."
                : "Kundli saved successfully.", "OK");

            await Shell.Current.GoToAsync("//search");
        }

        async Task<bool> TrySendWhatsAppAsync(UserRemedyStagingDto staging)
        {
            try
            {
                var pdfPath = await _pdfExportService.GenerateRemedyReviewPdfAsync(staging);

                var digitsOnly = new string((staging.CountryCode + staging.PhoneNo).Where(char.IsDigit).ToArray());
                if (!string.IsNullOrWhiteSpace(digitsOnly))
                {
                    try
                    {
                        await Launcher.Default.OpenAsync(new Uri($"https://wa.me/{digitsOnly}"));
                    }
                    catch
                    {
                        // WhatsApp may not be installed - the share sheet below still lets the user pick another app.
                    }
                }

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Send Kundli Remedy PDF",
                    File = new ShareFile(pdfPath)
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}