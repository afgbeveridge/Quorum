﻿#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quorum;
using FSM;
using Quorum.Integration;
using Infra;
using System.Net.NetworkInformation;
using System.DirectoryServices;

namespace ConsoleQuorum {

    public class Program {

        private static Dictionary<string, Func<bool>> Handlers = new Dictionary<string, Func<bool>> { 
            { "q", Query },
            { "x", Exit }
        };

        private static QuorumImplFacade Adapter { get; set; }

        public static void Main(string[] args) {

            try {
                Adapter = new QuorumImplFacade()
                    .WithBuilder(new Builder())
                    .OnTransport(args.Any() ? args.First() : null)
                    .Start<NullWorkerAdapter>();
                bool cont = true;
                while (cont) {
                    var k = Console.ReadLine();
                    if (k != null && Handlers.ContainsKey(k))
                        cont = Handlers[k]();
                }
            }
            finally {
                "Guard".GuardedExecution(() => {
                    // Caution as we are running an async method in a synchronous manner
                    if (Adapter.IsNotNull())
                        Task.Run(() => Adapter.Stop()).Wait();
                });
            }
        }

        private static bool Query() {
            Adapter.Machine.Trigger(EventInstance.Create(EventNames.Query, "{ \"TypeHint\": \"QueryRequest\" }"));
            return true;
        }

        private static bool Exit() {
            return false;
        }

    }
}
