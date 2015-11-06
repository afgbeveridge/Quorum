using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Quorum.Integration.Http {

    public class ClientDetails {
        public HttpClient Client { get; set; }
        public string Address { get; set; }
    }

}
