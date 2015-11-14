using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infra;

namespace Quorum.Payloads {

    public enum MachineStrength {  Compute, IO, Memory }

    public class Neighbour : BasePayload {

        public bool IsMaster { get; set; }

        public bool InEligibleForElection { get; set; }

        public string Name { get; set; }

        // Could be extended to allow heterogeneous addressing
        public string ContactAddress { get; set; }

        // Other qualities

        public double UpTime { get; set; }

        public double? TimeUntilRestart { get; set; }

        public MachineStrength Strength { get; set; }

        public HardwareDetails Hardware { get; set; }

        public static Neighbour Fabricate(bool isMaster, string name, double upTime, bool ineligible) {
                var strength = new Configuration().Get<string>(Constants.Configuration.MachineStrength);
                var actualStrength = String.IsNullOrEmpty(strength) ? MachineStrength.Compute : (MachineStrength)Enum.Parse(typeof(MachineStrength), strength, true);
                return new Neighbour {
                    IsMaster = isMaster,
                    Name = name,
                    UpTime = upTime,
                    Strength = actualStrength,
                    Hardware = new HardwareDetails(),
                    InEligibleForElection = ineligible
                };
        }

    }

    public class HardwareDetails {
        public HardwareDetails() {
            PhysicalMemory = HardwareInfo.GetPhysicalMemory();
            CPUManufacturer = HardwareInfo.GetCPUManufacturer();
            CPUSpeed = HardwareInfo.GetCpuSpeedInGHz().HasValue ? HardwareInfo.GetCpuSpeedInGHz().Value.ToString() : "Unknown";
            OS = HardwareInfo.GetOSInformation();
        }
        public string PhysicalMemory { get; set; }
        public string CPUManufacturer { get; set; }
        public string CPUSpeed { get; set; }
        public string OS { get; set; }
    }

}
