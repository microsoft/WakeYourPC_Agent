using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using Newtonsoft.Json;

namespace WakeYourPC.WakeUpAgent
{
    public static class WakeOnLanConstants
    {
        // Machine info Poll Interval in milli seconds
        public const int MachineInfoPollInterval = 300000;

        // Wake on Lan Poll Interval in seconds
        public const int WolPollInterval = 30*1000;
    }

    /*
    class Start
    {
        public bool WOLStarted = false;
        Timer WOLPollTimer = null;

        public void WOLTimerCallback(Object state)
        {
            Console.WriteLine("Entered WOL Timer Callback");
            if (WOLStarted)
            {
                Console.WriteLine("Previous work is in progress");
                return;
            }

            try
            {
                WOLStarted = true;
                List<MachineEntity> model = ExternalConnector.GetMachinesToAwake();

                foreach (var MachineObj in model)
                {
                    if (MachineObj.ShouldWakeup && MachineObj.MacAddress != null)
                    {
                        byte[] macBytes = MacAddress.GetMacBytes(MachineObj.MacAddress, '-');

                        // MagicPacket mp = new MagicPacket(macBytes, "10.171.41.67");
                        MagicPacket mp = new MagicPacket(macBytes, MachineObj.HostName); // TODO verify
                        mp.WakeUp();

                        bool result = mp.Verify();
                        MachineObj.ShouldWakeup = false;
                        MachineObj.State = (result) ? MachineState.Available.ToString() : MachineState.Unreachable.ToString();

                        // PUT this to azure service
                        ExternalConnector.UpdateMachineInfo(MachineObj);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in WOLTImer call back");
            }

            WOLStarted = false;
        }

        public void OnStart()
        {
            WOLPollTimer = new Timer(WOLTimerCallback, null, 0, WakeOnLanConstants.WOLPollInterval);

            //WOLTimerCallback(null);
            string macAddress = MacAddress.GetMac("10.171.41.67");

            Console.WriteLine("MacAddress=" + macAddress);
            byte[] macBytes = MacAddress.GetMacBytes(macAddress, '-');

            MagicPacket mp = new MagicPacket(macBytes, "10.171.41.67");
            mp.WakeUp();
            bool result = mp.Verify();
            Console.WriteLine(result);
        }

        public void OnStop()
        {
            if (WOLPollTimer != null)
            {
                WOLPollTimer.Dispose();
                WOLPollTimer = null;
            }
        }

    }
*/
}
