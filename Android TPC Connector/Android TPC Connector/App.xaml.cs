using Xamarin.Forms;

namespace Android_TPC_Connector
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            serverPage = new ServerPage();
            clientPage = new ClientPage();
            serverPage.TranslationX = 400;
            clientPage.Application = this;
            serverPage.ClientModeRequested += ServerPage_ClientModeRequested;
            clientPage.ServerModeRequested += ClientPage_ServerModeRequested;
        }

        private void ServerPage_ClientModeRequested(object sender, System.EventArgs e)
        {
            SwitchToClient();
        }

        private void ClientPage_ServerModeRequested(object sender, System.EventArgs e)
        {
            SwitchToServer();
        }

        public ServerPage serverPage;
        public ClientPage clientPage;
        public void SwitchToServer()
        {
            clientPage.TranslateTo(((int)clientPage.TranslationX) - 400, ((int)clientPage.TranslationY), 500, Easing.CubicOut);
            MainPage = serverPage;
            serverPage.TranslateTo(0, 0, 500, Easing.CubicIn);
        }
        public void SwitchToClient()
        {
            serverPage.TranslateTo(((int)serverPage.TranslationX) + 400, ((int)serverPage.TranslationY), 500, Easing.CubicOut);
            MainPage = clientPage;
            clientPage.TranslateTo(0, 0, 500, Easing.CubicIn);
        }
        protected override void OnStart()
        {
            MainPage = clientPage;
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
