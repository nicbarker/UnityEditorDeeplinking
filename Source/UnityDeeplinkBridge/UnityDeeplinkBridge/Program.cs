using System;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UnityDeeplinkBridge
{
    class Program 
    {
        public static void Main(string[] args)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9910);

                // Create a TCP/IP socket.  
                using (Socket sender = new Socket(IPAddress.Parse("127.0.0.1").AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    sender.Connect(remoteEP);

                    // Encode the data string into a byte array.  
                    string data = string.Join(" ", args);
                    int protPos = data.IndexOf("://") + 3;
                    data = data.Substring(protPos, data.Length - protPos);
                    Console.WriteLine(data);
                    //convert our data to bytes and add a header that specifies the number of bytes sent.
                    byte[] dataBytes = Encoding.ASCII.GetBytes(data.ToCharArray());
                    byte[] length = BitConverter.GetBytes(dataBytes.Length);
                    byte[] msg = new byte[length.Length + dataBytes.Length];
                    length.CopyTo(msg, 0);
                    dataBytes.CopyTo(msg, length.Length);

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }
    }
}
