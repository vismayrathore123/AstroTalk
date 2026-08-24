using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;
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
        private readonly IPermanentRemedyRepository _permanentRemedyRepository;
        private readonly IAppLogger _logger;

        private int _stagingId;
        private UserRemedyStagingDto? _staging;

        // Each row on screen: which remedy, which Grah it came from, and the live
        // checkbox state. Permanent rows start checked; Yearly rows start unchecked.
        private class RemedyRow
        {
            public int NavgrahId;
            public string RemedyName = string.Empty;
            public bool WasAlreadyPermanent;
            public CheckBox CheckBoxControl = null!;
        }

        private readonly List<RemedyRow> _permanentRows = new();
        private readonly List<RemedyRow> _yearlyRows = new();

        public string StagingId
        {
            set { if (int.TryParse(value, out var id)) _stagingId = id; }
        }

        public PreviewPage(
            IUserRemedyStagingService stagingService,
            IPersonService personService,
            IUserRemedyService userRemedyService,
            IPdfExportService pdfExportService,
            IPermanentRemedyRepository permanentRemedyRepository,
            IAppLogger logger)
        {
            InitializeComponent();
            _stagingService = stagingService;
            _personService = personService;
            _userRemedyService = userRemedyService;
            _pdfExportService = pdfExportService;
            _permanentRemedyRepository = permanentRemedyRepository;
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
            DobLabel.Text = _staging.DOB.ToString("dd MMM yyyy");

            WhatsAppTargetLabel.Text = string.IsNullOrWhiteSpace(_staging.PhoneNo)
                ? string.Empty
                : $"WhatsApp will be sent to {_staging.CountryCode} {_staging.PhoneNo}";

            await BuildRemedySectionsAsync();
            BuildPrecautionsSection();
        }

        async Task BuildRemedySectionsAsync()
        {
            PermanentRemedyHost.Children.Clear();
            YearlyRemedyHost.Children.Clear();
            _permanentRows.Clear();
            _yearlyRows.Clear();

            if (_staging == null) return;

            foreach (var selection in _staging.Selections)
            {
                var permanentForThisGrah = await _permanentRemedyRepository
                    .GetByPersonAndNavgrahAsync(_staging.PersonId, selection.NavgrahId);
                var permanentNames = new HashSet<string>(
                    permanentForThisGrah.Select(p => p.RemedyName), StringComparer.OrdinalIgnoreCase);

                foreach (var remedyName in selection.Remedies)
                {
                    bool isPermanent = permanentNames.Contains(remedyName);

                    var row = new RemedyRow
                    {
                        NavgrahId = selection.NavgrahId,
                        RemedyName = remedyName,
                        WasAlreadyPermanent = isPermanent
                    };

                    var target = isPermanent ? PermanentRemedyHost : YearlyRemedyHost;
                    var list = isPermanent ? _permanentRows : _yearlyRows;

                    row.CheckBoxControl = AddRemedyRow(target, $"{remedyName}  ·  {selection.NavgrahName}", isPermanent);
                    list.Add(row);
                }
            }

            NoPermanentLabel.IsVisible = _permanentRows.Count == 0;
            NoYearlyLabel.IsVisible = _yearlyRows.Count == 0;
        }

        CheckBox AddRemedyRow(VerticalStackLayout host, string labelText, bool isChecked)
        {
            var checkBox = new CheckBox { IsChecked = isChecked };
            var label = new Label { Text = labelText, FontSize = 15, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(8, 0, 0, 0) };

            var row = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                Children = { checkBox, label }
            };
            Grid.SetColumn(label, 1);

            host.Children.Add(new Border
            {
                Style = (Style)Microsoft.Maui.Controls.Application.Current!.Resources["CardBorder"],
                Padding = 12,
                Content = row
            });

            return checkBox;
        }

        void BuildPrecautionsSection()
        {
            PrecautionsHost.Children.Clear();

            var precautions = _staging?.SelectedPrecautions ?? new List<string>();
            PrecautionsCard.IsVisible = precautions.Count > 0;

            foreach (var text in precautions)
            {
                PrecautionsHost.Children.Add(new Label { Text = $"• {text}", FontSize = 14 });
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
                Grah = string.Join(", ", _staging.Selections.Select(s => s.NavgrahName)),
                Precautions = string.Join(",", _staging.SelectedPrecautions)
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
                foreach (var selection in _staging.Selections)
                    await _userRemedyService.SaveSelectedRemediesAsync(personId, selection.NavgrahId, selection.Remedies);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed saving remedies for PersonId={personId}", ex);
                await DisplayAlert("Error", "Record saved, but the remedies could not be stored. Please retry from Edit.", "OK");
            }

            try
            {
                // Yearly rows the user just ticked become permanent.
                foreach (var row in _yearlyRows.Where(r => r.CheckBoxControl.IsChecked))
                    await _permanentRemedyRepository.AddAsync(personId, row.NavgrahId, row.RemedyName);

                // Permanent rows the user unticked stop being permanent.
                foreach (var row in _permanentRows.Where(r => !r.CheckBoxControl.IsChecked))
                    await _permanentRemedyRepository.RemoveAsync(personId, row.NavgrahId, row.RemedyName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed updating permanent remedies for PersonId={personId}", ex);
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