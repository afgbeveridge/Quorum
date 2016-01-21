#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Collections.Generic;
using Quorum.Integration;
using System.Threading.Tasks;
using Quorum.Payloads;
using FSM;

namespace Quorum.Services {
    
    public interface ICommunicationsService {

        Task<IEnumerable<Neighbour>> DiscoverExcept(string invokingHostName, bool includeNonResponders = false);

        Task<IEnumerable<Neighbour>> Query(IEnumerable<string> targets, bool includeNonResponders = false);

        Task<BroadcastDiscoveryResult> BroadcastDiscovery(IEnumerable<string> targets, IEnumerable<string> possibleNeighbours);

        Task<Neighbour> QueryNeighbour(string name, IWriteableChannel channel = null);

        Task<IEnumerable<BasicMachine>> Ping(IEnumerable<string> targets, int timeoutMs);

        IEnumerable<BasicMachine> VisibleComputers(bool workgroupOnly = false);

        Task<AnalysisResult> Analyze(IContainer container, string name);

        Task<IEnumerable<object>> OfferConfiguration(IEnumerable<string> targets, IEnumerable<string> quorumMembers);

        bool RenderEligible(string name);

        bool RenderInEligible(string name);

    }
}
