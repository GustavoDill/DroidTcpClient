using Android.App;
using AndroidExtendedCommands.CSharp.Web.Communication;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using Xamarin.Forms;

namespace Android_TPC_Connector
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public static Activity AppContext;
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            Guid = AndroidExtendedCommands.CSharp.Generate.GUID();
        }
        TCPClient client;
        Thread t;
        string Guid = "";
        string id = "";
        private void btncnt_Clicked(object sender, EventArgs e)
        {
            t?.Abort();
            switch (btncnt.Text)
            {
                case "Connect":
                    client = new TCPClient(ip.Text, ushort.Parse(port.Text));
                    btncnt.Text = "Disconnect";
                    smsg.Text = "";
                    try
                    {
                        try { client.Connect(); } catch (Exception ex) { smsg.Text = ex.Message; }
                        Thread.Sleep(500);
                        if (client.Connected)
                        {
                            t = new Thread(new ThreadStart(ListenToServer));
                            t.Start();
                        }
                    }
                    catch { smsg.Text = "Connection Refused!"; btncnt.Text = "Connect"; }
                    break;
                case "Disconnect":
                    btncnt.Text = "Connect";
                    t.Abort();
                    client.SendString("ClientDisconnected-ID:" + id);
                    client.Disconnect();
                    break;
            }
        }
        void ListenToServer()
        {
            while (t.IsAlive && client.Connected)
            {
                do
                {
                    if (!client.Connected)
                        break;
                    var cnt = client.ReceiveString();
                    if (Regex.IsMatch(cnt, "AssignID: \"(\\d+)\""))
                    {
                        var id = Regex.Match(cnt, "AssignID: \"(\\d+)\"").Groups[1].Value;
                        this.id = id;
                        var welcomeMsg = cnt.Substring(("AssignID: \"" + id + "\" WelcomeMsg: \"").Length);
                        welcomeMsg.Substring(0, welcomeMsg.Length - 1);
                        Device.BeginInvokeOnMainThread(delegate ()
                        {
                            idlbl.Text = "Id: " + id;
                            smsg.Text = welcomeMsg.Replace("\\n", "\n").Replace("\\t", "\t");
                        });
                        break;
                    }
                    switch (cnt)
                    {
                        case "server-guid-request":
                            client.SendString(Guid);
                            break;
                        default:
                            Device.BeginInvokeOnMainThread(delegate ()
                            {
                                smsg.Text = cnt;
                            });
                            break;
                    }
                } while (false);
            }
        }
    }
}
