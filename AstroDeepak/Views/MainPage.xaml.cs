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
    }
}
