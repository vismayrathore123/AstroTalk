using AstroDeepak.Views;

namespace AstroDeepak
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("navgrah", typeof(NavgrahListPage));
            Routing.RegisterRoute("preview", typeof(PreviewPage));
            Routing.RegisterRoute("masterremedies", typeof(GrahRemedyPage));
            // "remedies" route removed - RemedySelectionPage no longer exists;
            // remedy selection is now inline on NavgrahListPage.
        }
    }
}