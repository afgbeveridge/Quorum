#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Net;

namespace Infra {

    public interface INetworkEnvironment {

        IPAddress LocalIPAddress { get; }

        string HostName { get; }

        long UniqueId { get; }

        long DeriveUniqueId(string name);

        string SeedForUniqueId { get; }

        string NameForIPAddress(IPAddress address);

    }

}
