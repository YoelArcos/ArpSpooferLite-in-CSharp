# ArpSpooferLite-in-CSharp
In this repository I created a lightweight version of an Arp Spoofer. To create a MITM Attack in a local Network to interfere and watch traffic that uses unsecure HTTP protocol. For training educational purpose.

In this project I learned a lot about how adress resolution works, how network devices map IP adresses to MAC and how traffic flows inside a LAN. 

# Installation (Usage Guide)
1. Install NPcap (Packet Library & Driver): [Npcap: Windows Packet Capture Library & Driver](https://npcap.com/) (select WinPcap-API-Compatibility! while going through the installation wizard)
(**Windows does not provide raw packet access natively**).
2. Clone or get the latest release containing the .exe file for windows.
`git clone https://github.com/YoelArcos/ArpSpooferLite-in-CSharp.git
cd ArpSpooferLite-in-CSharp
dotnet build -c Release`
3. Enable IP Forwarding (explained later in this README) **windows**: `Set-NetIPInterface -Forwarding Enabled`
for **linux** `sudo sysctl -w net.ipv4.ip_forward=1`
4. Start .exe file with this command `ArpSpooferLite.exe <victim IP> <router IP>`

To end the program just press the `Q`-Key and it will restore automatically all arp tables. 

(Note: ARP Spoofing only works in the same Layer-2 network segment)

# Education
Coming soon...


## Ip vs MAC quick intro
MAC adresses identify who you are, IP adresses identify where you are on the network
and ARP tables manae the mapping between who you are and where you are on the network.
(In an ARP spoofing attack, we pretend to be someone else).

## What ARP Does (Adress Resolution Protocol)
ARP is a simple protocol used in IPv4 networks.
Its job is:
Translate an IP address → into the MAC address of the device that owns it.
When a device wants to send a packet to 192.168.1.1, it asks:
“Who has 192.168.1.1? Tell me your MAC address.”
This is an ARP Request (broadcast).
The correct device replies:
“192.168.1.1 is at AA:BB:CC:DD:EE:FF.”
This is an ARP Reply (unicast).

Devices store these mappings in an ARP cache.

![alt text](https://www.cloudns.net/blog/wp-content/uploads/2023/05/How-Does-ARP-Work.png)

## What happens in an ARP-spoofing Attack
ARP has no authentication.
Any device on the LAN can send ARP replies — even if nobody asked for them.
This allows a malicious device to send fake ARP replies such as:
“192.168.1.1 is at MY MAC ADDRESS.”
If the victim accepts this fake entry, it will start sending traffic meant for the gateway to the attacker instead.

simplified presentation
<img width="998" height="582" alt="An example of a spoofing attack involving a postal worker" src="https://github.com/user-attachments/assets/9ff36a74-011e-4538-83ff-caa15cd516cf" />

Example image from book: Ethical hacking a hands on introduction to breaking in by Daniel G. Graham
<img width="974" height="759" alt="The first stage of an ARP spoofing attack" src="https://github.com/user-attachments/assets/21467e17-1685-4453-ba75-dfdf8ca32c90" />

info: The attacker has to enable ip forwarding. This allows the attacker’s machine to pass packets through to the real gateway, keeping the victim online while traffic flows through the attacker.

for **linux** `sudo sysctl -w net.ipv4.ip_forward=1`
for **windows** `Set-NetIPInterface -Forwarding Enabled`

After becoming the Middle Point the victims traffic passes through the attackers machine. The attacker forwards the packets to the real gateway and can observe or analyze the unencrypted traffic (e.g HTTP). The gateway responds back to the attacker and the attacker forwards that to the victim.

A better way to enhance the chance to not get detected or blocked by other Wlan rules is to poison the router aswell. By sending arp replies to the router and pretending to be the victim. (This is the second stage of poisoning in an arp spoofing attack.)
<img width="1000" height="780" alt="image" src="https://github.com/user-attachments/assets/84e9a40a-bbc7-468a-8931-af12863c65e7" />


After finishing the work the arp tables should be restored to the default values (the correct mappings of each device, so that the victim doesn't lose internet connection.)

## Why this works only in IPv4
ARP exists only in IPv4 networks.
IPv6 uses the Neighbor Discovery Protocol (NDP), which is more complex and not compatible with ARP spoofing techniques.

## ARP Packets in Detail 
Coming soon...

## How the Code Works
Coming soon...

