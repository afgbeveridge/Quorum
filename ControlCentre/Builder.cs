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
            Container = new WindsorContainer();
            Register<IPayloadParser, JsonPayloadParser>();
            Register<IPayloadBuilder, JsonPayloadBuilder>();
            Register<IReadableChannel, HttpReadableChannel>();
            Register<IWriteableChannel, HttpWriteableChannel>();
            Register<IConfiguration, Configuration>();
            Register<IDiscoveryService, DiscoveryService>();
            Register<INetworkEnvironment, SimpleNetworkEnvironment>();
        }

        private static void Register<TInterface, TConcrete>()
            where TInterface : class
            where TConcrete : TInterface {
            Container.Register(Component.For<TInterface>().ImplementedBy<TConcrete>().LifeStyle.Transient);
        }

        public static TType Resolve<TType>() {
            return Container.Resolve<TType>();
        }

        public static IWindsorContainer Container { get; private set; }

    }

}
