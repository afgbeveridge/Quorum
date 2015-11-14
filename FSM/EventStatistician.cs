using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infra;

namespace FSM {
    
    public class EventStatistician : IEventStatistician {

        private Dictionary<string, IEventSummary> EventStats = new Dictionary<string, IEventSummary>();

        public IEnumerable<IEventSummary> HandledEvents {
            get { return EventStats.Select(kvp => kvp.Value); }
        }

        public IEnumerable<IEventSummary> HandledEventsInNameOrder {
            get { return HandledEvents.OrderBy(s => s.Name); }
        }

        public void NoteEvent(string name, bool handled = true) {
            IEventSummary summary = EventStats.ContainsKey(name) ? EventStats[name] : null;
            if (summary.IsNull()) {
                summary = new EventState { Name = name, Processed = handled };
                EventStats[name] = summary;
            }
            summary.Occurrences += 1;
        }

        public class EventState : IEventSummary {
            public string Name { get; set; }
            public long Occurrences { get; set; }
            public bool Processed { get; set; }
        }

    }
}
