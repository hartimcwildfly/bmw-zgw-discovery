using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace BmwDiscovery
{
    class Program
    {
        public static IPAddress BuildIPv4BroadcastAddress(IPAddress ipAddress, IPAddress mask)
        {
            UInt32 ipInt = BitConverter.ToUInt32(ipAddress.GetAddressBytes(), 0);
            UInt32 maskInt = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);

            return new IPAddress(ipInt | ~maskInt);
        }

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
                        var broadcastAddress = BuildIPv4BroadcastAddress(unicastIp.Address, unicastIp.IPv4Mask);
                        Console.WriteLine("\tInterfaceName: " + nic.Name + " IPv4: " + unicastIp.Address + " BroadcastAddr: " +
                                          broadcastAddress);
                        broadcastAddresses.Add(broadcastAddress);
                        //unicastIp.PrefixLength not supported in .NET Core 3.1 on Linux, but supported in .NET 5.0 RC1 on Linux
                    }
                }
            }

            if (broadcastAddresses.Count < 1)
            {
                Console.WriteLine("Not found any suitable interfaces/networks. Now quitting.");
            }

            // TODO improvement: Start for each interface/broadcastIp a separate thread. Then wait in the main thread until all threads are terminated or timed out.
            UdpClient udpClient = new UdpClient();

            var endpoint = new IPEndPoint(broadcastAddresses[0], 6811);
            udpClient.EnableBroadcast = true;

            var discoverMessage = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x11};

            udpClient.Send(discoverMessage, discoverMessage.Length, endpoint);

            //wait sync for answer
            var response = udpClient.Receive(ref endpoint);

            Console.WriteLine("Endpoint IP is: " + endpoint.Address);
            Console.WriteLine(Encoding.Default.GetString(response));
        }
    }
}
