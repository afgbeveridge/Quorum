using System;
using FSM;

namespace Quorum.Integration.Http {

    public interface IHttpEventListener<TContext> {
        void Initialize();
        IStateMachine<TContext> Machine { get; set; }
        void UnInitialize();
    }

}
