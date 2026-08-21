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
        private readonly IAppLogger _logger;

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
            IPdfExportService pdfExportService,
            IAppLogger logger)
        {
            InitializeComponent();
            _stagingService = stagingService;
            _personService = personService;
            _userRemedyService = userRemedyService;
            _pdfExportService = pdfExportService;
            _logger = logger;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _staging = await _stagingService.GetByIdAsync(_stagingId);
            if (_staging == null)
            {
                _logger.LogWarning($"PreviewPage opened with missing staging record. StagingId={_stagingId}");
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

            int personId;
            try
            {
                personId = await _personService.SaveAsync(personDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed saving person from PreviewPage", ex);
                await DisplayAlert("Error", "Could not save this record. Please try again.", "OK");
                return;
            }

            try
            {
                await _userRemedyService.SaveSelectedRemediesAsync(
                    personId,
                    _staging.NavgrahId,
                    _staging.SelectedRemedies);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed saving remedies for PersonId={personId}", ex);
                await DisplayAlert("Error", "Record saved, but the remedies could not be stored. Please retry from Edit.", "OK");
            }

            string? savedFilePath = null;
            bool whatsAppSent = false;

            try
            {
                if (sendWhatsApp)
                {
                    whatsAppSent = await TrySendWhatsAppAsync(_staging);
                }
                else
                {
                    // "Confirm" (no WhatsApp) -> straight into Downloads, no dialog.
                    savedFilePath = await _pdfExportService.SaveRemedyReviewPdfToDownloadsAsync(_staging);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed generating/saving/sending the PDF", ex);
            }

            try
            {
                await _userRemedyService.MarkWhatsAppStatusAsync(personId, _staging.NavgrahId, whatsAppSent);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed updating WhatsApp status for PersonId={personId}", ex);
            }

            await _stagingService.DeleteAsync(_staging.Id);

            var confirmationMessage = sendWhatsApp
                ? (whatsAppSent
                    ? "Kundli saved. WhatsApp was opened for this person's number with the PDF ready to attach - finish sending it from there."
                    : "Kundli saved, but WhatsApp could not be opened.")
                : (savedFilePath != null
                    ? $"Kundli saved. PDF saved to:\n{savedFilePath}"
                    : "Kundli saved, but the PDF could not be written to Downloads.");

            await DisplayAlert("Saved", confirmationMessage, "OK");

            var addAnother = await DisplayAlert(
                "Add another remedy?",
                "Do you want to add remedies for another Grah for this same person now?",
                "Add Another Grah",
                "Done");

            if (addAnother)
            {
                personDto.Id = personId;
                var navParams = new Dictionary<string, object> { { "PersonDraft", personDto }, { "Mode", "Person" } };
                await Shell.Current.GoToAsync("navgrah", navParams);
            }
            else
            {
                await Shell.Current.GoToAsync("//search");
            }
        }

        async Task<bool> TrySendWhatsAppAsync(UserRemedyStagingDto staging)
        {
            // Platform reality check: no public Android or iOS API lets an app open one
            // specific WhatsApp chat with a file already attached with zero taps. The
            // wa.me link below opens the correct conversation for this person's saved
            // number; the OS share sheet that follows is where the user picks WhatsApp
            // again to actually attach the generated PDF. That one extra tap can't be
            // removed without WhatsApp itself exposing a "send to number with file" API,
            // which it doesn't.
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
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Could not open wa.me link for {digitsOnly}: {ex.Message}");
                    }
                }

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Send Kundli Remedy PDF",
                    File = new ShareFile(pdfPath)
                });

                _logger.LogInfo($"WhatsApp flow completed for PersonId={staging.PersonId}, Number={staging.CountryCode}{staging.PhoneNo}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"WhatsApp send flow failed for PersonId={staging.PersonId}", ex);
                return false;
            }
        }
    }
}