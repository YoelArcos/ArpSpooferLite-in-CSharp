using System.Net.NetworkInformation;
using System.Net;

namespace ArpSpooferLite
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Console.WriteLine("ARP Spoofer Lite");
            //Console.WriteLine("This is a simple ARP spoofer tool.");
            //// Check if the user has provided the necessary arguments
            //if (args.Length < 2)
            //{
            //    Console.WriteLine("Usage: ArpSpooferLite.exe <victim IP> <router IP>");
            //    return;
            //}
            //string victimIp = args[0];
            //string routerIp = args[1];

            // NOTE: Arp spoofing usage: Spoof Router IP to Target IP and Spoof Target IP to Router IP

            string victimIpTest = "192.168.1.1";
            string routerIpTest = "192.168.1.1";

            //use ping -4 192.168.1.34
            string hostName = Dns.GetHostName();
            string attackerIp = Dns.GetHostEntry(hostName).AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString(); 
            string attackerMac = GetMacByIp(attackerIp);
            Console.WriteLine("Attacker IP : " + attackerIp);
            Console.WriteLine("Attacker Mac : " + attackerMac);

            string victimMac = GetMacByIp(victimIpTest);
            Console.WriteLine("\nVictim Ip : " + victimIpTest); // TODO: change to targetIp
            Console.WriteLine("Victim Mac : " + victimMac);

            string routerMac = GetMacByIp(routerIpTest);
            Console.WriteLine("\nRouter Ip : " + routerIpTest);
            Console.WriteLine("Router Mac : " + victimMac);

            SpoofArpReply(victimMac, routerMac);
        }

        private static string? GetMacByIp(string ip)
        {
            var pingOption = new PingOptions()
            {
                DontFragment = false
            };

            // Ipv4 uses 32 byte long pings (TTL) and Ipv6 uses (128) so we force to ping in ipv4.
            byte[] buffer = new byte[32];

            // Ping the target IP to ensure it's reachable and to populate the ARP cache
            try
            {
                PingReply reply = new System.Net.NetworkInformation.Ping().Send(ip, 500,buffer, pingOption);
            }
            catch (Exception ex)
            {
                

                Console.WriteLine("Ping failed!: " + ex.Message);
                return null;
            }

            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "arp";
            process.StartInfo.Arguments = "-a " + ip;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            //Console.WriteLine(output);

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

        private static void SpoofArpReply(string? victimMac, string? routerMac)
        {
        }
    }
}