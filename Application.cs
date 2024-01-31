using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace BmwDiscovery
{
    class Application
    {
        private static readonly byte[] DiscoveryPayload = [0x00, 0x00, 0x00, 0x00, 0x00, 0x11];
        private const int DiscoveryPort = 6811;

        static void Main(string[] args)
        {
            List<UnicastIPAddressInformation> interfaceAddresses = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
                .Where(unicastIp => unicastIp.Address.AddressFamily == AddressFamily.InterNetwork)
                .Where(unicastIp => !IPAddress.IsLoopback(unicastIp.Address))
                .ToList();

            if (interfaceAddresses.Count < 1) Console.WriteLine("No suitable interfaces/networks found.");

            interfaceAddresses.AsParallel().ForAll(address => {
                    var ipAddressWithCidr = address.Address + "/" + address.PrefixLength;
                    Console.WriteLine("Sending broadcast on interface: " + ipAddressWithCidr);
                    try
                    {
                        UdpClient udpClient = new UdpClient(new IPEndPoint(address.Address, 0));
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
                        Console.WriteLine("Receiving message on interface " + ipAddressWithCidr + " failed: " + exception.Message);
                    }
                }
            );
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
