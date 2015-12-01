using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum.Payloads {

    public class QueryRequest : BasePayload {

        public string Requester { get; set; }

        public int? Timeout { get; set; }

    }
}
