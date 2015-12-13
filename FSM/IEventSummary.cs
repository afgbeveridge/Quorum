#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion

namespace FSM {

    public interface IEventSummary {
        string Name { get; }
        long Occurrences { get; set; }
        bool Processed { get; set; }
    }

}
