using System;
using System.Collections.Generic;
using NSubstitute;
using Castle.MicroKernel.Registration;

namespace Quorum.Tests {

    public class TestBuilder : Builder {

        public TestBuilder() : base(false) {
            Interjectors = new Dictionary<Type, Func<object>>();
        }

        public Dictionary<Type, Func<object>> Interjectors { get; set; }

        public override void Register<TInterface, TConcrete>() {
            Type type = typeof(TInterface);
            if (!Interjectors.ContainsKey(type))
                base.Register<TInterface, TConcrete>();
            else 
                Container.Register(Component.For<TInterface>().Instance(Interjectors[type]() as TInterface));
        }

    }
}
