#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion

namespace FSM {

    public interface IEventInterpreter<TContext> {
        InterpreterResult<TContext> TranslateToAction(object inbound);
    }

}
