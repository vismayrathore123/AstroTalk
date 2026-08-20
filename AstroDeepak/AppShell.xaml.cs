using AstroDeepak.Views;

namespace AstroDeepak
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("navgrah", typeof(NavgrahListPage));
            Routing.RegisterRoute("remedies", typeof(RemedySelectionPage));
            Routing.RegisterRoute("preview", typeof(PreviewPage));       // NEW
            Routing.RegisterRoute("masterremedies", typeof(GrahRemedyPage));
        }
    }
}