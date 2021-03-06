﻿#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.ServiceProcess;

namespace QuorumWindowsService {

    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main() {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] { 
                new QuorumAwareWindowsService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
