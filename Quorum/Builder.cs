﻿#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
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

        public Builder(bool requireTimer = true) {
            RequireTimer = requireTimer;
        }

        private bool RequireTimer { get; set; }

        public IStateMachine<IExecutionContext> CreateEmpty()  {
            CreateBaseRegistrations();
            ConfigureInjections();
            return new SimpleStateMachine<IExecutionContext>(Container.Resolve<IEventQueue>(), Container.Resolve<IEventStatistician>(), RequireTimer);

        }

        public Func<Type, Type, Type> TypeMorpher { get; set; }

        public IStateMachine<IExecutionContext> Create() {
            return CreateEmpty()
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
                       // State
                       .DefineState<PendingConfigurationState>()
                       // State
                       .DefineState<ReceivingConfigurationState>()
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
                       .ForAllEnter<QuiescentState>(EventNames.Quiescent)
                       .ForAllEnter<ReceivingConfigurationState>(EventNames.ConfigurationOffered);

        }

        public IContainer AsContainer() {
            return new ContainerWrapper { Container = Container };
        }

        public virtual void CreateBaseRegistrations() {
            Container = new WindsorContainer();
            Register<IPayloadParser, JsonPayloadParser>();
            Register<IPayloadBuilder, JsonPayloadBuilder>();
            RegisterChannelsAndListener();
            Register<IConfigurationOverrideStorage, StaticConfigurationOverrideStorage>();
            Register<IConfigurationOverrideStorage, TransientConfigurationOverrideStorage>();
            Register<IConfiguration, Configuration>();
            Register<INetworkEnvironment, SimpleNetworkEnvironment>();
            Register<ICommunicationsService, CommunicationsService>();
            Register<ISecurityService, SecurityService>();
            Container.Kernel.AddHandlerSelector(new WriteableChannelSelector());
            Container.Kernel.AddHandlerSelector(new ReadableChannelSelector());
            Container.Kernel.AddHandlerSelector(new EventListenerSelector());
            Container.Kernel.AddHandlerSelector(new LocalStorageSelector());
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

        protected virtual void RegisterInterceptors() {
            LoggingInterceptor.Interceptable("OnEntry", "OnExit");
            Container.Register(Component.For<IInterceptor>().ImplementedBy<LoggingInterceptor>());
        }

        public virtual void Register<TInterface, TConcrete>() where TInterface : class where TConcrete : TInterface {
            if (TypeMorpher.IsNull())
                Container.Register(Component.For<TInterface>().ImplementedBy<TConcrete>().LifeStyle.Transient);
            else
                Register<TInterface>(TypeMorpher(typeof(TInterface), typeof(TConcrete)));
        }

        public virtual void Register<TInterface>(Type t)
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

        public void Destroy() {
            if (Container.IsNotNull())
                Container.Dispose();
        }

        private class ContainerWrapper : IContainer {
            internal IWindsorContainer Container { get; set; }
            public TType Resolve<TType>(string name = null) where TType : class {
                return String.IsNullOrEmpty(name) ? Container.Resolve<TType>() : Container.Resolve<TType>(name);
            }

            public IEnumerable<TType> ResolveAll<TType>() where TType : class {
                return Container.ResolveAll<TType>().Cast<TType>();
            }
        }

    }

}
