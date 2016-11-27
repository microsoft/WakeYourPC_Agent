using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Net;
using System.Globalization;
using System.Text;
using System.Linq;


namespace WakeYourPC.WakeUpAgent
{
    static class MacAddress
    {
        // The max number of physical addresses.
        const int MAXLEN_PHYSADDR = 8;

        // Define the MIB_IPNETROW structure.
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_IPNETROW
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwIndex;
            [MarshalAs(UnmanagedType.U4)]
            public int dwPhysAddrLen;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac0;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac1;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac2;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac3;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac4;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac5;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac6;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac7;
            [MarshalAs(UnmanagedType.U4)]
            public int dwAddr;
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
        }

        // Declare the GetIpNetTable function.
        [DllImport("IpHlpApi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int GetIpNetTable(
           IntPtr pIpNetTable,
           [MarshalAs(UnmanagedType.U4)]
         ref int pdwSize,
           bool bOrder);

        [DllImport("IpHlpApi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int FreeMibTable(IntPtr plpNetTable);

        // The insufficient buffer error.
        const int ERROR_INSUFFICIENT_BUFFER = 122;


        public static byte[] GetMacBytes(String mac, char delimiter)
        {
            //String temp = mac.Replace(delimiter, "");
            //Console.WriteLine(temp);
            //long value = long.Parse(temp, NumberStyles.HexNumber, CultureInfo.CurrentCulture.NumberFormat);
            // byte[] macBytes = BitConverter.GetBytes(value);
            // hack: find a good method to convert directly from MAC to byte[]
            //   byte[] truncMac = new byte[6];
            //  Array.Copy(macBytes, truncMac, truncMac.Length);
            /*   for (int i = 0; i < 6; i++)
               {
                   Integer hex = Integer.parseInt(temp[i], 16);
                   truncMac[i] = hex.byteValue();
               }          
         */
            byte[] arr = mac.Split(delimiter).Select(x => Convert.ToByte(x, 16)).ToArray();
            byte[] truncMac = new byte[6];
            Array.Copy(arr, truncMac, truncMac.Length);
            return truncMac;
        }

        public static string GetMac(string givenIp)
        {
            // The number of bytes needed.
            int bytesNeeded = 0;

            // The result from the API call.
            int result = GetIpNetTable(IntPtr.Zero, ref bytesNeeded, false);

            // Call the function, expecting an insufficient buffer.
            if (result != ERROR_INSUFFICIENT_BUFFER)
            {
                // Throw an exception.
                throw new Win32Exception(result);
            }

            // Allocate the memory, do it in a try/finally block, to ensure
            // that it is released.
            IntPtr buffer = IntPtr.Zero;

            try
            {
                // Allocate the memory.
                buffer = Marshal.AllocCoTaskMem(bytesNeeded);

                // Make the call again. If it did not succeed, then
                // raise an error.
                result = GetIpNetTable(buffer, ref bytesNeeded, false);

                // If the result is not 0 (no error), then throw an exception.
                if (result != 0)
                {
                    // Throw an exception.
                    throw new Win32Exception(result);
                }

                // Now we have the buffer, we have to marshal it. We can read
                // the first 4 bytes to get the length of the buffer.
                int entries = Marshal.ReadInt32(buffer);

                // Increment the memory pointer by the size of the int.
                IntPtr currentBuffer = new IntPtr(buffer.ToInt64() +
                   Marshal.SizeOf(typeof(int)));

                // Allocate an array of entries.
                MIB_IPNETROW[] table = new MIB_IPNETROW[entries];

                // Cycle through the entries.
                for (int index = 0; index < entries; index++)
                {
                    // Call PtrToStructure, getting the structure information.
                    table[index] = (MIB_IPNETROW)Marshal.PtrToStructure(new
                       IntPtr(currentBuffer.ToInt64() + (index *
                       Marshal.SizeOf(typeof(MIB_IPNETROW)))), typeof(MIB_IPNETROW));
                }

                for (int index = 0; index < entries; index++)
                {
                    MIB_IPNETROW row = table[index];
                    IPAddress ip = new IPAddress(BitConverter.GetBytes(row.dwAddr));
                    /*  Console.Write("IP:" + ip.ToString() + "\t\tMAC:");

                      Console.Write(row.mac0.ToString("X2") + '-');
                      Console.Write(row.mac1.ToString("X2") + '-');
                      Console.Write(row.mac2.ToString("X2") + '-');
                      Console.Write(row.mac3.ToString("X2") + '-');
                      Console.Write(row.mac4.ToString("X2") + '-');
                      Console.WriteLine(row.mac5.ToString("X2")); */
                    if (ip.ToString() == givenIp)
                    {
                        FreeMibTable(buffer);
                        // Console.WriteLine(ip.ToString());
                        string output = row.mac0.ToString("X2") + '-' + row.mac1.ToString("X2") + '-' +
                            row.mac2.ToString("X2") + '-' + row.mac3.ToString("X2") + '-' +
                            row.mac4.ToString("X2") + '-' + row.mac5.ToString("X2");
                        return output;
                    }
                }
                // Release the memory.
                FreeMibTable(buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine("GetMac: Exception " + e);
            }
            return null;
        }
    }
}