using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BmwDiscovery
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpClient udpClient = new UdpClient();

            var endpoint = new IPEndPoint(IPAddress.Parse("169.254.255.255"), 6811);
            //var endpoint = new IPEndPoint(IPAddress.Broadcast, 6811);
            udpClient.EnableBroadcast = true;

            var discoverMessage = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x11};

            udpClient.Send(discoverMessage, discoverMessage.Length, endpoint);

            //wait sync for answer
            var response = udpClient.Receive(ref endpoint);  //works with endpoint or endpoint2
            
            Console.WriteLine("Endpoint IP is: " + endpoint.Address);
            Console.WriteLine(Encoding.Default.GetString(response));
        }
    }
}
