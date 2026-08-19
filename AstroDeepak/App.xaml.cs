using Microsoft.Extensions.DependencyInjection;

namespace AstroDeepak
{
    public partial class App : IApplication
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var personPage = _serviceProvider.GetRequiredService<PersonPage>(); 
            return new Window ( new NavigationPage (personPage) );
        }
    }
}