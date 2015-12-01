using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum.Integration.Tcp {
    
    public class TcpReadableChannel : IReadableChannel {

        public Task<string> Read(string address, int timeoutMs) {
            throw new NotImplementedException();
        }

        public IReadableChannel NewInstance() {
            throw new NotImplementedException();
        }
    }
}
