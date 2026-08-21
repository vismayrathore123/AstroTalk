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
            set { if (int.TryParse(value, out var id)) _stagingId = id; }
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
            PhoneLabel.Text = string.IsNullOrWhiteSpace(_staging.PhoneNo)
                ? "Not provided"
                : $"{_staging.CountryCode} {_staging.PhoneNo}";

            SelectionsHost.Children.Clear();
            foreach (var selection in _staging.Selections)
            {
                var header = new Label
                {
                    Text = $"REMEDIES FOR {selection.NavgrahName?.ToUpperInvariant()}",
                    Style = (Style)Microsoft.Maui.Controls.Application.Current!.Resources["SubtitleLabel"]
                };
                var remediesLabel = new Label
                {
                    Text = string.Join(", ", selection.Remedies),
                    FontSize = 16
                };
                SelectionsHost.Children.Add(new VerticalStackLayout { Spacing = 2, Children = { header, remediesLabel } });
            }
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
                Grah = string.Join(", ", _staging.Selections.Select(s => s.NavgrahName))
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
                // All Grahs picked in this session are saved together here - no more
                // looping back through "add another Grah".
                foreach (var selection in _staging.Selections)
                {
                    await _userRemedyService.SaveSelectedRemediesAsync(
                        personId,
                        selection.NavgrahId,
                        selection.Remedies);
                }
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
                    whatsAppSent = await TrySendWhatsAppAsync(_staging);
                else
                    savedFilePath = await _pdfExportService.SaveRemedyReviewPdfToDownloadsAsync(_staging);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed generating/saving/sending the PDF", ex);
            }

            try
            {
                foreach (var selection in _staging.Selections)
                    await _userRemedyService.MarkWhatsAppStatusAsync(personId, selection.NavgrahId, whatsAppSent);
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

            // No more "Add another Grah?" prompt - every Grah selected on the previous
            // screen is already saved above, so we just go back to the list.
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
                    try { await Launcher.Default.OpenAsync(new Uri($"https://wa.me/{digitsOnly}")); }
                    catch (Exception ex) { _logger.LogWarning($"Could not open wa.me link for {digitsOnly}: {ex.Message}"); }
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