using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSM {
    
    public interface IEventSummary {
        string Name { get; }
        long Occurrences { get; set; }
        bool Processed { get; set; }
    }

}
