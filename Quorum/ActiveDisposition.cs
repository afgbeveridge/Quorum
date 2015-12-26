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

    public enum TransportType { Tcp, Http, Https, Tcps };

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

        public static void AcceptTransportType(string type) {
            AcceptTransportType((TransportType)Enum.Parse(typeof(TransportType), type, true));
        }

        public static void AcceptTransportType(TransportType type) {
            Shim.Current = type;
            new Configuration().WithAppropriateOverrides().LocalSet(Constants.Configuration.EncryptedTransportRequired.Key, Shim.Current.ToString().EndsWith("s"));
        }

        public static void Initialise(string type = null) {
            AcceptTransportType(type ?? new Configuration().Get(Constants.Configuration.DefaultTransport));
        }

        /// <summary>
        /// Shared == true means that the receiver is not to be regarded as multi thread safe
        /// </summary>
        public static bool Shared {
            get {
                return Shim.IsTransient;
            }
            set {
                Shim = value ? new SharedDisposition() : (IDispositionShim)new StaticDisposition();
            }
        }

        public static bool IsTransient{
            get {
                return Shim.IsTransient;
            }
        }

        private static IDispositionShim Shim { get; set; }

        private interface IDispositionShim {
            TransportType Current { get; set; }
            bool IsTransient { get; }
        }

        private class StaticDisposition : IDispositionShim {
            public TransportType Current { get; set; }
            public bool IsTransient {
                get { return false; }
            }
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

            public bool IsTransient {
                get { return true; }
            }

        }
    }
}
