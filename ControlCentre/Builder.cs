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
using Quorum.States;
using Quorum.AOP;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Castle.DynamicProxy;
using Quorum.Integration;
using Quorum.Integration.Http;
using Quorum.Services;
using System.Reflection;
using Infra;

namespace ControlCentre {

    public static class Builder {

        public static void ConfigureInjections() {
            Container = new Quorum.Builder();
            Container.CreateBaseRegistrations();
        }

        public static TType Resolve<TType>() {
            return Container.Resolve<TType>();
        }

        private static Quorum.Builder Container { get; set; }

    }

}
