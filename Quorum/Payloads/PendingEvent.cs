using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum.Payloads {
    
    public class PendingEvent {

        public Guid Id { get; set;}

        public string Name { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}
