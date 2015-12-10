#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Threading.Tasks;

namespace Quorum {

    public interface IMasterWorkAdapter {

        Action WorkUnitExecuted { get; set; }

        Task Activated();

        Task DeActivated();

    }

}
