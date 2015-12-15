#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion

namespace FSM {

    public interface IMinimalContext {
        string HostName { get; }
        long NodeId { get; }
    }
}
