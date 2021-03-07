using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketServer
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
                string receivedText = Encoding.ASCII.GetString(buff, 0, numberOfReceivedBytes);
                Console.WriteLine("Client Said: " + receivedText);

                if (receivedText == "x")
                {
                    cancellationTokenSource.Cancel();
                }

                Array.Clear(buff, 0, buff.Length);
                numberOfReceivedBytes = 0;
            }
        }

        static void Main(string[] args)
        {
            //Create socket object
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Create an IPAddress.socket listening on any ip address.
            IPAddress ipaddr = IPAddress.Any;
            //Define IP end point
            IPEndPoint ipep = new IPEndPoint(ipaddr, 25000);
            //Bind socket to ip end point

            try
            {
                listenerSocket.Bind(ipep);
                //Call listen method on the Listener socket boject, pass a num to signify how many
                //clients can wait for a connection while the system is busy handling another connection
                listenerSocket.Listen(5);
                //Call Accept method on the listener socket
                //Accept is a synchrounous method (Blocking Method) that will not move forward until operation is completed -- Very Bad Method
                Socket client = listenerSocket.Accept();
                //Print out client ip end point
                Console.WriteLine("Client connected: " + client.ToString() + " -Ip End Point: " + client.RemoteEndPoint.ToString());
                //Define a buffer as byte array of 128 byte length
                // byte[] buff = new byte[128];
                //Define number of recieved bytes as an int
                // int numberOfReceivedBytes = 0;

                //Create task to listen for input
                Task.Run(() => listen(client));

                while (true)
                {
                    // numberOfReceivedBytes = client.Receive(buff);
                    //Console.WriteLine("Number of received bytes: " + numberOfReceivedBytes);
                    //Convert from byte array to ascii
                    // string receivedText = Encoding.ASCII.GetString(buff, 0, numberOfReceivedBytes);
                    //Print text
                    // Console.WriteLine("Data sent by client is: " + receivedText);
                    //Send data back to sender
                    //client.Send(buff);

                    Console.WriteLine("Please enter response to client: ");
                    string inputCommand = Console.ReadLine();
                    byte[] outBuff = Encoding.ASCII.GetBytes(inputCommand);
                    client.Send(outBuff);

                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    //Array.Clear(buff, 0, buff.Length);
                    //numberOfReceivedBytes = 0;
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.ToString());
            }
            finally
            {
                cancellationTokenSource.Cancel();
            }
        }
    }
}
