#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Linq;
using Infra;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Quorum.Integration {

    public class SimpleNetworkEnvironment : INetworkEnvironment {

        private const int ChunkSize = 8;

        public IPAddress LocalIPAddress {
            get {
                return Dns
                        .GetHostEntry(HostName)
                        .AddressList
                        .First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            }
        }

        public string HostName {
            get {
                return Dns.GetHostName();
            }
        }

        public long UniqueId {
            get {
                string hexValue = string.Empty;
                using (MD5 alg = MD5.Create()) {
                    var hash = alg.ComputeHash(Encoding.ASCII.GetBytes(HostName));
                    hexValue = string.Join(string.Empty, hash.Select(b => b.ToString("x2")).ToArray());
                }
                return hexValue
                            .Chunk(ChunkSize)
                            .Select(arr => new string(arr.ToArray()))
                            .Sum(s => long.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier));
            }
        }
    }
}
