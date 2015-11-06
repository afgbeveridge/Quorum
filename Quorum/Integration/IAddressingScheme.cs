using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum.Integration {
    
    public interface IAddressingScheme {

        string Name { get; }

        string Address { get; }

        string Port { get; set; }

    }
}
