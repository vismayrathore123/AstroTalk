using AstroDeepak.Views;

namespace AstroDeepak
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // These pages are not tabs/flyout items, they're pushed on top of
            // "main" when we call Shell.Current.GoToAsync("search") etc.
            Routing.RegisterRoute("search", typeof(SearchPage));
            Routing.RegisterRoute("form", typeof(PersonFormPage));
            Routing.RegisterRoute("navgrah", typeof(NavgrahListPage));
        }
    }
}
