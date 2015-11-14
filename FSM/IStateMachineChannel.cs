using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSM {

    public interface IStateMachineChannel<TContext> {
        Task Trigger(IEventInstance anEvent);
        Task RevertToPreviousState();
        bool HasPreviousState { get; }
        double UpTime { get; }
        IEnumerable<IEventInstance> PendingEvents { get; }
        IEventStatistician StatisticsHandler { get; }
    }

}
