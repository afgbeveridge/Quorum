#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
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
