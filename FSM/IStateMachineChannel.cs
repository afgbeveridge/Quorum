#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSM {

    public interface IStateMachineChannel<TContext> {
        Task Trigger(IEventInstance anEvent);
        Task RevertToPreviousState();
        bool HasPreviousState { get; }
        double UpTime { get; }
        double AbsoluteBootTime { get; }
        IEnumerable<IEventInstance> PendingEvents { get; }
        IEventStatistician StatisticsHandler { get; }
    }

}
