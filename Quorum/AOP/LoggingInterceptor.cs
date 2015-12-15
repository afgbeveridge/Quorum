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
using Castle.DynamicProxy;
using Infra;

namespace Quorum.AOP {
    
    public class LoggingInterceptor : IInterceptor {

        private static readonly List<string> KnownNames = new List<string>();

        public static void Interceptable(params string[] names) {
            KnownNames.AddRange(names);
        }

        public void Intercept(IInvocation invocation) {
            if (!KnownNames.Any() || KnownNames.Contains(invocation.Method.Name))
                LogFacade.Instance.LogDebug(invocation.InvocationTarget.GetType().Name + ",Execute," + invocation.Method.Name);
            invocation.Proceed();
        }

    }
}
