
using AndroidExtendedCommands.CSharp.Web.Communication;
using System;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Android_TPC_Connector
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerPage : ContentPage
    {
        public event EventHandler ClientModeRequested;
        public ServerPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            ClientModeRequested?.Invoke(this, e);
        }
        TCPServer server;
        Thread t;
        private void btncnt_Clicked(object sender, EventArgs e)
        {
            t?.Abort();
            switch (btncnt.Text)
            {
                case "Start server":
                    if (server != null)
                    {
                        server.ClientConnected -= Server_ClientConnected;
                        server.ClientDisconnected -= Server_ClientDisconnected;
                    }
                    server = new TCPServer(ip.Text, ushort.Parse(port.Text));
                    server.AutoRelistenForMessages = false;
                    server.BeginReceiveOnConnection = false;
                    server.ClientConnected += Server_ClientConnected;
                    server.ClientDisconnected += Server_ClientDisconnected;
                    btncnt.Text = "Stop server";
                    smsg.Text = "";
                    try
                    {
                        server.StartListening();

                    }
                    catch { smsg.Text = "Connection Refused!"; btncnt.Text = "Connect"; }
                    break;
                case "Stop server":
                    btncnt.Text = "Start server";
                    t.Abort();
                    server.BroadcastString("Server shutdown");
                    server.Shutdown();
                    break;
            }
        }

        private void Server_ClientDisconnected(object sender, TCPServer.ClientConnectionArgs e)
        {

        }

        private void Server_ClientConnected(object sender, TCPServer.ClientConnectionArgs e)
        {
            Device.BeginInvokeOnMainThread(delegate ()
            {
                var list = clientLister.ItemsSource as List<string>;
                list.Add("World");
                clientLister.ItemsSource = list;
            });
        }
    }
}