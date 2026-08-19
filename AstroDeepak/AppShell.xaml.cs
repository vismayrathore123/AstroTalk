using AstroDeepak.Views;

namespace AstroDeepak
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("search", typeof(SearchPage));
            Routing.RegisterRoute("form", typeof(PersonFormPage));
            Routing.RegisterRoute("navgrah", typeof(NavgrahListPage));
            Routing.RegisterRoute("remedies", typeof(RemedySelectionPage));
        }
    }
}