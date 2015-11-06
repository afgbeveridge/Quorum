using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum.Payloads {
    
    public class BasePayload {

        protected BasePayload() {
            TypeHint = GetType().Name;
        }

        public string TypeHint { get; set; }

    }
}
