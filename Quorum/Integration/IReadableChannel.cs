#region License
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

namespace Quorum.Integration {

    public interface IReadableChannel {

        Task<string> Read(string address, int timeoutMs);

        IReadableChannel NewInstance();

    }

}
