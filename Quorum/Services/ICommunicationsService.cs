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
using Quorum.Payloads;

namespace Quorum.Services {
    
    public interface ICommunicationsService {

        Task<IEnumerable<Neighbour>> DiscoverExcept(string invokingHostName);

        Task<IEnumerable<Neighbour>> Query(IEnumerable<string> targets, bool includeNonResponders = false);

        IEnumerable<BasicMachine> VisibleComputers(bool workgroupOnly = false);

        bool RenderEligible(string name);

        bool RenderInEligible(string name);

    }
}
