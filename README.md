# ArpSpooferLite-in-CSharp
In this repository I created a lightweight version of an Arp Spoofer. To create a MITM Attack in a local Network to interfere and watch traffic that uses unsecure HTTP protocol. For training educational purpose.

In this project I learned a lot about how adress resolution works, how network devices map IP adresses to MAC and how traffic flows inside a LAN. 

## Ip vs MAC quick intro
MAC	addresses	identify	who	you	are,	IP	addresses	identify	where	you	are,
and	ARP	tables	manage	the	mapping	between	who	you	are	and	where	you
are	on	the	network.	In an	ARP	spoofing	attack,	we	pretend	to	be	someone
else

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

Example image from book: Ethical hacking a hans on introduction to breaking in by Daniel G. Graham
<img width="974" height="759" alt="The first stage of an ARP spoofing attack" src="https://github.com/user-attachments/assets/21467e17-1685-4453-ba75-dfdf8ca32c90" />

info: The attacker has to enable ip forwarding. This allows the attacker’s machine to pass packets through to the real gateway, keeping the victim online while traffic flows through the attacker.

After becoming the Middle Point the victims traffic passes through the attackers machine. The attacker forwards the packets to the real gateway and can observe or analyze the unencrypted traffic (e.g HTTP). The gateway responds back to the attacker and the attacker forwards that to the victim.

A better way to enhance the chance to not get detected or blocked by other Wlan rules is to poison the router aswell. By sending arp replies to the router and pretending to be the victim.

## Why this works only in IPv4
ARP exists only in IPv4 networks.
IPv6 uses the Neighbor Discovery Protocol (NDP), which is more complex and not compatible with ARP spoofing techniques.
