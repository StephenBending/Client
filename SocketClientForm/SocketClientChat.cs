using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketClientForm
{
    public partial class SocketClientChat : Form
    {
        static object sharedTextLock = new object();
        static Thread listenThread;
        static Socket client;
        static string username;

        public void listen(Socket client, TextBox chat)
        {
            byte[] buff = new byte[128];
            int numberOfReceivedBytes = 0;
            while (true)
            {
                numberOfReceivedBytes = client.Receive(buff);

                Monitor.Enter(sharedTextLock);
                try
                {
                    AddChatText(Encoding.ASCII.GetString(buff, 0, numberOfReceivedBytes) + "\n");
                }
                finally
                {
                    Monitor.Exit(sharedTextLock);
                }
                //Console.WriteLine("Server Said: " + Encoding.ASCII.GetString(buff, 0, numberOfReceivedBytes));

                Array.Clear(buff, 0, buff.Length);
                numberOfReceivedBytes = 0;
            }
        }

        public SocketClientChat(string IPAddr, string PortAddr, string un)
        {
            InitializeComponent();
            username = un;
            this.FormClosing += delegate { listenThread.Abort(); };

            client = null;

            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipaddr = null;
            int nPortInput = 0;

            if (IPAddr == " ") IPAddr = "127.0.0.1";
            if (PortAddr == " ") PortAddr = "25000";

            IPAddress.TryParse(IPAddr, out ipaddr);
            int.TryParse(PortAddr.Trim(), out nPortInput);

            client.Connect(ipaddr, nPortInput);
            listenThread = new Thread(() => listen(client, txtChat));
            listenThread.Start();
        }

        public void AddChatText(String text)
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate () { AddChatText(text); });
                return;
            }
            txtChat.AppendText(Environment.NewLine);
            txtChat.AppendText(text);
        }

        private void SocketClientChat_Load(object sender, EventArgs e)
        {
            
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string inputCommand = username + ":" + txtInput.Text;
            byte[] buffSend = Encoding.ASCII.GetBytes(inputCommand);
            client.Send(buffSend);
            txtInput.Text = "";
        }
    }
}
