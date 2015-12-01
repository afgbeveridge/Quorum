using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infra;

namespace Quorum {

    // Lazy class :-)

    public enum TransportType { Tcp, Http };

    public static class ActiveDisposition {

        static ActiveDisposition() {
            Current = TransportType.Http;
        }

        public static TransportType Current { get; set; }

        public static void AcceptTransportType(string type) {
            Current = (TransportType)Enum.Parse(typeof(TransportType), type, true);
        }

        public static void Initialise(string type = null) {
            AcceptTransportType(type ?? new Configuration().Get<string>(Constants.Configuration.DefaultTransport));
        }

    }

}
