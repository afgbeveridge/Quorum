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
            LogFacade.Log("Choosing a master, candidates = " + (neighbours.IsNull() ? 0 : neighbours.Count));
            var n = neighbours.IsNull() ? null : neighbours.Concat(new [] { self }).Aggregate((i, j) => i.UpTime > j.UpTime ? i : j);
            LogFacade.Log("Candidate selected: " + (n.IsNull() ? "None - thus self" : n.Name));
            return n;
        }

    }
}
