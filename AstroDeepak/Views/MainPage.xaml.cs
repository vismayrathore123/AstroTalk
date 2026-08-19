namespace AstroDeepak.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void OnNewKundliTapped(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("search");

        async void OnOpenTapped(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("form");

        async void OnHamburgerClicked(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Menu", "Cancel", null, "Add Remedies");

            if (action == "Add Remedies")
                await Shell.Current.GoToAsync("navgrah?AdminMode=true");
        }
    }
}