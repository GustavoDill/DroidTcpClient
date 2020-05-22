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
            ClientDisplayers = new List<ClientDisplayer>();
            BindingContext = this;
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
                    //server.AutoRelistenForMessages = false;
                    //server.BeginReceiveOnConnection = false;
                    server.ClientConnected += Server_ClientConnected;
                    server.ClientDisconnected += Server_ClientDisconnected;
                    server.ClientDataReceived += Server_ClientDataReceived;
                    btncnt.Text = "Stop server";
                        server.StartListening();
                    break;
                case "Stop server":
                    btncnt.Text = "Start server";
                    t?.Abort();
                    server.BroadcastString("Server shutdown");
                    server.Shutdown();
                    break;
            }
        }
        public Dictionary<int, TCPClient> clients;
        public List<ClientDisplayer> ClientDisplayers { get; private set; }
        public class ClientDisplayer
        {
            public ClientDisplayer(TCPClient client, int id)
            {
                Client = client;
                Id = id.ToString();
                Ip = client.Ip.ToString();
                Port = client.Port.ToString();
            }
            public string Id { get; set; }
            public string Ip { get; set; }
            public string Port { get; set; }
            public TCPClient Client { get; }
        }
        private void Server_ClientDataReceived(object sender, TCPServer.ClientDataArgs e)
        {
            var data = e.Data.ToString();
        }
        int GetClientId()
        {
            for (int i = 1; i < clients.Count + 1; i++)
                if (!clients.ContainsKey(i))
                    return i;
            return -1;
        }
        private void Server_ClientDisconnected(object sender, TCPServer.ClientConnectionArgs e)
        {
            Device.BeginInvokeOnMainThread(delegate ()
            {
                foreach (var kv in clients)
                    if (kv.Value == e.Client)
                        clients.Remove(kv.Key);
                foreach (var d in ClientDisplayers)
                    if (d.Client == e.Client)
                        ClientDisplayers.Remove(d);
                Device.BeginInvokeOnMainThread(delegate ()
                {
                clientLister.ItemsSource = null;
                clientLister.ItemsSource = ClientDisplayers;
                });
            });
        }

        private void Server_ClientConnected(object sender, TCPServer.ClientConnectionArgs e)
        {
                var id = GetClientId();
                clients.Add(id, e.Client);
                e.Client.SendString($"AssignID: \"{id}\" WelcomeMsg: \"Welcome! Your Id: {id}");
                ClientDisplayers.Add(new ClientDisplayer(e.Client, id));
                Device.BeginInvokeOnMainThread(delegate ()
                {
                    clientLister.ItemsSource = null;
                    clientLister.ItemsSource = ClientDisplayers;
                });
        }

        private void broadcastBtn_Clicked(object sender, EventArgs e)
        {
            server.BroadcastString(smsg.Text);
        }
    }
}