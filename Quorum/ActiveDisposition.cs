﻿#region License
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

    public enum TransportType { Tcp, Http };

    public static class ActiveDisposition {

        private const string DispositionKey = "__Dispo__";

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
        }

        public static void Initialise(string type = null) {
            AcceptTransportType(type ?? new Configuration().Get<string>(Constants.Configuration.DefaultTransport));
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
