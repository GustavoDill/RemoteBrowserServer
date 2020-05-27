using CSharpExtendedCommands.Web.Communication;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace RemoteBrowserServer
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Commands.console = console;
            Load += Form1_Load;
            notifyIcon1.Icon = Icon;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var ips = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in ips.AddressList)
                if (Regex.IsMatch(ip.ToString(), @"(192\.168|10\.0)\.0\.\d+"))
                    textBox1.Text = ip.ToString();
        }
        TCPServer server;
        private void button1_Click(object sender, EventArgs e)
        {
            switch (((Button)sender).Text)
            {
                case "Start":
                    Start();
                    button1.Text = "Stop";
                    break;
                case "Stop":
                    Shutdown();
                    button1.Text = "Start";
                    break;
            }
            Log(button1.Text != "Start" ? "Server started" : "Server stopped", button1.Text != "Start" ? Color.Green : Color.Red);
        }
        void Log(string msg)
        {
            Commands.Log(msg);
        }
        void Log(string msg, Color color, bool doNewLine = true)
        {
            Commands.Log(msg, color, doNewLine);
        }
        private void Server_ClientDisconnected(object sender, TCPServer.ClientConnectionArgs e)
        {

        }
        private void Server_ClientConnected(object sender, TCPServer.ClientConnectionArgs e)
        {
            Log("Client connection { Host: " + e.Client.Ip.ToString() + " Port: " + e.Client.Port.ToString() + " }", Color.Orange);
            var tmout = e.Client.ClientSocket.ReceiveTimeout;
            e.Client.ClientSocket.ReceiveTimeout = 5000;
            var connectCode = "NULL";
            try { connectCode = e.Client.ReceiveString(); } catch { }
            e.Client.ClientSocket.ReceiveTimeout = tmout;
            if (connectCode != "RemoteBrowser#CODE#")
            {
                SendPackage(e.Client, "Code rejected");
                server.DisconnectClient(e.Client);
                Log($"Client Code rejected; {{ Host: {e.Client.Ip} Port: {e.Client.Port}}}");
            }
            else
            {
                Log($"Client Code accepted; {{ Host: {e.Client.Ip} Port: {e.Client.Port}}}");
                monitorThreads.Add(new Thread(new ParameterizedThreadStart(MonitorPackages))); monitorThreads.Last().Start(e.Client);
            }
        }
        void SendPackage(TCPClient client, TcpPackage package)
        {
            try
            {
                client.SendPackage(package);
            }
            catch { Log($"Error sending package to client {{Host: {client.Ip} Port: {client.Port}}}"); client.Disconnect(); }
        }
        public void OnClientShutdown(TCPClient client, Thread monitorThread)
        {
            if (!closing)
                Log($"Client disconnected {{Host: {client.Ip} Port: {client.Port}}}", Color.DarkRed);
            monitorThread.Abort();
            server.DisconnectClient(client);
        }
        List<Thread> monitorThreads = new List<Thread>();
        void MonitorPackages(object tcpClient)
        {
            while (server.Running)
            {
                var client = (TCPClient)tcpClient;
                TcpPackage package;
                try
                {
                    package = client.ReceivePackage();
                    if (package == "CLIENT-SHUTDOWN")
                    {
                        OnClientShutdown(client, Thread.CurrentThread);
                    }
                    var data = package.ToString();
                    var m = Regex.Match(data, @"([\w\-\d]+)( ?\: ?""([\w\d\-\\/% \*\+\{\}\(\)\[\]\t\r\#$:'""\|@\.]+)"")?");
                    var cmd = m.Groups[1].Value.Replace("-", "");
                    var arg = m.Groups[3].Value;
                    Log($"Client request: {{Command: \"{cmd}\" Arg: \"{arg}\"}} from {{Host: {client.Ip} Port: {client.Port}}}");
                    if (string.IsNullOrEmpty(arg))
                        typeof(Commands).GetMethod(cmd).Invoke(null, new object[] { client });
                    else
                        typeof(Commands).GetMethod(cmd).Invoke(null, new object[] { client, arg });
                    Thread.Sleep(500);
                }
                catch { OnClientShutdown(client, Thread.CurrentThread); return; }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Opacity = 0;
            var args = Environment.GetCommandLineArgs();
            if (args.Contains("--hidden"))
            {
                Opacity = 0;
                Hide();
                notifyIcon1.Visible = true;
            }
            Opacity = 1;
            ShowInTaskbar = true;
            if (!args.Contains("--nostart"))
                Start();
            Size workSize = new Size(Size.Width, Size.Height);
            button1.Size = new Size((workSize.Width - 26 - 26) / 2 - 26, button1.Height);
            button2.Size = new Size(button1.Size.Width, button2.Height);
            button2.Location = new Point(button1.Location.X + button1.Size.Width + 18, button2.Location.Y);
            button2.Location = new Point(console.Size.Width + console.Location.X - button2.Size.Width, button2.Location.Y);
            UpdateNotifyContextMenu();
        }
        bool closing = false;
        void UpdateNotifyContextMenu()
        {
            startToolStripMenuItem.Enabled = server != null ? !server.Running : true;
            restartToolStripMenuItem.Enabled = server != null ? server.Running : false;
            stopToolStripMenuItem.Enabled = server != null ? server.Running : false;
            button1.Text = server != null ? (server.Running ? "Stop" : "Start") : "Stop";
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server != null ? server.Running : false)
            {
                closing = true;
                server?.Shutdown();
            }
        }

        private void Server_SizeChanged(object sender, EventArgs e)
        {
            Size workSize = new Size(Size.Width, Size.Height);
            button1.Size = new Size((workSize.Width - 26 - 26) / 2 - 26, button1.Height);
            button2.Size = new Size(button1.Size.Width, button2.Height);
            button2.Location = new Point(button1.Location.X + button1.Size.Width + 18, button2.Location.Y);
            button2.Location = new Point(console.Size.Width + console.Location.X - button2.Size.Width, button2.Location.Y);
        }
        private void button2_Click_1(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;
            Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            console.Clear();
        }

        private void stopExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopExit();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shutdown();
        }
        void Restart()
        {
            Log("Server restarting...", Color.Blue, false);
            server?.Shutdown();
            for (int i = 0; i < monitorThreads.Count; i++)
                monitorThreads[i].Abort();
            monitorThreads.Clear();
            server = new TCPServer(textBox1.Text, ushort.Parse(textBox2.Text));
            server.AutoRelistenForMessages = false;
            server.BeginReceiveOnConnection = false;
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.Start();
            console.Log("\tDone", Color.Green);
            UpdateNotifyContextMenu();
        }
        void Shutdown()
        {
            if (server.Running)
            {
                server?.Shutdown();
                for (int i = 0; i < monitorThreads.Count; i++)
                    monitorThreads[i].Abort();
                monitorThreads.Clear();
            }
            UpdateNotifyContextMenu();
        }
        void Start()
        {
            server = new TCPServer(textBox1.Text, ushort.Parse(textBox2.Text));
            server.AutoRelistenForMessages = false;
            server.BeginReceiveOnConnection = false;
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.Start();
            UpdateNotifyContextMenu();
        }
        void StopExit()
        {
            Shutdown();
            Application.Exit();
        }
        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Restart();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                notifyIcon1.Visible = false;
                Show();
                BringToFront();
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void eXitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopExit();
        }
    }
}
