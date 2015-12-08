using System;
using System.Threading.Tasks;

namespace Quorum {

    public interface IMasterWorkAdapter {

        Action WorkUnitExecuted { get; set; }

        Task Activated();

        Task DeActivated();

    }

}
