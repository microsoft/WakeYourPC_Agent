using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Globalization;

namespace WakeYourPC.WakeUpAgent
{
    class MagicPacket
    {
        public byte[] mac;
        public string ip;
        const int retryCount = 5;
        public MagicPacket(byte[] macAddress, string hostname)
        {

            mac = new byte[macAddress.Length];
            Array.Copy(macAddress, mac, macAddress.Length);
            IPHostEntry host = Dns.GetHostEntry(hostname);
            foreach (IPAddress Entry in host.AddressList)
            {
                ip = Entry.ToString();
                break;
                //Console.WriteLine(ip);
            }
          
        }

        public void WakeUp()
        {
            
            //
            // WOL packet is sent over UDP 255.255.255.0:40000.
            //
            UdpClient client = new UdpClient();
            client.Connect(IPAddress.Broadcast, 40000);

            //
            // WOL packet contains a 6-bytes trailer and 16 times a 6-bytes sequence containing the MAC address.
            //
            byte[] packet = new byte[18 * 6];

            //
            // Trailer of 6 times 0xFF.
            //
            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;

            //
            // Body of magic packet contains 16 times the MAC address.
            //
            /*for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];
            */
            //string MAC_ADDRESS = "1866da069a63";
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                {
                    packet[i * 6 + j] = mac[j];
                }

            // Add Password "000000000000" at end
            for (int i = 17; i <= 17; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = 0X00;

            //
            // Send Wakup packet.
            //
            client.Send(packet, packet.Length);
            Console.WriteLine("Sent WOL packet ");
        }

        public bool Verify()
        {
            bool result = false;            
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;

            for (int i = 0; i < retryCount; i++)
            {
                PingReply reply = pingSender.Send(ip, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    result = true;
                    Console.WriteLine("Ping Succeeded" + ip);
                    break;
                }
                Thread.Sleep(5000);
            }
            return result;
        }
    } 
}
