using AndroidExtendedCommands.CSharp.DataTypeExtensions.RegularExpressions;
using AndroidExtendedCommands.CSharp.Web.Communication;
using System;
using System.Collections.Generic;
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
            clients = new Dictionary<int, TCPClient>();
            ClientDisplayers = new List<ClientDisplayer>()
            {
                new ClientDisplayer(){Ip = "Temp ip", Port = "Haha", Id= "1"}
            };
            BindingContext = this;
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            //var d = new Dialog(ClientPage.AppContext);
            //AlertDialog.Builder alert = new AlertDialog.Builder(ClientPage.AppContext);
            //alert.SetTitle("Select Action");
            //var items = new string[] { "Item 1", "Item 2", "Item #3" };
            //alert.SetSingleChoiceItems(items, -1, (senderAlert, args) => {

            //});
            //alert.SetPositiveButton("OK", (senderAlert, args) =>
            //{
            //    Device.BeginInvokeOnMainThread(() =>
            //    {
            //        smsg.Text = items[args.Which];
            //    });
            //});
            //alert.SetNegativeButton("Cancel", (senderAlert, args) =>
            //{

            //});
            //Dialog dialog = alert.Create();
            //dialog.Show();
            var s = AndroidExtendedCommands.Dialogs.ItemSelection.GetSingleSelect(ClientPage.AppContext, new string[] { "Item", "Item " }, "Select item");
            Device.BeginInvokeOnMainThread(() => smsg.Text = s);
            //ClientModeRequested?.Invoke(this, e);
        }
        TCPServer server;
        //Thread t;
        private void btncnt_Clicked(object sender, EventArgs e)
        {
            //t?.Abort();
            switch (btncnt.Text)
            {
                case "Start server":
                    if (server != null)
                    {
                        server.ClientConnected -= Server_ClientConnected;
                        server.ClientDisconnected -= Server_ClientDisconnected;
                    }
                    server = new TCPServer(ip.Text, ushort.Parse(port.Text));
                    server.ClientConnected += Server_ClientConnected;
                    server.ClientDisconnected += Server_ClientDisconnected;
                    server.ClientDataReceived += Server_ClientDataReceived;
                    btncnt.Text = "Stop server";
                    server.StartListening();
                    break;
                case "Stop server":
                    btncnt.Text = "Start server";
                    server.BroadcastString("Server shutdown");
                    try { server.Shutdown(); ClientDisplayers.Clear(); clientLister.ItemsSource = null; clientLister.ItemsSource = ClientDisplayers; } catch { }
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
            public override string ToString()
            {
                return $"Ip: {Ip}; Port: {Port}; Id: {Id}";
            }
            public ClientDisplayer() { }
            public string Id { get; set; }
            public string Ip { get; set; }
            public string Port { get; set; }
            public TCPClient Client { get; }
        }
        private void Server_ClientDataReceived(object sender, TCPServer.ClientDataArgs e)
        {

        }
        int GetClientId()
        {
            for (int i = 1; i <= 100; i++)
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
                    { clients.Remove(kv.Key); break; }
                foreach (var d in ClientDisplayers)
                    if (d.Client == e.Client)
                    { ClientDisplayers.Remove(d); break; }
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
        public object oldSel;
        private void clientLister_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => disconnectBtn.IsEnabled = e.SelectedItemIndex != -1);
        }

        private void disconnectBtn_Clicked(object sender, EventArgs e)
        {
            if (!disconnectBtn.IsEnabled)
                return;
            var id = int.Parse(clientLister.SelectedItem.ToString().GetRegexMatch(@"Id: ([\d]+)").Groups[1].Value);
            var client = clients[id];
            foreach (var d in ClientDisplayers)
                if (d.Client == client)
                { client.SendString("MANUAL DISCONNECT"); ClientDisplayers.Remove(d); clients.Remove(id); Device.BeginInvokeOnMainThread(() => { clientLister.ItemsSource = null; clientLister.ItemsSource = ClientDisplayers; disconnectBtn.IsEnabled = clientLister.SelectedItem != null; }); break; }
        }
    }
}