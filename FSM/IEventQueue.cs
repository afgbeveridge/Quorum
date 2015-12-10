#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSM {
    
    public interface IEventQueue {
        void Enqueue(IEventInstance instance);
        IEventInstance Dequeue();
        bool Any();
        IEnumerable<IEventInstance> All { get; }
    }

}
