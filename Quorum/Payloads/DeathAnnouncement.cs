using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum.Payloads {
    
    public class DeathAnnouncement : BasePayload {

        public string Name { get; set; }

        public bool IsMaster { get; set; }

    }

}
