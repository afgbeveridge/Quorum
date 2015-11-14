using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum.Payloads;
using Infra;

namespace Quorum.Integration {

    public class OldestCandidateAdjudicator : IElectionAdjudicator {

        public Neighbour Choose(IList<Neighbour> neighbours, Neighbour self) {
            LogFacade.Instance.LogInfo("Choosing a master, candidates = " + (neighbours.IsNull() ? 0 : neighbours.Count));
            var n = neighbours.IsNull() ? null : neighbours.Concat(new [] { self }).Where(_ => !_.InEligibleForElection).Aggregate((i, j) => i.UpTime > j.UpTime ? i : j);
            LogFacade.Instance.LogInfo("Candidate selected: " + (n.IsNull() ? "None - thus self" : n.Name));
            return n;
        }

    }
}
