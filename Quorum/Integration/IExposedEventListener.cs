using System;
using FSM;
using System.Threading.Tasks;

namespace Quorum.Integration {

    public interface IExposedEventListener<TContext> {
        void Initialize();
        IStateMachine<TContext> Machine { get; set; }
        void UnInitialize();
    }

}
