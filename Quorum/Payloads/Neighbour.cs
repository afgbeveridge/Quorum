using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum.Payloads {

    public enum MachineStrength {  Compute, IO, Memory }

    public class Neighbour : BasePayload {

        public bool IsMaster { get; set; }

        public string Name { get; set; }

        // Could be extended to allow heterogeneous addressing
        public string ContactAddress { get; set; }

        // Other qualities

        public double UpTime { get; set; }

        public double? TimeUntilRestart { get; set; }

        public MachineStrength Strength { get; set; }

    }

}
