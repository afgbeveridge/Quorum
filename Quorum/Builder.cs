#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Linq;
using FSM;
using Quorum.States;
using Quorum.AOP;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Castle.DynamicProxy;
using Quorum.Integration;
using Quorum.Integration.Http;
using Quorum.Integration.Tcp;
using Quorum.Services;
using System.Reflection;
using Infra;

namespace Quorum {

    public class Builder {
        
        public IStateMachine<IExecutionContext> Create() {
            CreateBaseRegistrations();
            ConfigureInjections();
            return new SimpleStateMachine<IExecutionContext>(Container.Resolve<IEventQueue>(), Container.Resolve<IEventStatistician>())
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
                       .MarkAsBounceState()
                       // State
                       .DefineState<PretenderState>()
                       .MarkAsBounceState()
                       // Commit last state
                       .Complete()
                       // Allow query transition from all states
                       .ForAllEnter<QueryState>(EventNames.Query)
                       .ForAllEnter<DiscoveryState>(EventNames.Discovery)
                       .ForAllEnter<DeathState>(EventNames.Die)
                       .ForAllEnter<DeathAnnouncementState>(EventNames.NeighbourDying)
                       .ForAllEnter<AbdicationState>(EventNames.Abdication)
                       .ForAllEnter<MasterState>(EventNames.Elected)
                       .ForAllEnter<PretenderState>(EventNames.MakePretender)
                       .ForAllEnter<QuiescentState>(EventNames.Quiescent);

        }

        public IContainer AsContainer() {
            return new ContainerWrapper { Container = Container };
        }

        public void CreateBaseRegistrations() {
            Container = new WindsorContainer();
            Register<IPayloadParser, JsonPayloadParser>();
            Register<IPayloadBuilder, JsonPayloadBuilder>();
            RegisterChannelsAndListener();
            Register<IConfiguration, Configuration>();
            Register<INetworkEnvironment, SimpleNetworkEnvironment>();
            Register<ICommunicationsService, CommunicationsService>();
            Container.Kernel.AddHandlerSelector(new WriteableChannelSelector());
            Container.Kernel.AddHandlerSelector(new ReadableChannelSelector());
            Container.Kernel.AddHandlerSelector(new EventListenerSelector());
        }

        protected virtual void ConfigureInjections() {
            RegisterInterceptors();
            Register<IExecutionContext, ExecutionContext>();
            Register<IEventInterpreter<IExecutionContext>, EventInterpreter>();
            Register<IElectionAdjudicator, EarliestBootCandidateAdjudicator>();
            Register<IEventQueue, ConcurrentEventQueue>();
            Register<IEventStatistician, EventStatistician>();
            RegisterStates();
        }

        protected virtual void RegisterChannelsAndListener() {
            Register<IReadableChannel, HttpReadableChannel>();
            Register<IWriteableChannel, HttpWriteableChannel>();
            Register<IExposedEventListener<IExecutionContext>, HttpExposedEventListener>();
            Register<IReadableChannel, TcpReadableChannel>();
            Register<IWriteableChannel, TcpWriteableChannel>();
            Register<IExposedEventListener<IExecutionContext>, TcpExposedEventListener>();
        }

        private void RegisterInterceptors() {
            LoggingInterceptor.Interceptable("OnEntry", "OnExit");
            Container.Register(Component.For<IInterceptor>().ImplementedBy<LoggingInterceptor>());
        }

        public void Register<TInterface, TConcrete>() where TInterface : class where TConcrete : TInterface {
            Container.Register(Component.For<TInterface>().ImplementedBy<TConcrete>().LifeStyle.Transient);
        }

        public void Register<TInterface>(Type t)
            where TInterface : class {
            Container.Register(Component.For<TInterface>().ImplementedBy(t).LifeStyle.Transient);
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

        public virtual TType Resolve<TType>() {
            return Container.Resolve<TType>();
        }

        public IWindsorContainer Container { get; private set; }

        private class ContainerWrapper : IContainer {
            internal IWindsorContainer Container { get; set; }
            public TType Resolve<TType>(string name = null) where TType : class {
                return String.IsNullOrEmpty(name) ? Container.Resolve<TType>() : Container.Resolve<TType>(name);
            }
        }

    }

}
