#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion

namespace FSM {

    public interface IContainer {
        TType Resolve<TType>(string name = null) where TType : class;
    }

}
