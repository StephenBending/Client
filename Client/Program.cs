using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Program
    {

        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        static void listen(Socket client)
        {
            byte[] buff = new byte[128];
            int numberOfReceivedBytes = 0;
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                numberOfReceivedBytes = client.Receive(buff);
                Console.WriteLine("Server Said: " + Encoding.ASCII.GetString(buff, 0, numberOfReceivedBytes));

                Array.Clear(buff, 0, buff.Length);
                numberOfReceivedBytes = 0;
            }
        }

        static void Main(string[] args)
        {
            Socket client = null;

            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipaddr = null;

            try
            {
                Console.WriteLine("*** Welcome to Socket Client Starter ***");
                Console.WriteLine("Please Type a Valid Server IP Address and Press Enter: ");

                string strIPAddress = Console.ReadLine();
                Console.WriteLine("Please Supply a Valid Port Number 0 - 65525 and Press Enter: ");
                string strPortInput = Console.ReadLine();
                int nPortInput = 0;

                if (strIPAddress == " ") strIPAddress = "127.0.0.1";
                if (strPortInput == " ") strPortInput = "25000";

                if (!IPAddress.TryParse(strIPAddress, out ipaddr))
                {
                    Console.WriteLine("Invalid server IP supplied.");
                    return;
                }
                if (!int.TryParse(strPortInput.Trim(), out nPortInput))
                {
                    Console.WriteLine("Invalid port number supplied, return.");
                    return;
                }
                if (nPortInput <= 0 || nPortInput > 65535)
                {
                    Console.WriteLine("Port number must be between 0 and 65535");
                    return;
                }


                System.Console.WriteLine(string.Format("IPAddress: {0} - Port: {1}", ipaddr.ToString(), nPortInput));

                client.Connect(ipaddr, nPortInput);

                Console.WriteLine("Connected to the server, type text and press enter to send it to the server, type <EXIT> to close.");

                string inputCommand = string.Empty;

                //Task.Run(() => listen(client));
                Thread listenThread = new Thread(() => listen(client));
                listenThread.Start();

                while (true)
                {
                    inputCommand = Console.ReadLine();

                    if (inputCommand.Equals("<EXIT>"))
                    {
                        //cancellationTokenSource.Cancel();
                        listenThread.Abort();
                        break;
                    }

                    byte[] buffSend = Encoding.ASCII.GetBytes(inputCommand);

                    client.Send(buffSend);

                    //byte[] buffReceived = new byte[128];
                    //int numberOfReceivedBytes = client.Receive(buffReceived);

                    //Console.WriteLine("Data received: {0}", Encoding.ASCII.GetString(buffReceived, 0, numberOfReceivedBytes));
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.ToString());
            }
            finally
            {
                if (client != null)
                {
                    if (client.Connected)
                    {
                        client.Shutdown(SocketShutdown.Both);
                    }
                    client.Close();
                    client.Dispose();
                }
                //Clean up listener task
                cancellationTokenSource.Cancel();
            }

            Console.WriteLine("Press a key to exit...");
            Console.ReadLine();
        }
    }
}
