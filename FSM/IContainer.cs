#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion

using System.Collections.Generic;

namespace FSM {

    public interface IContainer {
        TType Resolve<TType>(string name = null) where TType : class;
        IEnumerable<TType> ResolveAll<TType>() where TType : class;
    }

}
