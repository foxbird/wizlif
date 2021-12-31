using Foxpaws.Wizlif.Listeners;
using Foxpaws.Wizlif.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Foxpaws.Wizlif.Scanners
{
    public class WizScanner : IScanner
    {
        private readonly static int PING_DELAY_MS = 1000;
        private readonly static int PING_COUNT = 10;

        private readonly ILogger<WizScanner> _logger;
        protected IWizListener _listener;

        protected UdpClient Client { get; set; }

        protected WizBulbRegistry Registry { get; set; } = new WizBulbRegistry();

        public WizScanner(ILogger<WizScanner> logger, IWizListener listener)
        {
            _logger = logger;
            _listener = listener;
        }

        public async Task Scan(string interfaceName, int port)
        {
            _logger.LogDebug("Performing WizScan");
            var netIf = FindInterface(interfaceName);
            if (netIf == null)
            {
                _logger.LogCritical($"Could not find an interface to scan with");
                return;
            }

            var bcast = FindBbroadcastAddress(netIf);

            // Create the UdpClient
            UdpClient client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));

            // Allow sending of broadcast datagrabs
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            // Start the listener
            _listener.DataReceived = ReceivePing;
            _listener.Port = port;
            _listener.Client = client;
            _listener.Start();

            // Send the ping
            for (int i = 0; i < PING_COUNT; i++)
            {
                SendPing(client, bcast, port);
                Thread.Sleep(PING_DELAY_MS);
            }
            
            // Stop the listener
            _listener.Stop();

            // Print out a status
            Registry.Bulbs.ForEach(bulb => Console.WriteLine($"IP Address: {bulb.IpAddress}, MAC Address: {bulb.MacAddress}"));

            await Task.CompletedTask;
        }

        private void SendPing(UdpClient client, IPAddress bcast, int port)
        {
            var register = new WizRegisterMessage();
            var bytes = JsonSerializer.SerializeToUtf8Bytes(register);
            _logger.LogInformation($"Sending wiz register message of {bytes.Length} bytes to {bcast}:{port}");
            client.Send(bytes, bytes.Length, new IPEndPoint(bcast, port));
        }

        private void ReceivePing(UdpClient client, IPEndPoint endpoint, byte[] data)
        {
            _logger.LogDebug($"Received UDP response from WizBulb");
            var dgram = System.Text.Encoding.UTF8.GetString(data);
            var response = JsonSerializer.Deserialize<WizRegisterResponseMessage>(dgram);

            _logger.LogInformation($"Registered bulb {endpoint.Address} / {response.Result.MacAddress}: {response.Result.Success}");

            Registry.Add(endpoint.Address, response.Result.MacAddress);
        }

        private IPAddress FindBbroadcastAddress(NetworkInterface netIf)
        {
            IPAddress ipAddress = null, subnetMask = null;

            // Get the first IP address on the interface
            // TODO: Select all IP's on the interface
            var ipProps = netIf.GetIPProperties();
            foreach (var uniAddress in ipProps.UnicastAddresses)
            {
                // Ignore non IPv4 addresses
                if (uniAddress.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                    continue;
                ipAddress = uniAddress.Address;
                subnetMask = uniAddress.IPv4Mask;
                break;
            }

            var ipBytes = BitConverter.ToUInt32(ipAddress.GetAddressBytes(), 0);
            var maskBytes = BitConverter.ToUInt32(subnetMask.GetAddressBytes(), 0);
            var broadcastValue = ipBytes | ~maskBytes;
            var bcAddress = new IPAddress(broadcastValue);

            _logger.LogInformation($"Broadcast address for {netIf.Name} is {bcAddress}");

            return bcAddress;
        }

        private NetworkInterface FindInterface(string interfaceName = default)
        {

            List<NetworkInterface> consideredInterfaces = new List<NetworkInterface>();
            foreach (var netIf in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netIf.Name.ToLower() == interfaceName?.ToLower())
                {
                    _logger.LogDebug($"Selected interface {netIf.Name} by name");
                    return netIf;
                }

                // Don't consider disabled interfaces
                if (netIf.OperationalStatus != OperationalStatus.Up)
                {
                    _logger.LogDebug($"Ignored {netIf.Name} as it is not operational");
                    continue;
                }

                // Don't consider loopback interfaces
                if (netIf.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    _logger.LogDebug($"Ignored {netIf.Name} as it is type loopback");
                    continue;
                }

                var ipProps = netIf.GetIPProperties();

                var ipv4Addresses = ipProps.UnicastAddresses.Select(ua => ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                if (ipv4Addresses.Count() == 0)
                {
                    _logger.LogDebug($"Ignored {netIf.Name} as it has no IPv4 addresses");
                    continue;
                }

                // Don't consider interfaces without a gateway (local only)
                if (ipProps.GatewayAddresses.Count == 0)
                {
                    _logger.LogDebug($"Ignored {netIf.Name} as it has no gateway");
                    continue;
                }

                // Finally, consider this interface
                consideredInterfaces.Add(netIf);
            }

            if (consideredInterfaces.Count == 0)
            {
                _logger.LogCritical($"Could not find an interface to scan on");
                return null;
            }

            // The first interface in our list is the one we want
            var result = consideredInterfaces.First();

            if (consideredInterfaces.Count > 1)
            {
                _logger.LogWarning($"Too many interfaces eligible for scanning. Selected {result.Name}. Specify one using the interface argument to pick a particular one");
            }

            _logger.LogInformation($"Found interface {result.Name}");
            return result;
        }
    }
}
