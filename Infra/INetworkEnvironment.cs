﻿#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Infra {
    
    public interface INetworkEnvironment {

        IPAddress LocalIPAddress { get; }

        string HostName { get; }

        long UniqueId { get; }

    }

}
