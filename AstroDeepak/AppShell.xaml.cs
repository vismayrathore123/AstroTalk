using AstroDeepak.Views;

namespace AstroDeepak
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // "search" and "form" are declared directly as tabs above.
            // Only push-on-top pages need explicit route registration.
            Routing.RegisterRoute("navgrah", typeof(NavgrahListPage));
            Routing.RegisterRoute("remedies", typeof(RemedySelectionPage));
            Routing.RegisterRoute("masterremedies", typeof(MasterRemedyPage)); // NEW

        }
    }
}