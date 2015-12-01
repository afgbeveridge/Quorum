using System;
using FSM;

namespace Quorum.Integration {

    public interface IExposedEventListener<TContext> {
        void Initialize();
        IStateMachine<TContext> Machine { get; set; }
        void UnInitialize();
    }

}
