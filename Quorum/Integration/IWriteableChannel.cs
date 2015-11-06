using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum.Integration {
    
    public interface IWriteableChannel {

        Task<string> Write(string address, string content, int timeoutMs);
        void Respond(object responseChannel, string content, int timeoutMs);
        IWriteableChannel NewInstance();

    }

}
