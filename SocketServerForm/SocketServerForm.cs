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

namespace SocketServerForm
{
    public partial class SocketServerForm : Form
    {
        static Dictionary<Socket, Thread> listeners;
        static Thread newUserListener;
        static object sharedTextLock = new object();
        static object sharedDictLock = new object();
        Socket listenerSocket;
        IPAddress ipaddr;
        IPEndPoint ipep;

        public SocketServerForm()
        {
            InitializeComponent();
            this.FormClosing += delegate { newUserListener.Abort(); };
            this.Show();

            listeners = new Dictionary<Socket, Thread> { };

            //Create socket object
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Create an IPAddress.socket listening on any ip address.
            ipaddr = IPAddress.Any;
            //Define IP end point
            ipep = new IPEndPoint(ipaddr, 25000);
            //Bind socket to ip end point
            try
            {
                listenerSocket.Bind(ipep);
                listenerSocket.Listen(5);
            }
            catch
            {

            }
            newUserListener = new Thread(() => listenUsers());
            newUserListener.Start();
        }

        public void listenUsers()
        {
            while (true)
            {
                try
                {
                    Socket client = listenerSocket.Accept();
                    AddChatText("Client connected: " + client.ToString() + " -Ip End Point: " + client.RemoteEndPoint.ToString());
                    Monitor.Enter(sharedDictLock);
                    try
                    {
                        listeners.Add(client, new Thread(() => listen(client, txtChat)));
                        this.FormClosing += delegate { listeners[client].Abort(); };
                        listeners[client].Start();
                    }
                    finally
                    {
                        Monitor.Exit(sharedDictLock);
                    }
                }
                catch
                {

                }
            }
        }

        public void listen(Socket client, TextBox chat)
        {
            byte[] buff = new byte[128];
            int numberOfReceivedBytes = 0;
            while (true)//!cancellationTokenSource.IsCancellationRequested)
            {
                numberOfReceivedBytes = client.Receive(buff);
                string[] receivedText = Encoding.ASCII.GetString(buff, 0, numberOfReceivedBytes).Split(':');
                //Console.WriteLine("Client Said: " + receivedText);



                if (receivedText[1] == "x")
                {
                    //cancellationTokenSource.Cancel();
                    break;
                }

                foreach (Socket C in listeners.Keys)
                {
                    C.Send(buff);
                }
                //client.Send(buff);

                Monitor.Enter(sharedTextLock);
                try
                {
                    AddChatText(receivedText[0] + ": " + receivedText[1]);
                }
                finally
                {
                    Monitor.Exit(sharedTextLock);
                }

                Array.Clear(buff, 0, buff.Length);
                numberOfReceivedBytes = 0;
            }
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
