using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum {
    
    public interface IMasterWorkAdapter {

        Action WorkUnitExecuted { get; set; }

        Task Activated();

        Task DeActivated();

    }

}
