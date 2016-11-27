using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Microsoft.Win32;

namespace WakeYourPC.WakeUpAgent
{
    class ServiceMain : ServiceBase
    {
        private bool _wolStarted = false;
        Timer _wolPollTimer = null;

        /// <summary>
        /// Public Constructor for ServiceMain
        /// </summary>
        public ServiceMain()
        {
            this.ServiceName = "Wake Up Agent Service";
            this.EventLog.Log = "Application";

            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;
        }

        /// <summary>
        /// The Main Thread: This is where your Service is Run.
        /// </summary>
        public static void Main()
        {
            Run(new ServiceMain());
        }

        /// <summary>
        /// OnStart(): Put startup code here
        ///  - Start threads, get inital data, etc.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            string userName = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\WakeYourPC\\",
                "UserName", "saimanoj");
            this.EventLog.WriteEntry(string.Format("User name is {0}", userName), EventLogEntryType.Information, 100, 1);
            // Checking whether the username is valid
            UserEntity agentUser = ExternalConnector.GetUser(userName);
            if (agentUser == null)
            {
                this.EventLog.WriteEntry("Invalid User name Or the user is not registered yet" +
                                  string.Format("Please register the {0} first and then try passing as a command line argument.", userName) +
                                  "Exiting Now :-(");
                return;
            }
            UserEntitySingletonFactory.CreateUserEntity(userName);

            _wolPollTimer = new Timer(WolTimerCallback, null, 0, WakeOnLanConstants.WolPollInterval);

            //WOLTimerCallback(null);
            /*string macAddress = MacAddress.GetMac("10.171.41.67");

            this.EventLog.WriteEntry("MacAddress=" + macAddress);
            byte[] macBytes = MacAddress.GetMacBytes(macAddress, '-');

            MagicPacket mp = new MagicPacket(macBytes, "10.171.41.67");
            mp.WakeUp();
            bool result = mp.Verify();
            this.EventLog.WriteEntry(result);*/

        }

        /// <summary>
        /// OnStop(): Put your stop code here
        /// - Stop threads, set final data, etc.
        /// </summary>
        protected override void OnStop()
        {
            if (_wolPollTimer != null)
            {
                _wolPollTimer.Dispose();
                _wolPollTimer = null;
            }
        }

        public void WolTimerCallback(Object state)
        {
            this.EventLog.WriteEntry("Entered WOL Timer Callback");
            if (_wolStarted)
            {
                this.EventLog.WriteEntry("Previous work is in progress");
                return;
            }

            try
            {
                _wolStarted = true;
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

                        // PUT this machine object to azure service
                        ExternalConnector.UpdateMachineInfo(MachineObj);
                    }

                }
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry("Exception in WOLTImer call back " + e);
            }

            _wolStarted = false;
        }



        /// <summary>
        /// Dispose of objects that need it here.
        /// </summary>
        /// <param name="disposing">Whether
        ///    or not disposing is going on.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }


        /// <summary>
        /// OnPause: Put your pause code here
        /// - Pause working threads, etc.
        /// </summary>
        protected override void OnPause()
        {
            base.OnPause();
        }

        /// <summary>
        /// OnContinue(): Put your continue code here
        /// - Un-pause working threads, etc.
        /// </summary>
        protected override void OnContinue()
        {
            base.OnContinue();
        }

        /// <summary>
        /// OnShutdown(): Called when the System is shutting down
        /// - Put code here when you need special handling
        ///   of code that deals with a system shutdown, such
        ///   as saving special data before shutdown.
        /// </summary>
        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        /// <summary>
        /// OnCustomCommand(): If you need to send a command to your
        ///   service without the need for Remoting or Sockets, use
        ///   this method to do custom methods.
        /// </summary>
        /// <param name="command">Arbitrary Integer between 128 & 256</param>
        protected override void OnCustomCommand(int command)
        {
            //  A custom command can be sent to a service by using this method:
            //#  int command = 128; //Some Arbitrary number between 128 & 256
            //#  ServiceController sc = new ServiceController("NameOfService");
            //#  sc.ExecuteCommand(command);

            base.OnCustomCommand(command);
        }

        /// <summary>
        /// OnPowerEvent(): Useful for detecting power status changes,
        ///   such as going into Suspend mode or Low Battery for laptops.
        /// </summary>
        /// <param name="powerStatus">The Power Broadcast Status
        /// (BatteryLow, Suspend, etc.)</param>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        /// <summary>
        /// OnSessionChange(): To handle a change event
        ///   from a Terminal Server session.
        ///   Useful if you need to determine
        ///   when a user logs in remotely or logs off,
        ///   or when someone logs into the console.
        /// </summary>
        /// <param name="changeDescription">The Session Change
        /// Event that occured.</param>
        protected override void OnSessionChange(
                  SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }
    }
}
