using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSM;
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

namespace Quorum {

    public class Builder {
        
        public IStateMachine<IExecutionContext> Create() {
            ConfigureInjections();
            return new SimpleStateMachine<IExecutionContext>()
                        // Bootstrapping
                        .WithContext(Resolve<IExecutionContext>())
                        .DIContainer(AsContainer())
                        // State
                       .DefineState<DiscoveryState>()
                       .AsStartState()
                       .Singleton()
                       .On(EventNames.RequestElection)
                       .TransitionTo<ElectionState>()
                       .On(EventNames.Elected)
                       .TransitionTo<MasterState>()
                        // State
                       .DefineState<ElectionState>()
                       .Singleton()
                       .On(EventNames.ElectionResult)
                       .BeReflexive()
                       .On(EventNames.Elected)
                       .TransitionTo<MasterState>()
                       .On(EventNames.NotElected)
                       .TransitionTo<QuiescentState>()
                       .On(EventNames.ExternalRequestElection)
                       .BeReflexive()
                       .On(EventNames.VoteCast)
                       .BeReflexive()
                       .MarkAsBounceState()
                       // State
                       .DefineState<QuiescentState>()
                       .On(EventNames.RequestElection)
                       .TransitionTo<ElectionState>()
                       // State
                       .DefineState<MasterState>()
                       .On(EventNames.RequestElection)
                       .BeReflexive()
                        // State
                       .DefineState<QueryState>()
                       .MarkAsBounceState()
                        // State
                       .DefineState<DeathState>()
                       .MarkAsBounceState()
                       // State
                       .DefineState<AbdicationState>()
                       // Commit last state
                       .Complete()
                       // Allow query transition from all states
                       .ForAllEnter<QueryState>(EventNames.Query)
                       .ForAllEnter<DiscoveryState>(EventNames.Discovery)
                       .ForAllEnter<DeathState>(EventNames.Die)
                       .ForAllEnter<DeathAnnouncementState>(EventNames.NeighbourDying)
                       .ForAllEnter<AbdicationState>(EventNames.Abdication)
                       .ForAllEnter<MasterState>(EventNames.Elected)
                       .ForAllEnter<QuiescentState>(EventNames.Quiescent);

        }

        public IContainer AsContainer() {
            return new ContainerWrapper { Container = Container };
        }

        protected virtual void ConfigureInjections() {
            Container = new WindsorContainer();
            RegisterInterceptors();
            Register<IPayloadParser, JsonPayloadParser>();
            Register<IPayloadBuilder, JsonPayloadBuilder>();
            Register<IExecutionContext, ExecutionContext>();
            Register<IReadableChannel, HttpReadableChannel>();
            Register<IWriteableChannel, HttpWriteableChannel>();
            Register<IConfiguration, Configuration>();
            Register<IEventInterpreter<IExecutionContext>, EventInterpreter>();
            Register<IHttpEventListener<IExecutionContext>, HttpEventListener>();
            Register<INetworkEnvironment, SimpleNetworkEnvironment>();
            Register<IElectionAdjudicator, OldestCandidateAdjudicator>();
            Register<IDiscoveryService, DiscoveryService>();
            RegisterStates();
        }

        private void RegisterInterceptors() {
            LoggingInterceptor.Interceptable("OnEntry", "OnExit");
            Container.Register(Component.For<IInterceptor>().ImplementedBy<LoggingInterceptor>());
        }

        public void Register<TInterface, TConcrete>() where TInterface : class where TConcrete : TInterface {
            Container.Register(Component.For<TInterface>().ImplementedBy<TConcrete>().LifeStyle.Transient);
        }

        private void RegisterStates() {
            Action<Type, string> register = (t, n) => Container.Register(Component.For<IState<IExecutionContext>>().ImplementedBy(t).Named(n).Interceptors<LoggingInterceptor>());
            //Container.Register(AllTypes.FromThisAssembly().)
            Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(BaseState<IExecutionContext>)))
                .ToList()
                .ForEach(t => register(t, t.Name));
        }

        protected virtual TType Resolve<TType>() {
            return Container.Resolve<TType>();
        }

        private IWindsorContainer Container { get; set; }

        private class ContainerWrapper : IContainer {
            internal IWindsorContainer Container { get; set; }
            public TType Resolve<TType>(string name = null) where TType : class {
                return String.IsNullOrEmpty(name) ? Container.Resolve<TType>() : Container.Resolve<TType>(name);
            }
        }

    }

}
