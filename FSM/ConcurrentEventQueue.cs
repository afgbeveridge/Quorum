using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infra;

namespace FSM {

    public class ConcurrentEventQueue : IEventQueue {

        private ConcurrentQueue<IEventInstance> QueuedEvents = new ConcurrentQueue<IEventInstance>();

        public void Enqueue(IEventInstance instance) {
            LogFacade.Instance.LogInfo("Queue event: " + instance.EventName);
            QueuedEvents.Enqueue(instance);
            LogFacade.Instance.LogInfo("Queued events now " + QueuedEvents.Count + "(added " + instance.EventName + ")");
            LogFacade.Instance.LogInfo("Queued events waiting [" + (String.Join(", ", All.Select(e => e.EventName))) + "]");
        }

        public IEventInstance Dequeue() {
            IEventInstance anEvent;
            LogFacade.Instance.LogInfo("Trying to dequeue an event");
            if (QueuedEvents.TryDequeue(out anEvent))
                LogFacade.Instance.LogInfo("Dequeued: " + anEvent.EventName);
            return anEvent;
        }

        public bool Any() {
            return QueuedEvents.Any();
        }

        public IEnumerable<IEventInstance> All {
            get { return QueuedEvents.ToArray(); }
        }
    }

}
