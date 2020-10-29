using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace BmwDiscovery
{
    class Application
    {
        private static readonly byte[] DiscoveryPayload = {0x00, 0x00, 0x00, 0x00, 0x00, 0x11};
        private const int DiscoveryPort = 6811;

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
                    }
                }
            }

            if (broadcastAddresses.Count < 1)
            {
                Console.WriteLine("Not found any suitable interfaces/networks.");
            }

            broadcastAddresses.AsParallel().ForAll(
                address =>
                {
                    Console.WriteLine("Sending broadcast on interface: " + address);
                    try
                    {
                        UdpClient udpClient = new UdpClient(new IPEndPoint(address, 0));
                        udpClient.Client.ReceiveTimeout = 3000;

                        var endpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
                        udpClient.EnableBroadcast = true;

                        udpClient.Send(DiscoveryPayload, DiscoveryPayload.Length, endpoint);
                        var response = udpClient.Receive(ref endpoint);
                        Console.WriteLine("Endpoint IP is: " + endpoint.Address);
                        Console.WriteLine("Response was: " + Encoding.ASCII.GetString(response));
                    }
                    catch (SocketException exception)
                    {
                        Console.WriteLine("Receiving message on interface " + address + " failed: " + exception.Message);
                    }
                }
                );
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
