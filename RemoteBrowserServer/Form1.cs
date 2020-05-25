using CSharpExtendedCommands.Web.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace RemoteBrowserServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var ips = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in ips.AddressList)
                if (Regex.IsMatch(ip.ToString(), @"(192\.168|10\.0)\.\d+\.\d+"))
                    textBox1.Text = ip.ToString();
        }
        TCPServer server;
        private void button1_Click(object sender, EventArgs e)
        {
            switch (((Button)sender).Text)
            {
                case "Start":
                    server = new TCPServer(textBox1.Text, ushort.Parse(textBox2.Text));
                    server.AutoRelistenForMessages = false;
                    server.BeginReceiveOnConnection = false;
                    server.ClientConnected += Server_ClientConnected;
                    server.ClientDisconnected += Server_ClientDisconnected;
                    server.Start();
                    button1.Text = "Stop";
                    break;
                case "Stop":
                    server?.Shutdown();
                    button1.Text = "Start";
                    break;
            }
        }

        private void Server_ClientDisconnected(object sender, TCPServer.ClientConnectionArgs e)
        {

        }
        private void Server_ClientConnected(object sender, TCPServer.ClientConnectionArgs e)
        {
            var tmout = e.Client.ClientSocket.ReceiveTimeout;
            e.Client.ClientSocket.ReceiveTimeout = 3000;
            var connectCode = "NULL";
            try { connectCode = e.Client.ReceiveString(); } catch { }
            e.Client.ClientSocket.ReceiveTimeout = tmout;
            if (connectCode != "RemoteBrowser#CODE#")
                server.DisconnectClient(e.Client, "Code rejected");
            else
            { monitorThreads.Add(new Thread(new ParameterizedThreadStart(MonitorPackages))); monitorThreads.Last().Start(e.Client); }
        }
        List<Thread> monitorThreads = new List<Thread>();
        void MonitorPackages(object tcpClient)
        {
            while (server.Running)
            {
                var client = (TCPClient)tcpClient;
                TcpPackage package;
                try { package = client.ReceivePackage(); } catch { Application.Restart(); return; }
                var data = package.ToString();
                var m = Regex.Match(data, @"([\w\-\d]+)( ?\: ?""([\w\d\-\\/% \*\+\{\}\(\)\[\]\t\r:'""\|@\.]+)"")?");
                var cmd = m.Groups[1].Value.Replace("-", "");
                var arg = m.Groups[3].Value;
                if (string.IsNullOrEmpty(arg))
                {
                    typeof(Commands).GetMethod(cmd).Invoke(null, new object[] { client });
                }
                else
                {
                    typeof(Commands).GetMethod(cmd).Invoke(null, new object[] { client, arg });
                }
                Thread.Sleep(500);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            button1.PerformClick();
        }
    }
}
