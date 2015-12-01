﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum.Payloads;

namespace Quorum.Services {
    
    public interface ICommunicationsService {

        IEnumerable<Neighbour> DiscoverExcept(string invokingHostName);

        IEnumerable<Neighbour> Query(IEnumerable<string> targets, bool includeNonResponders = false);

        IEnumerable<BasicMachine> VisibleComputers(bool workgroupOnly = false);

        bool RenderEligible(string name);

        bool RenderInEligible(string name);

    }
}