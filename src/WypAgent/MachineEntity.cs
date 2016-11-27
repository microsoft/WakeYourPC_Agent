using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakeYourPC.WakeUpAgent
{
    public enum MachineState
    {
        Unknown = 0,
        Available,
        Asleep,
        WakingUp,
        Unreachable
    }

    public class MachineEntity
    {
        public string Username { get; set; }

        public string MachineName { get; set; }

        public string HostName { get; set; }

        public string Guid { get; set; }

        public string MacAddress { get; set; }

        public string State { get; set; }

        public bool ShouldWakeup { get; set; }
    }
}

