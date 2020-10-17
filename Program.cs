using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace BmwDiscovery
{
    class Program
    {
        static byte[] DISCOVERY_PAYLOAD = {0x00, 0x00, 0x00, 0x00, 0x00, 0x11};

        static void Main(string[] args)
        {
            List<IPAddress> broadcastAddresses = new List<IPAddress>();
            Console.WriteLine("Found interfaces:");
            foreach(NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()) {
                foreach (var unicastIp in nic.GetIPProperties().UnicastAddresses)
                {
                    if (nic.OperationalStatus == OperationalStatus.Up && unicastIp.Address.AddressFamily == AddressFamily.InterNetwork
                        && !IPAddress.IsLoopback(unicastIp.Address))
                    {
                        Console.WriteLine("\tInterfaceName: " + nic.Name + " IPv4: " + unicastIp.Address);
                        broadcastAddresses.Add(unicastIp.Address);
                        //unicastIp.PrefixLength not supported in .NET Core 3.1 on Linux, but supported in .NET 5.0 RC1 on Linux
                    }
                }
            }

            if (broadcastAddresses.Count < 1)
            {
                Console.WriteLine("Not found any suitable interfaces/networks. Now quitting.");
            }

            // TODO improvement: Start for each interface/broadcastIp a separate thread. Then wait in the main thread until all threads are terminated or timed out.
            UdpClient udpClient = new UdpClient(new IPEndPoint(broadcastAddresses[0], 0));
            udpClient.Client.ReceiveTimeout = 2000;

            var endpoint = new IPEndPoint(IPAddress.Broadcast, 6811);
            udpClient.EnableBroadcast = true;

            udpClient.Send(DISCOVERY_PAYLOAD, DISCOVERY_PAYLOAD.Length, endpoint);

            //wait sync for answer
            try
            {
                var response = udpClient.Receive(ref endpoint);
                Console.WriteLine("Endpoint IP is: " + endpoint.Address);
                Console.WriteLine("Response was: " + Encoding.Default.GetString(response));
            }
            catch (SocketException exception)
            {
                Console.WriteLine(exception.Message);
                //Console.WriteLine(exception.ErrorCode);
                //Console.WriteLine(exception.SocketErrorCode);
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
