#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using Infra;
using System.Web;

namespace Quorum {

    // Lazy class :-)

    public enum TransportType { Tcp, Http, Https };

    public static class ActiveDisposition {

        private const string DispositionKey = "__transport_type_disposition__";

        static ActiveDisposition() {
           Shared = false;
           Initialise();
        }

        public static TransportType Current {
            get {
                return Shim.Current;
            }
            set {
                Shim.Current = value;
            }
        }

        private static void AcceptTransportType(string type) {
            Shim.Current = (TransportType)Enum.Parse(typeof(TransportType), type, true);
            new Configuration().LocalSet(Constants.Configuration.EncryptedTransportRequired.Key, Shim.Current == TransportType.Https);
        }

        public static void Initialise(string type = null) {
            AcceptTransportType(type ?? new Configuration().Get(Constants.Configuration.DefaultTransport));
        }

        public static bool Shared { 
            set {
                Shim = value ? new SharedDisposition() : (IDispositionShim) new StaticDisposition();
            } 
        }

        private static IDispositionShim Shim { get; set; }

        private interface IDispositionShim { 
            TransportType Current { get; set; }
        }

        private class StaticDisposition : IDispositionShim {
            public TransportType Current { get; set; }
        }

        private class SharedDisposition : IDispositionShim {
            public TransportType Current {
                get {
                    return (TransportType)HttpContext.Current.Items[DispositionKey];
                }
                set {
                    HttpContext.Current.Items[DispositionKey] = value;
                }
            }
        }

    }

}
